using System.Net;
using System.Net.Sockets;

namespace GameServer;

public class Server
{
    public int Port { get; set; }
    private TcpListener _tcpListener;

    public void Start(int port)
    {
        Port = port;
        
        // tcp listener listens from any IP address
        _tcpListener = new TcpListener(IPAddress.Any, port);
        _tcpListener.Start();
        
        // TODO: make listener accept callback function
        
        Console.WriteLine($"Server started on port {port}.");
    }
    
    // setup callback function
}