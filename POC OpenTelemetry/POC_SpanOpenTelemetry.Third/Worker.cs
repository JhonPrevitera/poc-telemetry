using System.Diagnostics;

using POC_SpanOpenTelemtry.Common.Helpers;
using POC_SpanOpenTelemetry.Core.Services;

namespace POC_SpanOpenTelemetry.Third;

public class Worker(ILogger<KafkaService> loggerKafka) : BackgroundService
{
    
    private readonly KafkaService _kafkaService = new KafkaService(loggerKafka);
    private readonly ActivitySource _activitySource = new ActivitySource("Third Service");
    private const string TopicPub = "test3";
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var message = _kafkaService.Consumer();
            var ctx = Utils.GetContext(message.Headers, null);
            using var activity = _activitySource.StartActivity(nameof(ExecuteAsync), ActivityKind.Server, ctx);
            if (!string.IsNullOrEmpty(message.Value))
            {
                _kafkaService.ProduceMessage(TopicPub, message.Value, ctx.ToString()!);
            }
            loggerKafka.BeginScope(TopicPub);
            loggerKafka.LogInformation(message.Value);
            Activity.Current?.SetTag($"{ctx}", ctx.ToString());
            await Task.Delay(1000, stoppingToken);
        }
    }
}