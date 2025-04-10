using System;
using Shared.Packet;
using UnityEngine;

namespace Shared.PacketHandler
{
    public class PlayerMovementHandler : MonoBehaviour, IPacketHandler
    {
        private Rigidbody2D _rb;  // Assuming you are using a Rigidbody2D for physics-based movement

        void Start()
        {
            _rb = GetComponent<Rigidbody2D>();  // Get the Rigidbody2D component
        }

        // Called to process incoming movement data
        public void HandlePacket(IDisposable packet)
        {
            var playerMovementPacket = (PlayerMovementPacket)packet;
            
            // Update position
            transform.position = playerMovementPacket.position;
            
            // Update velocity (for physics-based movement)
            _rb.velocity = playerMovementPacket.velocity;
            
            // Update rotation (Z-axis only)
            transform.rotation = Quaternion.Euler(0f, 0f, playerMovementPacket.rotation);
        }
    }
}
