using System;
using UnityEngine;

namespace Shared.Packet
{
    [Serializable]
    public class UpdatePosClientPacket : IDisposable
    {
        private SerializableVector2 position;
        private float rotation;

        public UpdatePosClientPacket(SerializableVector2 position, float rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }

        public void Dispose()
        {
        }

        public Vector2 Position
        {
            get { return position; }
        }

        public float Rotation
        {
            get { return rotation; }
        }
    }
}