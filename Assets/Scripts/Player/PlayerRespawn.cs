using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerRespawn : MonoBehaviour
{
    public Transform respawnPoint;

    Rigidbody2D rb;
    PlayerHealth playerHealth;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();
        FindRespawnPointIfNeeded();
    }

    public void RespawnPlayer()
    {
        FindRespawnPointIfNeeded();
        if (respawnPoint == null) return;

        transform.position = respawnPoint.position;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (playerHealth != null)
            playerHealth.ResetHealth();
    }

    void FindRespawnPointIfNeeded()
    {
        if (respawnPoint != null) return;

        GameObject foundRespawnPoint = GameObject.Find("RespawnPoint");
        if (foundRespawnPoint != null)
            respawnPoint = foundRespawnPoint.transform;
    }
}
