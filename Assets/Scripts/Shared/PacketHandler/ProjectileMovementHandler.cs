using System;
using Shared.Packet;
using UnityEngine;

namespace Shared.PacketHandler
{
    public class ProjectileMovementHandler : MonoBehaviour, IPacketHandler
    {
        private Rigidbody2D _rb;  // Assuming you are using a Rigidbody2D for physics-based movement

        void Start()
        {
            _rb = GetComponent<Rigidbody2D>();  // Get the Rigidbody2D component
        }

        // Called to process incoming movement data
        public void HandlePacket(IDisposable packet)
        {
            var projectileMovementPacket = (ProjectileMovementPacket)packet;
            
            // Update position
            transform.position = projectileMovementPacket.position;
            
            // Update velocity (for physics-based movement)
            _rb.velocity = projectileMovementPacket.velocity;
            
            // Update rotation (Z-axis only)
            transform.rotation = Quaternion.Euler(0f, 0f, projectileMovementPacket.rotation);
        }
    }
}
