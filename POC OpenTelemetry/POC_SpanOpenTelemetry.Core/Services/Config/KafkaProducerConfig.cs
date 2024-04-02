
using Confluent.Kafka;

namespace POC_SpanOpenTelemetry.Core.Services.Config;

public class KafkaProducerConfig
{
    private readonly ProducerConfig _producerConfig = new()
    {
        ClientId = Environment.GetEnvironmentVariable("CLIENTID"),
        BootstrapServers = Environment.GetEnvironmentVariable("BOOTSTRAPSERVERS"),
        EnableSslCertificateVerification = false,
        AllowAutoCreateTopics = true //Caso queira criar automaticamente o topico no momento da publicação//
    };

    public IProducer<string, string> Build()
    {
        var producer = new ProducerBuilder<string, string>(_producerConfig)
            .SetStatisticsHandler((_, json) => Console.WriteLine($"Statics:{json}"))
            .Build();
        return producer;
    }
}