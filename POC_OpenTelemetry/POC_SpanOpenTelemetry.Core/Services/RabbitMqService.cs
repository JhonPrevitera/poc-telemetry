using System.Collections.Concurrent;
using System.Diagnostics;
using POC_SpanOpenTelemetry.Core.Config;
using POC_SpanOpenTelemetry.Core.Services.Contracts;
using POC_SpanOpenTelemtry.Common.Model;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ActivitySource = System.Diagnostics.ActivitySource;

namespace POC_SpanOpenTelemetry.Core.Services;

public class RabbitMqService: IRabbitMQService
{
 
    private static readonly string Queue = Environment.GetEnvironmentVariable("QUEUE")!;
    private static readonly RabbitConfig RabbitConfig = new (Queue);
    private readonly BlockingCollection<RabbitMQMessage> _messageQueue = new();
    private readonly ActivitySource _activitySource = new ("Service RabbitMQ");
    private readonly IModel _model =  RabbitConfig.ConfigureQueue();
    
    public RabbitMQMessage ListenMessage()
    {
        var consumer = new EventingBasicConsumer(_model);
        consumer.Received += (_, @event) =>
        {
            var message = new RabbitMQMessage
            {
                Body = @event.Body.ToArray(),
                Headers = @event.BasicProperties.Headers
            };
            _messageQueue.Add(message);
            _model.BasicAck(@event.DeliveryTag, true);
        };
        _model.BasicConsume(Queue, false, consumer);
        return _messageQueue.Take();
    }

    public void SendMessage(byte[] body, string route, IDictionary<string, object>? headers = null)
    {
        using var activityPublish = _activitySource.StartActivity("SendMessage", ActivityKind.Producer);
        try
        {
            var props = _model.CreateBasicProperties();
            props.Headers = headers ?? new Dictionary<string, object>();

            _model.BasicPublish("", route, props, body);
            activityPublish?.AddTag("Status", "Ok");
        }
        catch
        {
            using var activityError = _activitySource.StartActivity("Error" + "Producer");
            activityError?.SetStatus(ActivityStatusCode.Error);
        }
    }
}