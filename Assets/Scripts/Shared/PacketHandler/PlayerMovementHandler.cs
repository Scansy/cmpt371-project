using UnityEngine;

public class PlayerMovementHandler : MonoBehaviour
{
    private Rigidbody2D rb;  // Assuming you are using a Rigidbody2D for physics-based movement

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();  // Get the Rigidbody2D component
    }

    // Called to process incoming movement data
    public void PlayerMovementHandler(PlayerMovementPacket packet)
    {
        // Update position
        transform.position = packet.position;
        
        // Update velocity (for physics-based movement)
        rb.velocity = packet.velocity;
        
        // Update rotation (Z-axis only)
        transform.rotation = Quaternion.Euler(0f, 0f, packet.rotation);

    }
}
