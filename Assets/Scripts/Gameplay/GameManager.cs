using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool hasKey = false;
    public bool levelComplete = false;
    public GameObject keyIcon;
    public GameObject finishPanel;
    public GameObject gameOverPanel;
    public Text infoText;
    public PlayerRespawn playerRespawn;

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

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

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
        levelComplete = true;

        if (finishPanel != null)
            finishPanel.SetActive(true);

        if (infoText != null)
            infoText.text = "Level Selesai!";
    }

    public void ShowGameOver()
    {
        if (levelComplete) return;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void RetryButton()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (playerRespawn != null)
            playerRespawn.RespawnPlayer();

        Time.timeScale = 1f;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
