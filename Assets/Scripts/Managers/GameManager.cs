using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    public int startLives = 3;
    public float gameDuration = 60f;

    [Header("Current Game State")]
    public int currentScore = 0;
    public int currentLives;
    public float currentTime;
    public bool isGameActive = true;

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI livesText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;

    private int highScore = 0;
    private Coroutine timerCoroutine;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Load high score
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    void Start()
    {
        InitializeGame();
    }

    void InitializeGame()
    {
        currentScore = 0;
        currentLives = startLives;
        currentTime = gameDuration;
        isGameActive = true;

        // Find UI elements if not assigned
        FindUIElements();

        // Start game timer
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(GameTimer());

        // Hide game over panel
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        UpdateUI();

        Debug.Log("Game Started! Lives: " + currentLives + " Time: " + currentTime);
    }

    void FindUIElements()
    {
        if (scoreText == null) scoreText = GameObject.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
        if (timeText == null) timeText = GameObject.Find("TimeText")?.GetComponent<TextMeshProUGUI>();
        if (livesText == null) livesText = GameObject.Find("LivesText")?.GetComponent<TextMeshProUGUI>();
        if (gameOverPanel == null) gameOverPanel = GameObject.Find("GameOverPanel");

        if (gameOverPanel != null)
        {
            finalScoreText = gameOverPanel.transform.Find("FinalScoreText")?.GetComponent<TextMeshProUGUI>();
            highScoreText = gameOverPanel.transform.Find("HighScoreText")?.GetComponent<TextMeshProUGUI>();
        }
    }

    IEnumerator GameTimer()
    {
        while (currentTime > 0 && isGameActive)
        {
            yield return new WaitForSeconds(1f);
            currentTime -= 1f;
            UpdateUI();

            if (currentTime <= 0)
            {
                GameOver("Time's Up!");
            }
        }
    }

    public void AddScore(int points)
    {
        if (!isGameActive) return;

        currentScore += points;
        UpdateUI();

        // Update high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }
    }

    public void PlayerDied()
    {
        if (!isGameActive) return;

        currentLives--;
        UpdateUI();

        if (currentLives <= 0)
        {
            GameOver("No Lives Left!");
        }
        else
        {
            // Respawn player
            StartCoroutine(RespawnPlayer());
        }
    }

    IEnumerator RespawnPlayer()
    {
        Debug.Log("Respawning player...");

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Ẩn tạm thời sprite & collider, không tắt toàn object
            SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
            Collider2D col = player.GetComponent<Collider2D>();
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

            if (sr != null) sr.enabled = false;
            if (col != null) col.enabled = false;
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            // Đợi 2 giây trước khi respawn
            yield return new WaitForSeconds(2f);

            // Reset vị trí
            player.transform.position = Vector3.zero;

            // Bật lại sprite & collider
            if (sr != null) sr.enabled = true;
            if (col != null) col.enabled = true;

            // Hiệu ứng bất tử tạm thời
            StartCoroutine(InvincibilityEffect(player));

            Debug.Log("Player respawn complete!");
        }
    }

    IEnumerator InvincibilityEffect(GameObject player)
    {
        SpriteRenderer sprite = player.GetComponent<SpriteRenderer>();
        Color originalColor = sprite.color;
        float invincibilityTime = 3f;
        float elapsedTime = 0f;

        while (elapsedTime < invincibilityTime)
        {
            // Nhấp nháy
            sprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f);
            yield return new WaitForSeconds(0.1f);
            sprite.color = originalColor;
            yield return new WaitForSeconds(0.1f);

            elapsedTime += 0.2f;
        }

        sprite.color = originalColor;
    }

    void GameOver(string reason)
    {
        isGameActive = false;

        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        // Show game over UI
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (finalScoreText != null)
                finalScoreText.text = "SCORE: " + currentScore;
            if (highScoreText != null)
                highScoreText.text = "HIGH SCORE: " + highScore;
        }

        Debug.Log("Game Over: " + reason);
    }

    void UpdateUI()
    {
        Debug.Log($"[UI UPDATE] Score={currentScore}, Time={currentTime}, Lives={currentLives}");

        if (scoreText != null)
            scoreText.text = "SCORE: " + currentScore.ToString("000000");

        if (timeText != null)
            timeText.text = "TIME: " + Mathf.CeilToInt(currentTime).ToString("000");

        if (livesText != null)
            livesText.text = ""+ currentLives;
    }

    // Public methods for UI buttons
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}