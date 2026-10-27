using System.Net.NetworkInformation;

namespace NetworkTracing.Tests;

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
        Assert.Throws<InvalidOperationException>(() => new NetworkTracer("lucalhust", 9));
    }
}
