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

            UpdatePositionPlayer(updatePositionPacket.PlayerId, position, rotation);

            server.BroadcastData(new UpdatePosClientPacket(updatePositionPacket.PlayerId, position, rotation));
        }

        private void UpdatePositionPlayer(string playerId, Vector2 position, Quaternion rotation)
        {
            // Use the correct Players dictionary from GameServer
            var playerList = server.Players;

            if (playerList.ContainsKey(playerId))
            {
                try
                {
                    playerList[playerId].position = position;
                    playerList[playerId].rotation = rotation;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error updating player {playerId}: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"Player with ID {playerId} not found in the server's player list.");
            }
        }
    }
}