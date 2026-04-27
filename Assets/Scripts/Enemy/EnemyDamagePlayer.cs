using System.Reflection;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyDamagePlayer : MonoBehaviour
{
    public int damage = 1;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        Component playerHealth = collision.gameObject.GetComponent("PlayerHealth");
        if (playerHealth == null) return;

        Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
        bool playerIsFalling = playerRb != null && playerRb.linearVelocity.y < 0f;
        bool playerIsAboveEnemy = collision.transform.position.y > transform.position.y + 0.3f;

        if (playerIsFalling && playerIsAboveEnemy) return;

        MethodInfo takeDamage = playerHealth.GetType().GetMethod(
            "TakeDamage",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new[] { typeof(int) },
            null);

        if (takeDamage != null)
            takeDamage.Invoke(playerHealth, new object[] { damage });
    }
}
