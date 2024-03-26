namespace POC_SpanOpenTelemtry.Common.Model;

public class RabbitMQMessage
{
    public byte[]? Body { get; set; }
    public IDictionary<string, object>? Headers { get; set; }
}