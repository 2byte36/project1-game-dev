using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;
    public Slider healthSlider;
    public GameObject healthUI;
    private bool isDead;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (healthUI != null)
            healthUI.SetActive(false);
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        if (healthUI != null)
            healthUI.SetActive(true);

        if (currentHealth <= 0)
            Die();
    }

    public void Die()
    {
        if (isDead)
            return;

        isDead = true;
        Destroy(gameObject);
    }
}
