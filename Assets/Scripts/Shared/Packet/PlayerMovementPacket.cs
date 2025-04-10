using System;
using UnityEngine;

namespace Shared.Packet
{
    [Serializable]
    public class PlayerMovementPacket : IDisposable
    {
        // Movement-related data (2D)
        public SerializableVector2 position;    // Player's position (in 2D)
        public SerializableVector2 velocity;    // Player's movement velocity (2D)
        public float rotation;      // Rotation around the Z-axis (for 2D)
        
        // Whether the player is running

        public PlayerMovementPacket(SerializableVector2 position, SerializableVector2 velocity, float rotation)
        {
            this.position = position;
            this.velocity = velocity;
            this.rotation = rotation;
            Debug.Log("Player is moving around");
        }

        public void Dispose()
        {
            
        }
    }
}
