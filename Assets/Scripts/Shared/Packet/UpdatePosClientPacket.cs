using System;
using UnityEngine;

namespace Shared.Packet
{
    [Serializable]
    public class UpdatePosClientPacket : IDisposable
    {
        private Vector2 position;
        private Quaternion rotation;

        public UpdatePosClientPacket(Vector2 position, Quaternion rotation)
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

        public Quaternion Rotation
        {
            get { return rotation; }
        }
    }
}