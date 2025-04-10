using System;
using UnityEngine;

namespace Shared.Packet
{
    [Serializable]
    public class UpdatePosServerPacket : IDisposable
    {
        private Vector2 position;
        private Quaternion rotation;

        public string playerId { get; set;}

        public UpdatePosServerPacket(string playerID, Vector2 position, Quaternion rotation)
        {
            this.playerId = playerID;
            this.position = position;
            this.rotation = rotation;
        }

        public string PlayerId
        {
            get { return playerId; }
            set { playerId = value; }
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