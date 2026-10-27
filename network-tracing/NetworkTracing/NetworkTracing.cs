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
        IPAddress[] hostAddresses = GetHostAddresses();

        // Try converting this to a do while loop
        foreach (var address in hostAddresses)
        {
            if (ResolveNetworkInterface(address))
            {
                return;
            }
        }

        throw new InvalidOperationException($"Could not determine network interface for host {Host} and port {Port}.");
    }

    private bool ResolveNetworkInterface(IPAddress destinationAddress)
    {
        try
        {
            using var socket = new Socket(destinationAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect(new IPEndPoint(destinationAddress, Port));

            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (HasLocalEndPoint(socket, networkInterface))
                {
                    return true;
                }
            }
        }
        catch (SocketException)
        {
            // Try all resolved addresses for the host.
        }

        return false;
    }

    private bool HasLocalEndPoint(Socket socket, NetworkInterface networkInterface)
    {
        var hasLocalAddress = networkInterface.GetIPProperties().UnicastAddresses
            .Any(address => address.Address.Equals(((IPEndPoint)socket.LocalEndPoint).Address)); // Should always be IPEndPoint

        if (hasLocalAddress)
        {
            NetworkInterface = networkInterface;
            return true;
        }

        return false;
    }

    private IPAddress[] GetHostAddresses()
    {
        try
        {
            return Dns.GetHostAddresses(Host);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Could not resolve host {Host}.", ex);
        }
    }
}

