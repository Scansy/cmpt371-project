using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPClient : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;

    void Start()
    {
        client = new TcpClient("127.0.0.1", 7777);
        stream = client.GetStream();
        receiveThread = new Thread(ReceiveMessages);
        receiveThread.IsBackground = true;
        receiveThread.Start();

        // Example: Send a move command
        SendMessageToServer("Hello from Client!");
    }

    void SendMessageToServer(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        stream.Write(data, 0, data.Length);
        Debug.Log("Sent: " + message);
    }

    void ReceiveMessages()
    {
        byte[] buffer = new byte[1024];

        while (client.Connected)
        {
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead == 0) break;

            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Debug.Log("Received from server: " + message);
        }
    }

    void OnApplicationQuit()
    {
        client.Close();
        stream.Close();
        receiveThread.Abort();
    }
}
