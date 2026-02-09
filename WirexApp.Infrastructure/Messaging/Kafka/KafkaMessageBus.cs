using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace WirexApp.Infrastructure.Messaging.Kafka
{
    public class KafkaMessageBus : IMessageBus, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaMessageBus> _logger;

        public KafkaMessageBus(KafkaConfiguration configuration, ILogger<KafkaMessageBus> logger)
        {
            _logger = logger;

            var config = new ProducerConfig
            {
                BootstrapServers = configuration.BootstrapServers,
                ClientId = configuration.ClientId,
                Acks = Acks.All,
                EnableIdempotence = true,
                MaxInFlight = 5,
                CompressionType = CompressionType.Snappy,
                LingerMs = 10,
                BatchSize = 32768,
                MessageSendMaxRetries = 3
            };

            _producer = new ProducerBuilder<string, string>(config)
                .SetErrorHandler((_, error) =>
                {
                    _logger.LogError($"Kafka error: {error.Reason}");
                })
                .Build();
        }

        public async Task PublishAsync<T>(string topic, T message) where T : class
        {
            await PublishAsync(topic, Guid.NewGuid().ToString(), message);
        }

        public async Task PublishAsync<T>(string topic, string key, T message) where T : class
        {
            try
            {
                var serializedMessage = JsonConvert.SerializeObject(message, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });

                var kafkaMessage = new Message<string, string>
                {
                    Key = key,
                    Value = serializedMessage,
                    Timestamp = Timestamp.Default
                };

                var result = await _producer.ProduceAsync(topic, kafkaMessage);

                _logger.LogInformation(
                    $"Message published to Kafka: Topic={topic}, Partition={result.Partition}, Offset={result.Offset}");
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, $"Failed to publish message to Kafka topic: {topic}");
                throw;
            }
        }

        public void Dispose()
        {
            _producer?.Flush(TimeSpan.FromSeconds(10));
            _producer?.Dispose();
        }
    }
}
