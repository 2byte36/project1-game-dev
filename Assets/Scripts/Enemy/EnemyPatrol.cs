using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol : MonoBehaviour
{
    public float moveSpeed = 2f;
    public bool movingRight = true;
    public Transform groundCheck;
    public Transform wallCheck;
    public float groundCheckRadius = 0.1f;
    public float wallCheckRadius = 0.1f;
    public LayerMask groundLayer;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (groundCheck == null || wallCheck == null)
            return;

        bool groundAhead = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        bool wallAhead = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, groundLayer);

        if (!groundAhead || wallAhead)
            Flip();

        float direction = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    void Flip()
    {
        movingRight = !movingRight;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (movingRight ? 1f : -1f);
        transform.localScale = scale;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
        }
    }
}
