using System;
using UnityEngine;

namespace Shared.Packet
{
    [Serializable]
    public class PlayerMovementPacket : IDisposable
    {
        // Movement-related data (2D)
        public Vector2 position;    // Player's position (in 2D)
        public Vector2 velocity;    // Player's movement velocity (2D)
        public float rotation;      // Rotation around the Z-axis (for 2D)
        
        // Whether the player is running

        public PlayerMovementPacket(Vector2 position, Vector2 velocity, float rotation)
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
