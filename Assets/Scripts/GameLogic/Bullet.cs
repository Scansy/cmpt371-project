using UnityEngine;

namespace GameLogic
{
    public class Bullet : MonoBehaviour
    {
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Check if we hit a player
            if (collision.gameObject.CompareTag("Player"))
            {
                // Get the PlayerControl component and trigger death
                PlayerControl player = collision.gameObject.GetComponent<PlayerControl>();
                if (player != null)
                {
                    player.Die();
                }
            }

            // Destroy the bullet regardless of what it hit
            Destroy(gameObject);
        }

        void Update()
        {
            Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);
            if (viewPos.x < 0 || viewPos.x > 1 || viewPos.y < 0 || viewPos.y > 1)
            {
                Destroy(gameObject);
            }
        }
    }
} 
