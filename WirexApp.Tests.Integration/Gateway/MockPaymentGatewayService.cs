using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WirexApp.Tests.Integration.Gateway
{
    // Mock implementation of PaymentGatewayService for testing
    public class MockPaymentGatewayService
    {
        private readonly List<MockPayment> _payments = new();
        private int _paymentCounter = 0;

        public Task<CreatePaymentResult> CreatePaymentAsync(
            string userId,
            int sourceCurrency,
            int targetCurrency,
            double sourceValue)
        {
            // Validate business rules
            if (sourceValue <= 0)
            {
                return Task.FromResult(new CreatePaymentResult
                {
                    Success = false,
                    Message = "Amount must be greater than zero"
                });
            }

            if (sourceCurrency == targetCurrency)
            {
                return Task.FromResult(new CreatePaymentResult
                {
                    Success = false,
                    Message = "Source and target currencies must be different"
                });
            }

            // Create payment
            var payment = new MockPayment
            {
                PaymentId = Guid.NewGuid().ToString(),
                UserId = userId,
                SourceCurrency = sourceCurrency,
                TargetCurrency = targetCurrency,
                SourceValue = sourceValue,
                TargetValue = sourceValue * 1.1, // Mock exchange rate
                Status = "Completed",
                CreatedAt = DateTime.UtcNow
            };

            _payments.Add(payment);
            _paymentCounter++;

            return Task.FromResult(new CreatePaymentResult
            {
                Success = true,
                Message = "Payment created successfully",
                UserId = userId
            });
        }

        public Task<MockPayment?> GetPaymentAsync(string paymentId)
        {
            var payment = _payments.FirstOrDefault(p => p.PaymentId == paymentId);
            return Task.FromResult(payment);
        }

        public Task<List<MockPayment>> GetAllPaymentsAsync()
        {
            return Task.FromResult(_payments.ToList());
        }

        public Task<List<MockPayment>> GetPaymentsByUserAsync(string userId)
        {
            var payments = _payments.Where(p => p.UserId == userId).ToList();
            return Task.FromResult(payments);
        }

        public Task<MockPaymentStats> GetPaymentStatsAsync()
        {
            var stats = new MockPaymentStats
            {
                TotalPayments = _payments.Count,
                TotalVolume = _payments.Sum(p => p.SourceValue),
                PaymentsByCurrency = new Dictionary<string, int>
                {
                    ["USD"] = _payments.Count(p => p.SourceCurrency == 1),
                    ["EUR"] = _payments.Count(p => p.SourceCurrency == 2)
                },
                PaymentsByStatus = new Dictionary<string, int>
                {
                    ["Completed"] = _payments.Count(p => p.Status == "Completed"),
                    ["Pending"] = _payments.Count(p => p.Status == "Pending")
                }
            };

            return Task.FromResult(stats);
        }
    }

    public class CreatePaymentResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    public class MockPayment
    {
        public string PaymentId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public int SourceCurrency { get; set; }
        public int TargetCurrency { get; set; }
        public double SourceValue { get; set; }
        public double TargetValue { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class MockPaymentStats
    {
        public int TotalPayments { get; set; }
        public double TotalVolume { get; set; }
        public Dictionary<string, int> PaymentsByCurrency { get; set; } = new();
        public Dictionary<string, int> PaymentsByStatus { get; set; } = new();
    }
}
