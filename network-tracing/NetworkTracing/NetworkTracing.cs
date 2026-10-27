using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace NetworkTracing;

public class NetworkTracer
{

    public string Host { get; private set; }

    public int Port { get; private set; }

    public NetworkInterface? NetworkInterface { get; private set; }

    public NetworkTracer(string host, int port)
    {
        Host = host;
        Port = port;
        DetermineInterface();
    }

    public void DetermineInterface()
    {
        // Select first network interface that can reach the host.
        NetworkInterface = Dns.GetHostAddresses(Host).Select(ResolveNetworkInterface).FirstOrDefault();
    
        if (NetworkInterface == null)
        {
        throw new InvalidOperationException(
            $"Could not determine network interface for host {Host} and port {Port}.");
        }
    }

    private NetworkInterface? ResolveNetworkInterface(IPAddress destinationAddress)
    {
        try
        {
            using var socket = new Socket(destinationAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect(new IPEndPoint(destinationAddress, Port));

            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (HasLocalEndPoint(socket, networkInterface))
                {
                    return networkInterface;
                }
            }
        }
        catch (SocketException)
        {
            // Try all resolved addresses for the host.
        }

        return null;
    }

    private bool HasLocalEndPoint(Socket socket, NetworkInterface networkInterface)
    {
        var hasLocalAddress = networkInterface.GetIPProperties().UnicastAddresses
            .Any(address => address.Address.Equals(((IPEndPoint)socket.LocalEndPoint).Address)); // Should always be IPEndPoint

        if (hasLocalAddress)
        {
            return true;
        }

        return false;
    }
}

