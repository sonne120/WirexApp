using System;

namespace WirexApp.Infrastructure.Outbox
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }

        public string EntityType { get; set; }

        public string EntityId { get; set; }

        public string EventType { get; set; }

        public string Payload { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ProcessedAt { get; set; }

        public OutboxMessageStatus Status { get; set; }

        public int RetryCount { get; set; }

        public string ErrorMessage { get; set; }

        public string Topic { get; set; }

        public OutboxMessage()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            Status = OutboxMessageStatus.Pending;
            RetryCount = 0;
        }
    }

    public enum OutboxMessageStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3
    }
}
