using System.Diagnostics;
using System.Text;
using Confluent.Kafka;

namespace POC_SpanOpenTelemtry.Publish;

public class Worker : BackgroundService
{
    // private readonly ILogger<KafkaClient> _loggerKafka;
    // private readonly KafkaClient kafkaClient;
    private readonly ActivitySource activitySource;

    public Worker()
    {
        // kafkaClient = new KafkaClient(_loggerKafka!, "worker3");
        activitySource = new ActivitySource("Terceiro Serviço");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var topicPub = "teste3";
            // var message = kafkaClient.Consumer();
            // var ctx = GetContext(message.Headers);
            using var activity = activitySource.StartActivity(nameof(ExecuteAsync), ActivityKind.Server);

            // if (!String.IsNullOrEmpty(message.Value))
            {
                // kafkaClient.KafkaProducer(message, topicPub).Wait();
            }
            // _loggerKafka.BeginScope(topicPub);
            // _loggerKafka.LogInformation(message.Value);
            activity?.Stop();
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