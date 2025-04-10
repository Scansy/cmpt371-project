using System;
using Shared.Packet;
using UnityEngine;

namespace Shared.PacketHandler
{
    public class SpawnProjectileHandler : MonoBehaviour, IPacketHandler
    {
        [SerializeField] private GameObject _projectilePrefab; // Assign this in the inspector

        public void HandlePacket(IDisposable packet)
        {
            var  projectileMovementPacket = (ProjectileMovementPacket)packet;
            
            // Spawn the projectile
            GameObject projectile = Instantiate(
                _projectilePrefab,
                projectileMovementPacket.position,
                Quaternion.Euler(0f, 0f, projectileMovementPacket.rotation)
            );

            // Apply velocity
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = projectileMovementPacket.velocity;
            }
        }

        // USE CASE EXAMPLE:::::
        /* SpawnProjectilePacket packet = new SpawnProjectilePacket(
            transform.position,
            transform,
            new Vector2(5f, 0f) // Example velocity
        );

        spawnProjectilePacketHandler.HandlePacket(packet);
        */
    }
}