using Confluent.Kafka;

namespace POC_SpanOpenTelemtry.Core.Services.Contracts;

public interface IKafkaTopicConsumer
{
    public void ProduceMessage(string topic, string message, string traceparent);
    public void ConsumerKafka(Action<ConsumeResult<string, string>?> action);
    public Message<string, string> Consumer();
}