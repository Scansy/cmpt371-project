using UnityEngine;
using Shared.PacketHandler;
using System;

namespace Shared.Packet
{
    [Serializable]
    public class CapturePointUpdatePacket : IDisposable
    {
        private float captureProgress;
        private bool isCaptured;
        private string controllingPlayerId;
        private Color pointColor;

        public CapturePointUpdatePacket(float progress, bool captured, string controllerId, Color color)
        {
            captureProgress = progress;
            isCaptured = captured;
            controllingPlayerId = controllerId;
            pointColor = color;
        }

        public float CaptureProgress => captureProgress;
        public bool IsCaptured => isCaptured;
        public string ControllingPlayerId => controllingPlayerId;
        public Color PointColor => pointColor;

        public void Dispose()
        {
        }
    }
}