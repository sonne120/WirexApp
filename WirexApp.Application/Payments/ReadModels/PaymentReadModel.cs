using System;

namespace WirexApp.Application.Payments.ReadModels
{
    public class PaymentReadModel
    {
        public Guid PaymentId { get; set; }

        public Guid UserAccountId { get; set; }

        public Guid UserId { get; set; }

        public string UserName { get; set; }

        public string UserEmail { get; set; }

        public string SourceCurrency { get; set; }

        public string TargetCurrency { get; set; }

        public decimal SourceValue { get; set; }

        public decimal TargetValue { get; set; }

        public string Status { get; set; }

        public DateTime CreateDate { get; set; }

        public bool IsRemoved { get; set; }

        public bool IsEmailNotificationSent { get; set; }

        // Denormalized fields for performance
        public decimal ExchangeRate { get; set; }

        public string StatusDescription { get; set; }

        public DateTime? LastModifiedDate { get; set; }
    }
}
