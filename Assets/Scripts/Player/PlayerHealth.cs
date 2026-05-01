using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    int currentHealth;
    Animator anim;
    Component playerRespawn;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        playerRespawn = GetComponent("PlayerRespawn");
        CacheHeartsIfNeeded();
        UpdateHeartsUI();
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);

        if (anim != null && HasAnimTrigger(anim, "hit"))
            anim.SetTrigger("hit");

        UpdateHeartsUI();

        if (currentHealth <= 0)
            Die();
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateHeartsUI();
    }

    void UpdateHeartsUI()
    {
        if (hearts == null) return;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;

            bool isFull = i < currentHealth;
            hearts[i].sprite = isFull ? fullHeart : emptyHeart;
            hearts[i].color = isFull || emptyHeart != null
                ? Color.white
                : new Color(1f, 1f, 1f, 0.35f);
            hearts[i].enabled = i < maxHealth;
        }
    }

    void CacheHeartsIfNeeded()
    {
        if (hearts != null && hearts.Length > 0) return;

        GameObject uiRoot = GameObject.Find("Canvas/PlayerHealthUI");
        if (uiRoot == null) return;

        hearts = new Image[3];
        for (int i = 0; i < hearts.Length; i++)
        {
            Transform heart = uiRoot.transform.Find($"Heart{i + 1}");
            hearts[i] = heart != null ? heart.GetComponent<Image>() : null;
        }
    }

    void Die()
    {
        if (GameManager.instance != null)
            GameManager.instance.ShowGameOver();
        else if (playerRespawn != null)
            playerRespawn.SendMessage("RespawnPlayer", SendMessageOptions.DontRequireReceiver);
    }

    static bool HasAnimTrigger(Animator animator, string triggerName)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
            if (parameter.type == AnimatorControllerParameterType.Trigger && parameter.name == triggerName)
                return true;

        return false;
    }
}
