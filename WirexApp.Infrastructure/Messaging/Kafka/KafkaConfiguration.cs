namespace WirexApp.Infrastructure.Messaging.Kafka
{
    public class KafkaConfiguration
    {
        public string BootstrapServers { get; set; }

        public string ClientId { get; set; }

        public string GroupId { get; set; }

        public bool EnableAutoCommit { get; set; } = false;

        public int SessionTimeoutMs { get; set; } = 10000;
    }
}
