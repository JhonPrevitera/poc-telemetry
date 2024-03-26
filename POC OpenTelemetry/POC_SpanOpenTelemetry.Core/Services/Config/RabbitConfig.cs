using System.Collections.Concurrent;
using POC_SpanOpenTelemtry.Common;
using RabbitMQ.Client;

namespace POC_SpanOpenTelemetry.Core.Config;

public class RabbitConfig
{
    private readonly IConnection _connection;
    private readonly IModel _model;
    private readonly string _queue;

    public RabbitConfig(string queue)
    {
        _queue = queue;
        var rabbitMqUrl = "amqp://guest:guest@localhost:5672//";
        var connectionFactory = new ConnectionFactory
        {
            Uri = new Uri(rabbitMqUrl)
        };
        _connection = connectionFactory.CreateConnection();
        _model = _connection.CreateModel();
    }
    public IModel ConfigureQueue()
    {
        _model.QueueDeclare(_queue, true, false, false, null);
        return _model;
    }
}