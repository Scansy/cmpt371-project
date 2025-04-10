using System;
using Shared.Packet;
using UnityEngine;

namespace Shared.PacketHandler
{
    public class PlayerMovementHandler : MonoBehaviour, IPacketHandler
    {
        private Rigidbody2D _rb;

        void Start()
        {
            _rb = GetComponent<Rigidbody2D>();  // Get the Rigidbody2D component
        }

        // Called to process incoming movement data
        public void HandlePacket(IDisposable packet)
        {
            Debug.Log("Movevement handler called");
            var playerMovementPacket = (PlayerMovementPacket)packet;
            
            // Update position
            transform.position = playerMovementPacket.position;
            
            // Update velocity (for physics-based movement)
            _rb.velocity = playerMovementPacket.velocity;
            
            // Update rotation (Z-axis only)
            transform.rotation = Quaternion.Euler(0f, 0f, playerMovementPacket.rotation);

            Debug.Log("Handler called successfully!");
        }
    }
}
