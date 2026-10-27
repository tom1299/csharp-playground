using System.Diagnostics.Tracing;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace NetworkTracing.Tests;

public class HttpEventListener : EventListener
{

    public List<EventWrittenEventArgs> Events { get; private set; } = [];

    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name == "System.Net.Http")
        {
            EnableEvents(eventSource, EventLevel.Informational);
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        Console.WriteLine($"{DateTime.UtcNow:ss:fff} {eventData.EventName}: " +
            string.Join(' ', eventData.PayloadNames!.Zip(eventData.Payload!).Select(pair => $"{pair.First}={pair.Second}")));
        Events?.Add(eventData);
    }
}

public class NetworkTracerTests
{
    [Fact]
    public void DetermineInterfaceForLocalhostReturnsLoopbackInterface()
    {
        // Port 9 = Discard protocol. See https://en.wikipedia.org/wiki/Discard_Protocol
        var tracer = new NetworkTracer("localhost", 9);
    
        var networkInterface = tracer.NetworkInterface;

        Assert.NotNull(networkInterface);
        Assert.Equal(NetworkInterfaceType.Loopback, networkInterface.NetworkInterfaceType);
    }

    [Fact]
    public void DetermineInterfaceForGoogleDotCom()
    {
        var tracer = new NetworkTracer("google.com", 443);

        var networkInterface = tracer.NetworkInterface;

        Assert.NotNull(networkInterface);
        Assert.NotEqual(NetworkInterfaceType.Loopback, networkInterface.NetworkInterfaceType);
    }

    [Fact]
    public void DetermineInterfaceForNonExistentHost()
    {
        Assert.Throws<SocketException>(() => new NetworkTracer("lucalhust", 9));
    }

    [Fact]
    public async Task Tracing()
    {
        using var listener = new HttpEventListener();
        using var client = new HttpClient();
        using var response = await client.GetAsync("https://httpbin.org/get");

        response.EnsureSuccessStatusCode();

        Assert.NotEmpty(listener.Events);
        Assert.Equal(10, listener.Events.Count);
        Assert.Contains(listener.Events, e => e.EventName == "RequestStart");
        Assert.Contains(listener.Events, e => e.EventName == "RequestStop");
    }
}
