using System;
using WirexApp.Domain;

namespace WirexApp.Application.Payments
{
    public class PaymentDto
    {
        public Guid PaymentId { get; set; }

        public Guid UserAccountId { get; set; }

        public string SourceCurrency { get; set; }

        public string TargetCurrency { get; set; }

        public decimal SourceValue { get; set; }

        public decimal TargetValue { get; set; }

        public string Status { get; set; }

        public DateTime CreateDate { get; set; }

        public bool IsRemoved { get; set; }
    }
}
