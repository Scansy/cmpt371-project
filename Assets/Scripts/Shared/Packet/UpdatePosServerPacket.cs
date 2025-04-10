using System;
using UnityEngine;

namespace Shared.Packet
{
    [Serializable]
    public class UpdatePosServerPacket : IDisposable
    {
        private SerializableVector2 position;
        private float rotation;

        private string playerId;

        public UpdatePosServerPacket(string playerID, SerializableVector2 position, float rotation)
        {
            this.playerId = playerID;
            this.position = position;
            this.rotation = rotation;
        }

        public void Dispose()
        {
        }

        public SerializableVector2 Position
        {
            get { return position; }
        }

        public float Rotation
        {
            get { return rotation; }
        }
    }
}