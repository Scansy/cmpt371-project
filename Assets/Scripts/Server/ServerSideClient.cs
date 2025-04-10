using System;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Shared.Packet;
using Client;

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
    }
}