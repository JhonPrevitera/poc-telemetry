using POC_SpanOpenTelemtry.Common.Model;

namespace POC_SpanOpenTelemetry.Api.test;

public class UnitTest1
{
    [Theory]
    [InlineData("Oi", "Ol�")]
    public void Model(string message, string header)
    {
        var model = new MessageModel{Message = message, Header = header};
        Assert.Equal(message, model.Message);
        Assert.Equal(header, model.Header);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("ab", "ab")]
    public void InvalidMessage(string message, string headers)
    {
        var model = new MessageModel { Message = message, Header = headers };
        Assert.True(model.Header == null ||  model.Message == null);
        Assert.True(message.Length >= 3 && headers.Length >= 3,"Message and Header both have more than three letters and are not null.");
    }
    
    
}