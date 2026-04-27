using UnityEngine;

[DisallowMultipleComponent]
public class KeyPickup : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (GameManager.instance != null)
            GameManager.instance.CollectKey();

        Destroy(gameObject);
    }
}
