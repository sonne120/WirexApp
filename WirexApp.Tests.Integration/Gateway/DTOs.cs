using System;
using System.Collections.Generic;

namespace WirexApp.Tests.Integration.Gateway
{
    public class CreatePaymentResponse
    {
        public string Message { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
    }

    public class PaymentDto
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

    public class PaymentStatsDto
    {
        public int TotalPayments { get; set; }
        public double TotalVolume { get; set; }
        public Dictionary<string, int> PaymentsByCurrency { get; set; } = new();
        public Dictionary<string, int> PaymentsByStatus { get; set; } = new();
    }
}
