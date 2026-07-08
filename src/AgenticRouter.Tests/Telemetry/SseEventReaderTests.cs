using AgenticRouter.Telemetry;

namespace AgenticRouter.Tests.Telemetry;

/// <summary>Covers <see cref="SseEventReader"/>.</summary>
public class SseEventReaderTests
{
    [Fact]
    public void ReadDataEvents_NullInput_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => SseEventReader.ReadDataEvents(null!).ToList());
    }

    [Fact]
    public void ReadDataEvents_EmptyInput_YieldsNothing()
    {
        var events = SseEventReader.ReadDataEvents(string.Empty).ToList();

        Assert.Empty(events);
    }

    [Fact]
    public void ReadDataEvents_MultipleValidEvents_YieldsInOrder()
    {
        var sse = "data: {\"n\":1}\n\ndata: {\"n\":2}\n\ndata: {\"n\":3}\n\n";

        var events = SseEventReader.ReadDataEvents(sse).ToList();

        Assert.Equal([1, 2, 3], events.Select(e => (int)e["n"]!));
    }

    [Fact]
    public void ReadDataEvents_DoneSentinel_IsSkipped()
    {
        var sse = "data: {\"n\":1}\n\ndata: [DONE]\n\n";

        var events = SseEventReader.ReadDataEvents(sse).ToList();

        Assert.Single(events);
    }

    [Fact]
    public void ReadDataEvents_MalformedJsonLine_IsSkippedNotThrown()
    {
        var sse = "data: { not valid json\n\ndata: {\"n\":1}\n\n";

        var events = SseEventReader.ReadDataEvents(sse).ToList();

        Assert.Single(events);
        Assert.Equal(1, (int)events[0]["n"]!);
    }

    [Fact]
    public void ReadDataEvents_NonObjectJsonPayload_IsSkipped()
    {
        var sse = "data: [1,2,3]\n\ndata: {\"n\":1}\n\n";

        var events = SseEventReader.ReadDataEvents(sse).ToList();

        Assert.Single(events);
    }

    [Fact]
    public void ReadDataEvents_NonDataLines_AreIgnored()
    {
        var sse = "event: message_start\nid: 42\n\ndata: {\"n\":1}\n\n";

        var events = SseEventReader.ReadDataEvents(sse).ToList();

        Assert.Single(events);
    }

    [Fact]
    public void ReadDataEvents_EmptyDataPayload_IsSkipped()
    {
        var sse = "data: \n\ndata: {\"n\":1}\n\n";

        var events = SseEventReader.ReadDataEvents(sse).ToList();

        Assert.Single(events);
    }
}
