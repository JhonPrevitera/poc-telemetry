using POC_SpanOpenTelemtry.Common.Helpers;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace POC_SpanOpenTelemetry.Core.Services.Config
{
    public class KafkaConsumerConfig(ILogger<KafkaService> logger)
    {
        private readonly string _topic = Environment.GetEnvironmentVariable("TOPIC")!;

        private readonly ConsumerConfig _config = new()
        {
            GroupId = Environment.GetEnvironmentVariable("GROUPID"),
            ClientId = Environment.GetEnvironmentVariable("CLIENTID"),
            BootstrapServers = Environment.GetEnvironmentVariable("BOOTSTRAPSERVERS"),
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
            logger?.LogInformation(
                $"ASSIGNED PARTITIONS: [{string.Join(",", partitions.Select(p => $"TOPIC:\"{p.Topic}\" PARTITION: {p.Partition.Value}"))}]");
        }

        private void ConsumerOnPartitionsRevokedHandler(IConsumer<string, string> sender,
            List<TopicPartitionOffset> topicPartitionOffsets)
        {
            logger?.LogInformation(
                $"REVOKED PARTITIONS: [{string.Join(",", topicPartitionOffsets.Select(p => $"TOPIC:\"{p.Topic}\" PARTITION:{p.Partition.Value}"))}]");
        }
    }
}