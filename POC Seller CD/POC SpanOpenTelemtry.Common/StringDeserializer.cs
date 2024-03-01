using Confluent.Kafka;
using System.Text.Json;
using System.Text;

namespace Common
{
    public class StringDeserializer : IDeserializer<string?>
    {
        public string? Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            string _payload = string.Empty;
            if (isNull)
                return default;

            try
            {
                var jsonString = Encoding.UTF8.GetString(data.ToArray());
                _payload = jsonString;

                return _payload;
            }
            catch (Exception ex)
            {
                throw new JsonException(string.Concat("Error deserializing Kafka message value ::: ", _payload), ex);
            }
        }
    }
}