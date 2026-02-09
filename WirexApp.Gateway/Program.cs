using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Grpc.Net.ClientFactory;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;
using Serilog.Events;
using WirexApp.Gateway.Services;
using WirexApp.Gateway.Grpc;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "WirexApp-Gateway")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] [Gateway] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        "logs/gateway-.log",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting WirexApp API Gateway");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // Add configuration
    builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

    // Add services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Ocelot
    builder.Services.AddOcelot(builder.Configuration);

    // gRPC Clients
    var writeServiceUrl = builder.Configuration["GrpcServices:WriteService"] ?? "http://localhost:5011";
    var readServiceUrl = builder.Configuration["GrpcServices:ReadService"] ?? "http://localhost:5012";

    builder.Services.AddGrpcClient<WirexApp.Gateway.Grpc.PaymentWriteService.PaymentWriteServiceClient>(options =>
    {
        options.Address = new Uri(writeServiceUrl);
    });

    builder.Services.AddGrpcClient<WirexApp.Gateway.Grpc.PaymentReadService.PaymentReadServiceClient>(options =>
    {
        options.Address = new Uri(readServiceUrl);
    });

    // Custom Gateway Services
    builder.Services.AddScoped<PaymentGatewayService>();

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Gateway is healthy"));

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    var app = builder.Build();

    app.UseCors("AllowAll");

    app.UseRouting();

    app.MapControllers();
    app.MapHealthChecks("/health");

    // Use Ocelot for HTTP routing (skip in Testing environment for integration tests)
    if (app.Environment.EnvironmentName != "Testing")
    {
        await app.UseOcelot();
        Log.Information("Ocelot API Gateway middleware enabled");
    }
    else
    {
        Log.Information("Ocelot skipped in Testing environment - using direct controller routing");
    }

    Log.Information("API Gateway started successfully on {Urls}", string.Join(", ", app.Urls));
    app.Run();
}
catch (System.Exception ex)
{
    Log.Fatal(ex, "API Gateway terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make the implicit Program class public for WebApplicationFactory
public partial class Program { }
