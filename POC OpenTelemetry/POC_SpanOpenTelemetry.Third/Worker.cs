using System.Diagnostics;

using POC_SpanOpenTelemtry.Common.Helpers;
using POC_SpanOpenTelemetry.Core.Services;

namespace POC_SpanOpenTelemetry.Third;

public class Worker : BackgroundService
{
    private readonly ILogger<KafkaService> _loggerKafka;
    private readonly KafkaService _kafkaService;
    private readonly ActivitySource _activitySource;

    public Worker(ILogger<KafkaService> loggerKafka)
    {
        _loggerKafka = loggerKafka;
        _kafkaService = new KafkaService(_loggerKafka);
        _activitySource = new ActivitySource("Third Service");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            const string topicPub = "test3";
            var message = _kafkaService.Consumer();
            var ctx = Utils.GetContext(message.Headers, null);
            using var activity = _activitySource.StartActivity(nameof(ExecuteAsync), ActivityKind.Server, ctx);
            if (!string.IsNullOrEmpty(message.Value))
            {
                _kafkaService.ProduceMessage(topicPub, message.Value, ctx.ToString()!);
            }
            _loggerKafka.BeginScope(topicPub);
            _loggerKafka.LogInformation(message.Value);
            activity?.Stop();
            await Task.Delay(1000, stoppingToken);
        }
    }
}