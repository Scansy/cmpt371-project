using System;
using UnityEngine;

namespace Shared.Packet
{
    [Serializable]
    public class ProjectileMovementPacket : IDisposable
    {
        // Movement-related data (2D)
        public Vector2 position;    // Projectile's position (in 2D)
        public Vector2 velocity;    // Projectile's movement velocity (2D)
        public float rotation;      // Rotation around the Z-axis (for 2D)

        // Constructor to initialize the packet
        public ProjectileMovementPacket(Vector2 position, Vector2 velocity, float rotation)
        {
            this.position = position;
            this.velocity = velocity;
            this.rotation = rotation;
        }

        // You can include methods to serialize or update the packet, depending on your needs
        public void Dispose()
        {
            // Clean up if necessary
        }
    }
}
