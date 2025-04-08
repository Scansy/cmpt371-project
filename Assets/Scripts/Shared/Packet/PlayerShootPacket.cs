using System;
using UnityEngine;

namespace Shared.Packet
{
    [Serializable]
    public class PlayerShootPacket : IDisposable
    {
        public Vector2 shootPosition;     // The position of the player (or where the shot was fired from)
        public float shootAngle;          // The angle of the shot, or you could use a direction vector instead

        // Constructor to initialize the packet
        public PlayerShootPacket(Vector2 shootPosition, float shootAngle)
        {
            this.shootPosition = shootPosition;
            this.shootAngle = shootAngle;
        }

        // Dispose method (clean up)
        public void Dispose()
        {
            // Clean up resources if needed
        }
    }
}
