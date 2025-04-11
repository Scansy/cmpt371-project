using UnityEngine;
using Shared.PacketHandler;
using System;

namespace Shared.Packet
{
    [Serializable]
    public class CapturePointUpdatePacket : IDisposable
    {
        private float captureProgress;
        private Color pointColor;

        public CapturePointUpdatePacket(float progress, Color color)
        {
            captureProgress = progress;
            pointColor = color;
        }

        public float CaptureProgress => captureProgress;
        public Color PointColor => pointColor;

        public void Dispose()
        {
        }
    }
}