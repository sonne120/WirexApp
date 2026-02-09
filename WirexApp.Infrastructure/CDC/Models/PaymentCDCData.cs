using System;

namespace WirexApp.Infrastructure.CDC.Models
{
    public class PaymentCDCData
    {
        public Guid PaymentId { get; set; }

        public Guid UserAccountId { get; set; }

        public Guid UserId { get; set; }

        public string SourceCurrency { get; set; }

        public string TargetCurrency { get; set; }

        public decimal SourceValue { get; set; }

        public decimal TargetValue { get; set; }

        public string Status { get; set; }

        public DateTime CreateDate { get; set; }

        public bool IsRemoved { get; set; }

        public bool IsEmailNotificationSent { get; set; }

        public decimal ExchangeRate { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        // Metadata
        public int Version { get; set; }

        public DateTime CapturedAt { get; set; }

        public PaymentCDCData()
        {
            CapturedAt = DateTime.UtcNow;
        }
    }
}
