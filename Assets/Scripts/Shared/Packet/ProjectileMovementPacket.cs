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

        public ProjectileMovementPacket(Vector2 position, Vector2 velocity, float rotation)
        {
            this.position = position;
            this.velocity = velocity;
            this.rotation = rotation;
        }

        public void Dispose()
        {
        }
    }
}
