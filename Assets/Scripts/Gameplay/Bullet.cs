using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;
    public float lifeTime = 2f;

    private float direction = 1f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(float dir)
    {
        direction = Mathf.Approximately(dir, 0f) ? 1f : Mathf.Sign(dir);

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;
    }

    void Update()
    {
        transform.position += Vector3.right * (direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();
            if (enemyHealth == null)
                enemyHealth = collision.GetComponentInParent<EnemyHealth>();

            if (enemyHealth != null)
                enemyHealth.TakeDamage(damage);

            Destroy(gameObject);
        }
        else if (!collision.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
