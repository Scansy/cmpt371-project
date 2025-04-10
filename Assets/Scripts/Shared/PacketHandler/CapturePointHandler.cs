using UnityEngine;
using Shared.Packet;
using Shared.PacketHandler;
using System;
using GameLogic;

namespace Client
{
    public class CapturePointHandler : IPacketHandler
    {
        private readonly GameClient client;
        private CapturePoint capturePoint;

        public CapturePointHandler(GameClient client, CapturePoint capturePoint)
        {
            this.client = client;
            this.capturePoint = capturePoint;
        }

        public void HandlePacket(IDisposable packet)
        {
            if (packet is CapturePointUpdatePacket updatePacket)
            {
                // Update the capture point's visual state
                if (capturePoint != null)
                {
                    // Update the capture progress bar
                    if (capturePoint.captureBar != null)
                    {
                        capturePoint.captureBar.value = updatePacket.CaptureProgress;
                    }

                    // Update the point's color
                    if (capturePoint.spriteRenderer != null)
                    {
                        capturePoint.spriteRenderer.color = updatePacket.PointColor;
                    }

                    // Update the controlling player reference if needed
                    if (!string.IsNullOrEmpty(updatePacket.ControllingPlayerId))
                    {
                        // Find the controlling player GameObject by ID
                        var players = GameObject.FindGameObjectsWithTag("Player");
                        foreach (var player in players)
                        {
                            var playerControl = player.GetComponent<PlayerControl>();
                            if (playerControl != null && playerControl.client.getPlayerId() == updatePacket.ControllingPlayerId)
                            {
                                capturePoint.controllingPlayer = player;
                                break;
                            }
                        }
                    }
                    else
                    {
                        capturePoint.controllingPlayer = null;
                    }

                    // Update the captured state
                    capturePoint.isCaptured = updatePacket.IsCaptured;
                }
            }
        }
    }
} 