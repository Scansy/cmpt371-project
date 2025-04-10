using System;
using Shared.Packet;
using UnityEngine;

namespace Shared.PacketHandler
{
    public class PlayerShootPacketHandler : MonoBehaviour
    {
        public int playerId;  // This player's unique ID

        public GameObject bulletPrefab; // Bullet prefab to instantiate
        public float bulletSpeed = 10f; // Bullet speed

        // This method processes the shoot packet and fires the shot
        public void HandlePacket(IDisposable packet)
        {
            var playerShootPacket = (PlayerShootPacket)packet; 
            
            // Fire the bullet from the player's position, in the direction of the shot's angle
            FireBullet(playerShootPacket.shootPosition, playerShootPacket.shootAngle);
        }

        private void FireBullet(Vector2 position, float angle)
        {
            // Instantiate the bullet at the shoot position
            GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.Euler(0f, 0f, angle));

            // Get the bullet's Rigidbody2D and apply velocity in the direction of the shoot angle
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)); // Convert angle to direction
                rb.velocity = direction * bulletSpeed;  // Apply velocity to the bullet
            }

            //TODO: (optional) add a lifespan or other logic for the bullet here, like collision detection
        }
    }
}