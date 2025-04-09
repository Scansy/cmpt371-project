using UnityEngine;
using Shared.Packet;

public class SpawnProjectilePacketHandler : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab; // Assign this in the inspector

    public void HandlePacket(SpawnProjectilePacket packet)
    {
        // Spawn the projectile
        GameObject projectile = Instantiate(
            projectilePrefab,
            packet.spawnPosition,
            packet.startingRotation
        );

        // Apply velocity
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = packet.velocity;
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
