using System.Diagnostics;
using System.Text;
using Confluent.Kafka;
using POCSpanOpentelemetry.Common;

namespace POC_SpanOpenTelemtry.Mid;

public class Worker : BackgroundService
{
    private readonly ILogger<RabbitMQClient> _loggerMQ;
    private readonly RabbitMQClient _rabbitMQ;
    private readonly ActivitySource activitySource;


    public Worker(ILogger<RabbitMQClient> loggerMQ)
    {
        _rabbitMQ = new RabbitMQClient();
        activitySource = new ActivitySource("Primeiro Serviço");
        _loggerMQ = loggerMQ;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var topicPub = "teste2";
            var message = _rabbitMQ.Listen();
            //var ctx = GetContext(message.Result.Headers);
            using var activity = activitySource.StartActivity(nameof(ExecuteAsync), ActivityKind.Server);
            _rabbitMQ.SendMessage(message.Body, topicPub, message.Headers);

            _loggerMQ.LogInformation($"{message.Headers}");
            Activity.Current?.AddEvent(new ActivityEvent("Passou aqui"));
            await Task.Delay(1000, stoppingToken);
        }
    }

    private static ActivityContext GetContext(Headers headers)
    {
        if (headers.Count == 0) return new ActivityContext();

        try
        {
            var traceparentValue = headers.BackingList.FirstOrDefault(e => e.Key == "traceparent");
            var traceparentBytes = traceparentValue!.GetValueBytes();
            var listHeadrs = Encoding.UTF8.GetString(traceparentBytes);
            var ctx = ActivityContext.Parse(listHeadrs, null);
            return new ActivityContext(ctx.TraceId, ActivitySpanId.CreateRandom(), ActivityTraceFlags.Recorded);
        }
        catch
        {
            return new ActivityContext();
        }
    }
}