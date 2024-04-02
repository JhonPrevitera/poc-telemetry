using System.Text;
using Confluent.Kafka;

namespace POC_SpanOpenTelemtry.Common.Helpers;

public static class Utils
{
    public static ActivityContext GetContext(Headers? headers, string? traceParent)
    {
        if (headers?.Count == 0 && string.IsNullOrEmpty(traceParent)) return new ActivityContext();
        try
        {
            if (!string.IsNullOrEmpty(traceParent))
            {
                return new ActivityContext(ActivityTraceId.CreateFromString(traceParent), ActivitySpanId.CreateRandom(),
                    ActivityTraceFlags.Recorded);
            }
            var traceParentValue = headers!.BackingList.FirstOrDefault(e => e.Key == "traceparent");
            var traceParentBytes = traceParentValue?.GetValueBytes();
            var traceParentString = Encoding.UTF8.GetString(traceParentBytes!);
            Console.WriteLine(traceParentString);
            var ctx = ActivityContext.Parse(traceParentString, null);
            Console.WriteLine(traceParentString);
            return new ActivityContext(ctx.TraceId,
                ActivitySpanId.CreateRandom(), ActivityTraceFlags.Recorded);
        }
        catch
        {
            return new ActivityContext();
        }
    }
    public static string SpanTraceId(ICollection<object>? traceParent)
    {
        if (traceParent == null) return string.Empty;
        var byteList = new List<byte>();
        foreach (var value in traceParent)
        {
            if (value is byte[] byteArray)
            {
                byteList.AddRange(byteArray);
            }
        }
        var traceParentBytes = byteList.ToArray();
        var traceByte = Encoding.UTF8.GetString(traceParentBytes);
        var traceId = traceByte.Split("-");
        return traceId[1];
    }
}