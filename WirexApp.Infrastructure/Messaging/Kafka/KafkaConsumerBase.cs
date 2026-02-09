using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WirexApp.Infrastructure.Messaging.Kafka
{
    public abstract class KafkaConsumerBase<T> : BackgroundService where T : class
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly ILogger _logger;
        private readonly string _topic;

        protected KafkaConsumerBase(
            KafkaConfiguration configuration,
            string topic,
            ILogger logger)
        {
            _topic = topic;
            _logger = logger;

            var config = new ConsumerConfig
            {
                BootstrapServers = configuration.BootstrapServers,
                GroupId = configuration.GroupId,
                ClientId = configuration.ClientId,
                EnableAutoCommit = configuration.EnableAutoCommit,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                SessionTimeoutMs = configuration.SessionTimeoutMs,
                EnablePartitionEof = true
            };

            _consumer = new ConsumerBuilder<string, string>(config)
                .SetErrorHandler((_, error) =>
                {
                    _logger.LogError($"Kafka consumer error: {error.Reason}");
                })
                .Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(_topic);

            _logger.LogInformation($"Kafka consumer started for topic: {_topic}");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(stoppingToken);

                        if (consumeResult?.Message == null)
                            continue;

                        var message = JsonConvert.DeserializeObject<T>(
                            consumeResult.Message.Value,
                            new JsonSerializerSettings
                            {
                                TypeNameHandling = TypeNameHandling.All
                            });

                        if (message != null)
                        {
                            await HandleMessageAsync(message, stoppingToken);

                            _consumer.Commit(consumeResult);

                            _logger.LogInformation(
                                $"Message processed from Kafka: Topic={_topic}, Partition={consumeResult.Partition}, Offset={consumeResult.Offset}");
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, $"Error consuming message from topic: {_topic}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing message from topic: {_topic}");
                    }
                }
            }
            finally
            {
                _consumer.Close();
                _consumer.Dispose();
            }
        }

        protected abstract Task HandleMessageAsync(T message, CancellationToken cancellationToken);

        public override void Dispose()
        {
            _consumer?.Dispose();
            base.Dispose();
        }
    }
}
