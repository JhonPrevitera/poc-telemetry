using System.Diagnostics;
using POC_SpanOpenTelemtry.Common.Helpers;
using POC_SpanOpenTelemetry.Core.Services;

namespace POC_SpanOpenTelemetry.Second;

public class Worker(ILogger<RabbitMqService> loggerMq, ILogger<KafkaService> loggerKafka) : BackgroundService
{
    private readonly RabbitMqService _rabbitMq = new();
    private readonly ActivitySource _activitySource = new("Second Service");
    private readonly KafkaService _kafkaService = new(loggerKafka);
    private const string QueuePub = "test2";
    private const string TopicPub = "topic2";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var message = _rabbitMq.ListenMessage();
            var traceByte = Utils.SpanTraceId(message.Headers!.Values);
            var ctx = Utils.GetContext(null, traceByte);
            using var activity = _activitySource.StartActivity(nameof(ExecuteAsync), ActivityKind.Server, ctx);
            _rabbitMq.SendMessage(message.Body!, QueuePub, message.Headers);
            _kafkaService.ProduceMessage(TopicPub, message.Body?.ToString()!, activity!.Id!);
            loggerMq.LogInformation($"{message.Headers}");
            Activity.Current?.AddEvent(new ActivityEvent("passed through here"));
            Activity.Current?.SetTag($"{ctx}", ctx.ToString());
            await Task.Delay(1000, stoppingToken);
        }
    }
}