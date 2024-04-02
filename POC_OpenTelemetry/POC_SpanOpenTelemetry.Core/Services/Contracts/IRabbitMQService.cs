using POC_SpanOpenTelemtry.Common.Model;

namespace POC_SpanOpenTelemetry.Core.Services.Contracts;

public interface IRabbitMQService
{
    public RabbitMQMessage ListenMessage();
    public void SendMessage(byte[] body, string route, IDictionary<string, object>? headers = null);
}