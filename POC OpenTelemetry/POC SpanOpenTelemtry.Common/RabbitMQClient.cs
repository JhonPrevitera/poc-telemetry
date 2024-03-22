using Confluent.Kafka;
using POC_SpanOpenTelemtry.Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace POCSpanOpentelemetry.Common
{
    public class RabbitMQClient : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _model;
        private readonly string _queueName;
        private readonly System.Diagnostics.ActivitySource activitySource;
        private BlockingCollection<RabbitMQMessage> _messageQueue = new BlockingCollection<RabbitMQMessage>();

        public RabbitMQClient()
        {
            string? queue = Environment.GetEnvironmentVariable("QUEUE");
            string? rabbitMqUrl = $"amqp://guest:guest@localhost:5672//";
            var connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(rabbitMqUrl)
            };
            _connection = connectionFactory.CreateConnection();
            _model = _connection.CreateModel();
            _queueName = queue!;
            activitySource = new("Primeiro Serviço");
        }

        private void ConfigureQueue(string name, IModel model)
        {
            model.QueueDeclare(queue: name, durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        public RabbitMQMessage Listen()
        {
            try
            {
                ConfigureQueue(_queueName, _model);

                var consumer = new EventingBasicConsumer(_model);
                consumer.Received += (sender, @event) =>
                {
                    var message = new RabbitMQMessage
                    {
                        Body = @event.Body.ToArray(),
                        Headers = @event.BasicProperties.Headers
                    };

                    _messageQueue.Add(message);
                    _model.BasicAck(@event.DeliveryTag, true);
                };
                _model.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);

                return _messageQueue.Take();
            }
            catch { throw; }

        }


        public void SendMessage(byte[] body, string route, IDictionary<string, object>? headers = null)
        {
            using var activitPublish = activitySource.StartActivity("SendMessage", ActivityKind.Producer);
            try
            {
                IBasicProperties props = _model.CreateBasicProperties();
                props.Headers = headers ?? new Dictionary<string, object>();

                _model.BasicPublish(exchange: "", routingKey: route, basicProperties: props, body: body);
                activitPublish?.AddTag("Status", "Deu Certo");
            }
            catch
            {
                using var activityErro = activitySource.StartActivity("ErroPublisher");
                activityErro?.SetStatus(ActivityStatusCode.Error);
            }

        }

        public void Dispose()
        {
            _model.Dispose();
            _connection.Dispose();
        }
    }

}

