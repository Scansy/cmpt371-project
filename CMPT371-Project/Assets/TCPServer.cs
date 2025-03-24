using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPServer : MonoBehaviour
{
    private TcpListener server;
    private Thread serverThread;
    private bool isRunning = false;

    void Start()
    {
        serverThread = new Thread(StartServer);
        serverThread.IsBackground = true;
        serverThread.Start();
    }

    void StartServer()
    {
        server = new TcpListener(IPAddress.Any, 7777);
        server.Start();
        isRunning = true;
        Debug.Log("Server started on port 7777...");

        while (isRunning)
        {
            TcpClient client = server.AcceptTcpClient();
            Debug.Log("Client connected!");
            Thread clientThread = new Thread(HandleClient);
            clientThread.IsBackground = true;
            clientThread.Start(client);
        }
    }

    void HandleClient(object obj)
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

    void OnApplicationQuit()
    {
        isRunning = false;
        server.Stop();
    }
}
