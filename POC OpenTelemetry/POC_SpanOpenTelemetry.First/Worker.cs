using System.Diagnostics;
using POC_SpanOpenTelemtry.Common.Helpers;
using POC_SpanOpenTelemetry.Core.Services;
using ActivitySource = System.Diagnostics.ActivitySource;

namespace POC_SpanOpenTelemetry.First;

public class Worker(ILogger<RabbitMqService> loggerMq) : BackgroundService
{
    private readonly RabbitMqService _rabbitMq = new();
    private readonly ActivitySource _activitySource = new("First Service");


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            const string topicPub = "test";
            var message = _rabbitMq.ListenMessage(); 
            var traceByte = Utils.SpanTraceId(message.Headers!.Values);
            var ctx = Utils.GetContext(null, traceByte);
            using var activity = _activitySource.StartActivity(nameof(ExecuteAsync), ActivityKind.Server, ctx);
            _rabbitMq.SendMessage(message.Body!, topicPub, message.Headers);
            loggerMq.LogInformation($"{message.Headers}");
            Activity.Current?.AddEvent(new ActivityEvent("passed through here"));
            await Task.Delay(1000, stoppingToken);
        }
    }

  
}