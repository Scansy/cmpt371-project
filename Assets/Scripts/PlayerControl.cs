using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing.Security;

public class PlayerControl : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Rigidbody2D rb;
    public Weapon weapon;
    public float respawnTime = 3f;
    public Vector2[] spawnPoints = new Vector2[3] {
        new Vector2(-5, 0),    // Player 1 spawn
        new Vector2(0, 0),     // Player 2 spawn
        new Vector2(5, 0)      // Player 3 spawn
    };

    private bool isDead = false;
    private Vector2 moveDirection;
    private Vector2 mousePosition;
    private Collider2D playerCollider;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        playerCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isDead) return;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        if (Input.GetMouseButtonDown(0)) {
            weapon.Fire();
        }

        moveDirection = new Vector2(moveX, moveY).normalized;
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        rb.velocity = new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed);

        Vector2 aimDirection = mousePosition - rb.position;
        float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = aimAngle;
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        playerCollider.enabled = false;
        spriteRenderer.enabled = false;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // Start respawn coroutine
        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);

        // Get spawn point based on player number
        int playerNumber = GetPlayerNumber();
        Vector2 spawnPoint = spawnPoints[playerNumber - 1];

        // Reset position and state
        transform.position = spawnPoint;
        isDead = false;
        playerCollider.enabled = true;
        spriteRenderer.enabled = true;
    }

    private int GetPlayerNumber()
    {
        string playerName = gameObject.name.ToLower();
        if (playerName.Contains("1") || playerName.Contains("one")) return 1;
        if (playerName.Contains("2") || playerName.Contains("two")) return 2;
        if (playerName.Contains("3") || playerName.Contains("three")) return 3;
        return 1; // Default to player 1 if can't determine
    }
}
