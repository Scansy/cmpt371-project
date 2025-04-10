using System;
using UnityEngine;

namespace Shared.Packet
{
    [Serializable]
    public class SpawnPlayerPacket : IDisposable
    {
        public Vector3 spawnPosition;
        public Quaternion startingRotation; //getter

        public SpawnPlayerPacket(Vector3 pos, Transform playerTransform) {
            this.spawnPosition = pos; // Set the spawn position
            this.startingRotation = playerTransform.rotation; // Capture the current rotation (Quaternion)
        }

        public void Dispose()
        {
            
        }
    }
}