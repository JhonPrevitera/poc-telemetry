using Confluent.Kafka;
using POC_SpanOpenTelemtry.Core.Services.Contracts;

namespace POC_SpanOpenTelemtry.Core
{
    public class KafkaService: IKafkaService
    {
        public void ProduceMessage(string topic, string message, string traceparent)
        {
            throw new NotImplementedException();
        }

        public void ConsumerKafka(Action<ConsumeResult<string, string>?> action)
        {
            throw new NotImplementedException();
        }

        public Message<string, string> Consumer()
        {
            throw new NotImplementedException();
        }
    }
}