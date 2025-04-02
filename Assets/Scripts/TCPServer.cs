using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPServer : MonoBehaviour
{
    private TcpListener _server;
    private Thread _serverThread;
    private bool _isRunning = false;

    private void Start()
    {
        _serverThread = new Thread(StartServer);
        _serverThread.IsBackground = true;
        _serverThread.Start();
    }

    private void StartServer()
    {
        _server = new TcpListener(IPAddress.Any, 7777);
        _server.Start();
        _isRunning = true;
        Debug.Log("Server started on port 7777...");

        while (_isRunning)
        {
            TcpClient client = _server.AcceptTcpClient();
            Debug.Log("Client connected!");
            Thread clientThread = new Thread(HandleClient);
            clientThread.IsBackground = true;
            clientThread.Start(client);
        }
    }

    private void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        while (client.Connected)
        {
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead == 0) break;

            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Debug.Log("Received: " + message);

            // Process input (Example: move player)
            string response = "Server processed: " + message;
            byte[] responseData = Encoding.UTF8.GetBytes(response);
            stream.Write(responseData, 0, responseData.Length);
        }

        client.Close();
    }

    private void OnApplicationQuit()
    {
        _isRunning = false;
        _server.Stop();
    }
}
