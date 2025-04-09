using System;
using UnityEngine;


namespace Shared.Packet
{
    [Serializable]
    public class SpawnProjectilePacket : IDisposable
    {
        public Vector3 spawnPosition;
        public Quaternion startingRotation; //getter
        public Vector2 velocity;    // Player's movement velocity (2D)

        public SpawnProjectilePacket(Vector3 pos, Transform projTransform, Vector2 speed) {
            this.spawnPosition = pos; // Set the spawn position
            this.startingRotation = projTransform.rotation; // Capture the current rotation (Quaternion)
            this.velocity = speed;
        }
        // public void Dispose()
        // {
        //     s = null;
        // }
    }
}