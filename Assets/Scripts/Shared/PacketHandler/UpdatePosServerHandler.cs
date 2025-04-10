using System;
using UnityEngine;
using Server;
using Shared.Packet;

namespace Shared.PacketHandler
{
    public class UpdatePosServerHandler : MonoBehaviour, IPacketHandler 
    {
        private GameServer server;

        private void Awake()
        {
            server = GameServer.Instance;
        }

        public void HandlePacket(IDisposable packet)
        {
            var updatePositionPacket = (UpdatePosServerPacket)packet;

            var position = updatePositionPacket.Position;
            var rotation = updatePositionPacket.Rotation;

            UpdatePositionPlayer(position, Quaternion.Euler(0f, 0f, updatePositionPacket.Rotation));
            
            server.BroadcastData(new UpdatePosClientPacket(position, rotation));
        }

        private void UpdatePositionPlayer(Vector2 position, Quaternion rotation)
        {
            // TODO: in the server, update _players
            var playerList = server._players;

            foreach (var player in playerList)
                {
                    try
                    {
                        player.Value.position = position;
                        player.Value.rotation = rotation;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error sending data to client {player.Key}: {ex.Message}");
                    }
                }
        }
    }
}