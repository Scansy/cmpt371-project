using System;
using UnityEngine;


namespace Shared.Packet
{
    [Serializable]
    public class SpawnProjectilePacket : IDisposable
    {
        public Vector3 spawnPosition;
        public float startingRotation; //getter
        public SerializableVector2 velocity;    // Player's movement velocity (2D)

        public SpawnProjectilePacket(Vector3 pos, float rotation, SerializableVector2 speed) {
            this.spawnPosition = pos; // Set the spawn position
            this.startingRotation = rotation; // Capture the current rotation (Quaternion)
            this.velocity = speed;
        }
        
        public void Dispose()
        {
        }
    }
}