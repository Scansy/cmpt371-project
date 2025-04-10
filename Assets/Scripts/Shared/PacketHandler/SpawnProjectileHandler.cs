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
            var  spawnPacket = (SpawnProjectilePacket)packet;
            
            // Spawn the projectile
            Vector3 position = new Vector3(spawnPacket.spawnPosition.x, spawnPacket.spawnPosition.y, 0f);
            GameObject projectile = Instantiate(
                _projectilePrefab,
                position,
                Quaternion.Euler(0f, 0f, spawnPacket.startingRotation)
            );

            // Apply velocity
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = spawnPacket.velocity;
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