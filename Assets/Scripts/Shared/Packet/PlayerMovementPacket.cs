using System;

namespace Shared.Packet
{
    [Serializable]
    public class PlayerMovementPacket : IDisposable
    {
        public int playerId;          // Player's unique ID
        public float positionX;       // X component of position
        public float positionY;       // Y component of position
        public float movementX;       // X component of movement vector
        public float movementY;       // Y component of movement vector
        public float rotation;        // Rotation around the Z-axis

        public PlayerMovementPacket(int playerId, UnityEngine.Vector2 position, UnityEngine.Vector2 movementVector, float rotation)
        {
            this.playerId = playerId;
            this.positionX = position.x;
            this.positionY = position.y;
            this.movementX = movementVector.x;
            this.movementY = movementVector.y;
            this.rotation = rotation;
        }

        public UnityEngine.Vector2 GetPosition()
        {
            return new UnityEngine.Vector2(positionX, positionY);
        }

        public UnityEngine.Vector2 GetMovementVector()
        {
            return new UnityEngine.Vector2(movementX, movementY);
        }

        public void Dispose()
        {
        }
    }
}