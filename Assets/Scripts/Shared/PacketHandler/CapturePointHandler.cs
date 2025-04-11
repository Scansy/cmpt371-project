using UnityEngine;
using Shared.Packet;
using Shared.PacketHandler;
using GameLogic;
using System;

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
            var capturePointPacket = (CapturePointUpdatePacket) packet;
            
            // Update the capture progress bar
            capturePoint.captureBar.value = capturePointPacket.CaptureProgress;

            // Update the point's color
            capturePoint.spriteRenderer.color = capturePointPacket.PointColor;
        }
    }
} 