using Asp.Versioning;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System;
using System.Text;
using System.Threading.RateLimiting;
using WirexApp.Application;
using WirexApp.Application.Configuration;
using WirexApp.Infrastructure.CDC;
using WirexApp.Infrastructure.DataAccess;
using WirexApp.Infrastructure.Messaging;
using WirexApp.Infrastructure.Messaging.Kafka;
using WirexApp.Infrastructure.Outbox;
using WirexApp.Infrastructure.UnitOfWork;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "WirexApp-WriteService")
    .Enrich.WithProperty("ServiceType", "Write")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] [WriteService] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        "logs/write-service-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting WirexApp Write Service (Command Side)");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // Use Autofac
    builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
    builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
    {
        containerBuilder.RegisterModule<DataAccessModule>();
        containerBuilder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerLifetimeScope();
    });

    // API Versioning
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // gRPC
    builder.Services.AddGrpc();

    // MediatR for Commands
    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(ICommand).Assembly);
    });

    // FluentValidation
    builder.Services.AddValidatorsFromAssembly(typeof(ICommand).Assembly);
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

    // JWT Authentication
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyForDevelopmentOnly123456789ThisShouldBeAtLeast32Characters";

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"] ?? "WirexApp",
            ValidAudience = jwtSettings["Audience"] ?? "WirexApp",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

    builder.Services.AddAuthorization();


    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<Microsoft.AspNetCore.Http.HttpContext, string>(context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                factory: partition => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 100,
                    QueueLimit = 0,
                    Window = TimeSpan.FromMinutes(1)
                }));
    });
    
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Write Service is healthy"));

    builder.Services.AddMemoryCache();
    
    var kafkaConfig = new KafkaConfiguration
    {
        BootstrapServers = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
        ClientId = builder.Configuration["Kafka:ClientId"] ?? "WirexApp-WriteService",
        GroupId = builder.Configuration["Kafka:GroupId"] ?? "WirexApp-write-consumer-group"
    };

    builder.Services.AddSingleton(kafkaConfig);
    builder.Services.AddSingleton<IMessageBus, KafkaMessageBus>();
    
    builder.Services.AddSingleton<IOutboxRepository, InMemoryOutboxRepository>();
    builder.Services.AddSingleton<ICDCEventPublisher, CDCEventPublisherWithOutbox>();

    // Outbox Processor - publishes CDC events from Outbox to Kafka
    var cdcEnabled = builder.Configuration.GetValue<bool>("CDC:Enabled", true);
    if (cdcEnabled)
    {
        builder.Services.AddHostedService<OutboxProcessor>();
        Log.Information("Outbox Processor enabled for CDC event publishing");
    }
    
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "WirexApp Write Service API",
            Version = "v1",
            Description = "Command Side - Handles all write operations (Create, Update, Delete)"
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policyBuilder =>
        {
            policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        });
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Write Service API v1");
            c.RoutePrefix = string.Empty;
        });
    }

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();

    app.MapControllers();
    app.MapHealthChecks("/health");

    // Map gRPC Service
    app.MapGrpcService<WirexApp.WriteService.Grpc.PaymentWriteGrpcService>();

    Log.Information("WirexApp Write Service started successfully on {Urls}", string.Join(", ", app.Urls));
    Log.Information("gRPC Service enabled on port 5011");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Write Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
