using POC_SpanOpenTelemtry.Core.Config;
using System.Diagnostics;
using System.Text;
using Confluent.Kafka;
using Confluent.Kafka.Extensions.Diagnostics;
using Microsoft.Extensions.Logging;
using POC_SpanOpenTelemtry.Core.Services.Contracts;

namespace POC_SpanOpenTelemtry.Core
{
    public class KafkaService: IKafkaService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IProducer<string, string> _producer;
        private readonly ErrorCode _errorCode;
        private readonly ILogger<KafkaService> _logger;
        private readonly ActivitySource _activity;

        public KafkaService(ILogger<KafkaService> logger)
        {
            _logger = logger;
            _consumer = new KafkaConsumerConfig(logger).Build();
            _producer = new KafkaProducerConfig().Build();
            _errorCode = new ErrorCode();
            _activity = new ActivitySource(nameof(KafkaService));
        }
        public void ProduceMessage(string topic, string message, string traceparent)
        {
            using var activityProduce = _activity.StartActivity();
            try
            {
                var data = Encoding.UTF8.GetBytes(traceparent);
                var headers = new Headers()
                {
                    new Header("traceparent", data)
                };
                _producer.ProduceAsync(topic,
                    new Message<string, string> { Key = "", Value = message, Headers = headers }).Wait();
            }
            catch (KafkaException e)
            {
                _logger.LogError($"{_errorCode},{e}");
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
                _logger.LogError($"{_errorCode}, {e}");
                using var activityErrorConsumer = _activity.StartActivity();
                activityErrorConsumer?.SetStatus(ActivityStatusCode.Error, $"{e}");
                activityErrorConsumer?.SetTag("ERROR", $"Error on consuming message: {e}");
                throw;
            }
        }
    }
}