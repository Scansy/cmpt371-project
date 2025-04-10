using System;
using Shared.Packet;
using Server;
using UnityEngine;

namespace Shared.PacketHandler
{
    public class PlayerMovementHandler : IPacketHandler
    {
        public void HandlePacket(IDisposable packet)
        {
            // Ensure the packet is of type PlayerMovementPacket
            if (packet is PlayerMovementPacket playerMovementPacket)
            {
                Debug.Log($"Processing PlayerMovementPacket for Player ID: {playerMovementPacket.playerId}");

                // Get the server instance
                var server = GameServer.Instance;

                lock (server._players)
                {
                    // Check if the player exists on the server
                    if (server._players.TryGetValue(playerMovementPacket.playerId.ToString(), out var playerData))
                    {
                        // Update the player's position, velocity, and rotation on the server
                        playerData.position = playerMovementPacket.position;
                        playerData.rotation = Quaternion.Euler(0f, 0f, playerMovementPacket.rotation);

                        Debug.Log($"Updated Player {playerMovementPacket.playerId}: Position={playerMovementPacket.position}, Rotation={playerMovementPacket.rotation}");
                    }
                    else
                    {
                        Debug.LogWarning($"Player with ID {playerMovementPacket.playerId} not found on the server.");
                    }
                }

                // Broadcast the updated position to all clients
                server.BroadcastData(new UpdatePosClientPacket(
                    playerMovementPacket.position,
                    Quaternion.Euler(0f, 0f, playerMovementPacket.rotation)
                ));
            }
            else
            {
                Debug.LogError("Received an invalid packet type in PlayerMovementHandler.");
            }
        }
    }
}