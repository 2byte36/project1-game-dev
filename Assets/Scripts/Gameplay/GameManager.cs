using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool hasKey = false;
    public GameObject keyIcon;
    public GameObject finishPanel;
    public Text infoText;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    void Start()
    {
        if (keyIcon != null)
            keyIcon.SetActive(false);

        if (finishPanel != null)
            finishPanel.SetActive(false);

        if (infoText != null)
            infoText.text = string.Empty;
    }

    public void CollectKey()
    {
        hasKey = true;

        if (keyIcon != null)
            keyIcon.SetActive(true);

        if (infoText != null)
            infoText.text = "Kunci berhasil diambil!";
    }

    public void FinishLevel()
    {
        if (finishPanel != null)
            finishPanel.SetActive(true);

        if (infoText != null)
            infoText.text = "Level Selesai!";
    }
}
