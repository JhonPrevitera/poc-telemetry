using System.Collections.Concurrent;
using System.Diagnostics;
using POC_SpanOpenTelemtry.Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ActivitySource = System.Diagnostics.ActivitySource;

namespace POCSpanOpentelemetry.Common;

public class RabbitMQClient : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _model;
    private readonly string _queueName;
    private readonly ActivitySource activitySource;
    private readonly BlockingCollection<RabbitMQMessage> _messageQueue = new();

    public RabbitMQClient()
    {
        var queue = Environment.GetEnvironmentVariable("QUEUE");
        var rabbitMqUrl = "amqp://guest:guest@localhost:5672//";
        var connectionFactory = new ConnectionFactory
        {
            Uri = new Uri(rabbitMqUrl)
        };
        _connection = connectionFactory.CreateConnection();
        _model = _connection.CreateModel();
        _queueName = queue!;
        activitySource = new ActivitySource("Primeiro Serviço");
    }

    public void Dispose()
    {
        _model.Dispose();
        _connection.Dispose();
    }

    private void ConfigureQueue(string name, IModel model)
    {
        model.QueueDeclare(name, true, false, false, null);
    }

    public RabbitMQMessage Listen()
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
        _model.BasicConsume(_queueName, false, consumer);

        return _messageQueue.Take();
    }


    public void SendMessage(byte[] body, string route, IDictionary<string, object>? headers = null)
    {
        using var activitPublish = activitySource.StartActivity("SendMessage", ActivityKind.Producer);
        try
        {
            var props = _model.CreateBasicProperties();
            props.Headers = headers ?? new Dictionary<string, object>();

            _model.BasicPublish("", route, props, body);
            activitPublish?.AddTag("Status", "Deu Certo");
        }
        catch
        {
            using var activityErro = activitySource.StartActivity("ErroPublisher");
            activityErro?.SetStatus(ActivityStatusCode.Error);
        }
    }
}