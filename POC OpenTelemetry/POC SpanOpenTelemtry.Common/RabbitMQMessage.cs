using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POC_SpanOpenTelemtry.Common
{
    public class RabbitMQMessage
    {
        public byte[]? Body { get; set; } 
        public IDictionary<string, object>? Headers { get; set; }
    }
}
