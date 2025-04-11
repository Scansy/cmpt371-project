using System;
using UnityEngine;

namespace Shared.Packet
{
    [Serializable]
    public class SpawnPlayerPacket : IDisposable
    {
        public string playerId; //getter
        public Vector3 spawnPosition;
        public Quaternion startingRotation; //getter

        public SpawnPlayerPacket(string id, Vector3 pos, Transform playerTransform) {
            this.playerId = id; // Set the player ID
            this.spawnPosition = pos; // Set the spawn position
            this.startingRotation = playerTransform.rotation; // Capture the current rotation (Quaternion)
        }

        public void Dispose()
        {
            
        }
    }
}