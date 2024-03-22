using Common;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace POC_SpanOpenTelemtry.Core.Config
{
    public class KafkaConsumerConfig
    {
        private readonly string _topic = Environment.GetEnvironmentVariable("TOPIC")!;
        private readonly ILogger<KafkaService> _logger;

        public KafkaConsumerConfig(ILogger<KafkaService> logger)
        {
            _logger = logger;
        }

        private readonly ConsumerConfig _config = new()
        {
            GroupId = Environment.GetEnvironmentVariable("GROUPID"),
            ClientId = Environment.GetEnvironmentVariable("CLIENTID"),
            BootstrapServers = Environment.GetEnvironmentVariable("BOOTSTRAPSERVES"),
            EnableAutoCommit = true,
            AllowAutoCreateTopics = false,
            EnableSslCertificateVerification = false,
        };

        public IConsumer<string, string> Build()
        {
            var consumer = new ConsumerBuilder<string, string>(_config)
                .SetStatisticsHandler((_, json) => Console.WriteLine($"Statics:{json}"))
                .SetValueDeserializer(new StringDeserializer()!)
                .SetPartitionsAssignedHandler(ConsumerOnPartitionsAssigned)
                .SetPartitionsRevokedHandler(ConsumerOnPartitionsRevokedHandler)
                .Build();
            consumer.Subscribe(_topic);
            return consumer;
        }

        private void ConsumerOnPartitionsAssigned(IConsumer<string, string> sender, List<TopicPartition> partitions)
        {
            _logger?.LogInformation(
                $"ASSIGNED PARTITIONS: [{string.Join(",", partitions.Select(p => $"TOPIC:\"{p.Topic}\" PARTITION: {p.Partition.Value}"))}]");
        }

        private void ConsumerOnPartitionsRevokedHandler(IConsumer<string, string> sender,
            List<TopicPartitionOffset> topicPartitionOffsets)
        {
            _logger?.LogInformation(
                $"REVOKED PARTITIONS: [{string.Join(",", topicPartitionOffsets.Select(p => $"TOPIC:\"{p.Topic}\" PARTITION:{p.Partition.Value}"))}]");
        }
    }
}