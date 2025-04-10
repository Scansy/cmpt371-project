using System;
using Shared.Packet;
using Server;
using UnityEngine;
using GameLogic;

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

                Debug.Log($"Before loop");
                foreach (PlayerData data in server._players.Values)
                {
                    Debug.Log($"Player ID in _players: " + data.id);
                }
             Debug.Log($"AFter loop");

                lock (server._players)
                {
                    // Check if the player exists on the server
                    if (server._players.TryGetValue(playerMovementPacket.playerId.ToString(), out var playerData))
                    {
                        // Update the player's position, velocity, and rotation on the server
                        playerData.position = playerMovementPacket.GetMovementVector();
                        playerData.rotation = Quaternion.Euler(0f, 0f, playerMovementPacket.rotation);

                        Debug.Log($"Updated Player {playerMovementPacket.playerId}: Position={playerMovementPacket.GetMovementVector()}, Rotation={playerMovementPacket.rotation}");
                    }
                    else
                    {
                        Debug.LogWarning($"Player with ID {playerMovementPacket.playerId} not found on the server.");
                    }
                }

                // Broadcast the updated position to all clients
                server.BroadcastData(new UpdatePosClientPacket(
                    playerMovementPacket.GetMovementVector(),
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