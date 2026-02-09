using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System;
using System.Text;
using WirexApp.Application;
using WirexApp.Infrastructure.CDC.Consumers;
using WirexApp.Infrastructure.DataAccess.Read;
using WirexApp.Infrastructure.Messaging;
using WirexApp.Infrastructure.Messaging.Kafka;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "WirexApp-ReadService")
    .Enrich.WithProperty("ServiceType", "Read")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] [ReadService] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        "logs/read-service-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting WirexApp Read Service (Query Side)");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

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

    // MediatR for Queries
    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(IQuery<>).Assembly);
    });

    // JWT Authentication (Read-only validation)
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

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Read Service is healthy"));

    // Memory Cache for read models
    builder.Services.AddMemoryCache();
    builder.Services.AddResponseCaching();

    // Kafka Configuration (for CDC consumers)
    var kafkaConfig = new KafkaConfiguration
    {
        BootstrapServers = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
        ClientId = builder.Configuration["Kafka:ClientId"] ?? "WirexApp-ReadService",
        GroupId = builder.Configuration["Kafka:GroupId"] ?? "WirexApp-read-consumer-group"
    };

    builder.Services.AddSingleton(kafkaConfig);
    builder.Services.AddSingleton<IMessageBus, KafkaMessageBus>();

    // Read Services
    builder.Services.AddSingleton<PaymentReadService>();

    // CDC Consumers - Update read models from write-side events
    var cdcEnabled = builder.Configuration.GetValue<bool>("CDC:Enabled", true);
    if (cdcEnabled)
    {
        builder.Services.AddHostedService<PaymentCDCConsumer>();
        Log.Information("CDC Consumer enabled for read model synchronization");
    }

    // Swagger
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "WirexApp Read Service API",
            Version = "v1",
            Description = "Query Side - Handles all read operations (GET requests only)"
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
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Read Service API v1");
            c.RoutePrefix = string.Empty;
        });
    }

    app.UseHttpsRedirection();
    app.UseResponseCaching();
    app.UseRouting();
    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    // Map gRPC Service
    app.MapGrpcService<WirexApp.ReadService.Grpc.PaymentReadGrpcService>();

    Log.Information("WirexApp Read Service started successfully on {Urls}", string.Join(", ", app.Urls));
    Log.Information("gRPC Service enabled on port 5012");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Read Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
