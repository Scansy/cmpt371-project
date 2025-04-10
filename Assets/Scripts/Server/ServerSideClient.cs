using System;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Shared.Packet;
using Client;
using UnityEngine;

namespace Server
{
    public class ServerSideClient
    {
        private Thread _clientThread;
        private GameServer _server;
        private BinaryFormatter _formatter = new BinaryFormatter();
        private TcpClient _client; //active socket instance
        //GameClient realClient;

        public ServerSideClient(GameServer server, TcpClient client)
        {
            _server = server;
            _client = client;
            _clientThread = new Thread(ReceiveMessage);
        }

        public void ReceiveMessage() // Recieves from Client
        {
            while (_client.Connected)
            {
                var stream = _client.GetStream();
                var packet = (IDisposable) _formatter.Deserialize(stream);
                _server.HandlePacket(packet);
            }
        }
        
        public bool IsConnected()
        {
            return _client != null && _client.Connected;
        }
        
        public void SendMessage(IDisposable packet) // Sends to corresponding client
        {
            if (_client != null && _client.Connected)
            {
                var stream = _client.GetStream();
                _formatter.Serialize(stream, packet);
            }
        }
        
        public void Shutdown()
        {
            Debug.Log("Shutting down ServerSideClient...");
            
            // Stop the client thread
            if (_clientThread != null && _clientThread.IsAlive)
            {
                Debug.Log("Waiting for client thread to finish...");
                _clientThread.Join(1000); // Wait up to 1 second
                if (_clientThread.IsAlive)
                {
                    Debug.LogWarning("Client thread did not finish in time, aborting...");
                    _clientThread.Abort();
                }
            }
            
            // Close the client connection
            if (_client != null)
            {
                try
                {
                    _client.Close();
                    _client = null;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error closing client: {ex.Message}");
                }
            }
            
            Debug.Log("ServerSideClient shutdown complete.");
        }
    }
}