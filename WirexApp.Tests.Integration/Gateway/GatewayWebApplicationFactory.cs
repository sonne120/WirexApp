using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WirexApp.Gateway.Services;
using GrpcTypes = WirexApp.Gateway.Grpc;

namespace WirexApp.Tests.Integration.Gateway
{
    public class GatewayWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.Sources.Clear();

                var testConfig = new Dictionary<string, string?>
                {
                    ["GrpcServices:WriteService"] = "http://localhost:5011",
                    ["GrpcServices:ReadService"] = "http://localhost:5012",
                    ["GlobalConfiguration:BaseUrl"] = "http://localhost"
                };

                config.AddInMemoryCollection(testConfig);
            });

            builder.ConfigureTestServices(services =>
            {
                // Remove the real PaymentGatewayService
                services.RemoveAll<PaymentGatewayService>();

                // Add our test implementation as a singleton so state persists across requests
                services.AddSingleton<TestPaymentGatewayService>();
                services.AddScoped<PaymentGatewayService>(sp =>
                    sp.GetRequiredService<TestPaymentGatewayService>());
            });
        }
    }

    /// <summary>
    /// Test implementation of PaymentGatewayService that provides in-memory functionality
    /// without requiring actual gRPC services
    /// </summary>
    public class TestPaymentGatewayService : PaymentGatewayService
    {
        private readonly List<GrpcTypes.PaymentModel> _payments = new();

        public TestPaymentGatewayService()
            : base(null!, null!, new LoggerFactory().CreateLogger<PaymentGatewayService>())
        {
        }

        public override Task<GrpcTypes.CreatePaymentResponse> CreatePaymentAsync(
            string userId,
            int sourceCurrency,
            int targetCurrency,
            double sourceValue)
        {
            // Validate business rules
            if (sourceValue <= 0)
            {
                return Task.FromResult(new GrpcTypes.CreatePaymentResponse
                {
                    Success = false,
                    Message = "Amount must be greater than zero"
                });
            }

            if (sourceCurrency == targetCurrency)
            {
                return Task.FromResult(new GrpcTypes.CreatePaymentResponse
                {
                    Success = false,
                    Message = "Source and target currencies must be different"
                });
            }

            // Create payment
            var payment = new GrpcTypes.PaymentModel
            {
                PaymentId = Guid.NewGuid().ToString(),
                UserId = userId,
                SourceCurrency = sourceCurrency.ToString(),
                TargetCurrency = targetCurrency.ToString(),
                SourceValue = sourceValue,
                TargetValue = sourceValue * 1.1,
                Status = "Completed"
            };

            _payments.Add(payment);

            return Task.FromResult(new GrpcTypes.CreatePaymentResponse
            {
                Success = true,
                Message = "Payment created successfully",
                UserId = userId
            });
        }

        public override Task<GrpcTypes.PaymentModel?> GetPaymentAsync(string paymentId)
        {
            var payment = _payments.FirstOrDefault(p => p.PaymentId == paymentId);
            return Task.FromResult(payment);
        }

        public override Task<List<GrpcTypes.PaymentModel>> GetAllPaymentsAsync()
        {
            return Task.FromResult(_payments.ToList());
        }

        public override Task<List<GrpcTypes.PaymentModel>> GetPaymentsByUserAsync(string userId)
        {
            var payments = _payments.Where(p => p.UserId == userId).ToList();
            return Task.FromResult(payments);
        }

        public override Task<GrpcTypes.GetPaymentStatsResponse> GetPaymentStatsAsync()
        {
            var stats = new GrpcTypes.GetPaymentStatsResponse
            {
                TotalPayments = _payments.Count,
                PendingPayments = _payments.Count(p => p.Status == "Pending"),
                CompletedPayments = _payments.Count(p => p.Status == "Completed"),
                TotalAmount = _payments.Sum(p => p.SourceValue)
            };

            return Task.FromResult(stats);
        }
    }
}
