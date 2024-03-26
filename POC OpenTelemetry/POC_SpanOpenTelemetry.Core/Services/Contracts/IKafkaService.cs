using Confluent.Kafka;

namespace POC_SpanOpenTelemetry.Core.Services.Contracts;

public interface IKafkaService
{
    public void ProduceMessage(string topic, string message, string traceparent);
    public void ConsumerKafka(Action<ConsumeResult<string, string>?> action);
    public Message<string, string> Consumer();
}