using System;
using UnityEngine;

namespace Shared.PacketHandler
{
    public class UpdatePosClientHandler : MonoBehaviour, IPacketHandler 
    {
        public void HandlePacket(IDisposable packet)
        {
            var updatePositionPacket = (Packet.UpdatePosClientPacket)packet;


            
            transform.position = updatePositionPacket.Position;
            transform.rotation = Quaternion.Euler(0f, 0f, updatePositionPacket.Rotation);

            Debug.Log($"Sent to Client: Position: {updatePositionPacket.Position}, Rotation: {updatePositionPacket.Rotation}");

            // rb.velocity = new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed);

            // Vector2 aimDirection = mousePosition - rb.position;
            // float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;
            // rb.rotation = aimAngle;
        }
    }
}