using System.Diagnostics;
using System.Text;
using Confluent.Kafka;
using Confluent.Kafka.Extensions.Diagnostics;
using Microsoft.Extensions.Logging;
using POC_SpanOpenTelemetry.Core.Services.Config;
using POC_SpanOpenTelemetry.Core.Services.Contracts;

namespace POC_SpanOpenTelemetry.Core.Services
{
    public class KafkaService(ILogger<KafkaService> logger) : IKafkaService
    {
        private readonly IConsumer<string, string> _consumer = new KafkaConsumerConfig(logger).Build();
        private readonly IProducer<string, string> _producer = new KafkaProducerConfig().Build();
        private readonly ErrorCode _errorCode = new();
        private readonly ActivitySource _activity = new(nameof(KafkaService));

        public void ProduceMessage(string topic, string message, string traceParent)
        {
            using var activityProduce = _activity.StartActivity();
            try
            {
                var data = Encoding.UTF8.GetBytes(traceParent);
                var headers = new Headers()
                {
                    new Header("traceparent", data)
                };
                _producer.ProduceAsync(topic,
                    new Message<string, string> { Key = "", Value = message, Headers = headers }).Wait();
            }
            catch (KafkaException e)
            {
                logger.LogError($"{_errorCode},{e}");
                activityProduce?.SetTag("otel.status_code", "ERROR");
                activityProduce?.SetTag("otel.status_description", e.Message);
                activityProduce?.SetStatus(ActivityStatusCode.Error);
            }
        }

        public void ConsumerKafka(Action<ConsumeResult<string, string>?> action)
        {
            _consumer.ConsumeWithInstrumentation(action, 300000);
        }

        public Message<string, string> Consumer()
        {
            try
            {
                var message = _consumer.Consume();
                return message.Message;
            }
            catch (ConsumeException e)
            {
                logger.LogError($"{_errorCode}, {e}");
                using var activityErrorConsumer = _activity.StartActivity();
                activityErrorConsumer?.SetStatus(ActivityStatusCode.Error, $"{e}");
                activityErrorConsumer?.SetTag("ERROR", $"Error on consuming message: {e}");
                throw;
            }
        }
    }
}