using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerStompAttack : MonoBehaviour
{
    public float bounceForce = 6f;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Enemy")) return;

        EnemyHealth enemyHealth = collision.gameObject.GetComponent<EnemyHealth>();
        if (enemyHealth == null) return;

        bool isFalling = rb.linearVelocity.y < 0f;
        bool isAboveEnemy = transform.position.y > collision.transform.position.y + 0.3f;

        if (isFalling && isAboveEnemy)
        {
            enemyHealth.Die();
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceForce);
        }
    }
}
