using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using GameLogic;
using Shared;               
using Shared.Packet;

namespace Server
{
    public class ServerSideClient
    {
        private readonly GameServer _server;
        private readonly TcpClient _client;
        private readonly BinaryFormatter _formatter = new BinaryFormatter();
        private readonly Thread _clientThread;

        public int ClientId { get; }

        public ServerSideClient(GameServer server, TcpClient client, int clientId)
        {
            _server = server;
            _client = client;
            ClientId = clientId;
            _clientThread = new Thread(ReceiveMessage);
        }

        public void ReceiveMessage()
        {
            // try
            // {
                while (_client.Connected)
                {
                    var stream = _client.GetStream();
                    var packet = (IDisposable)_formatter.Deserialize(stream);
                    _server.HandlePacket(packet);
                }
            // }
            // catch (Exception ex)
            // {
            //     Debug.LogError($"Error receiving message from client {ClientId}: {ex.Message}");
            // }
        }

        public void SendMessage(IDisposable packet)
        {
            // try
            // {
                if (_client.Connected)
                {
                    var stream = _client.GetStream();
                    _formatter.Serialize(stream, packet);
                }
            // }
            // catch (Exception ex)
            // {
            //     Debug.Log($"Error sending message to client {ClientId}: {ex.Message}");
            // }
        }
    }
}