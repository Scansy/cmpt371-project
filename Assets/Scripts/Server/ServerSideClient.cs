using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Shared.Packet;

namespace Server
{
    public class ServerSideClient
    {
        private Thread _clientThread;
        private GameServer _server;
        private BinaryFormatter _formatter = new BinaryFormatter();
        private TcpClient _client; //active socket instance

        public ServerSideClient(GameServer server, TcpClient client)
        {
            _server = server;
            _client = client;
            _clientThread = new Thread(DeserializeAndHandlePacket);
        }

        private void DeserializeAndHandlePacket()
        {
            while (_client.Connected)
            {
                var stream = _client.GetStream();
                var packet = (IPacket) _formatter.Deserialize(stream);
                _server.HandlePacket(packet);
            }
        }
    }
}