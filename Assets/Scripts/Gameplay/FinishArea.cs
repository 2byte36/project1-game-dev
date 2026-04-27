using UnityEngine;

[DisallowMultipleComponent]
public class FinishArea : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (GameManager.instance != null && GameManager.instance.hasKey)
            GameManager.instance.FinishLevel();
        else
            Debug.Log("Ambil kunci dulu sebelum masuk rumah!");
    }
}
