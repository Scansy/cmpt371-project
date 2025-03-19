using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);

        // Implement game logic where bullet hit enemy here      
    }

    void OnBecomeInvisible() {
        Destroy(gameObject);
    }
}
