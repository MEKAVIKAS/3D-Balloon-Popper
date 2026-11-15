using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// UIManager - Controls all on-screen UI elements during gameplay.
/// Auto-finds all UI elements by name so it works even if Inspector
/// references were not saved. This makes it resilient.
/// </summary>
public class UIManager : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────────────────────────
    public static UIManager Instance { get; private set; }

    // ── Inspector References (auto-found if null) ─────────────────────────────
    [Header("HUD Labels (auto-found if null)")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;

    [Header("Game Over Panel (auto-found if null)")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;

    [Header("Buttons (auto-found if null)")]
    public Button restartButton;
    public Button mainMenuButton;

    // ── Unity Lifecycle ────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Auto-find all UI elements by GameObject name
        if (scoreText    == null) scoreText    = FindTMP("ScoreText");
        if (timerText    == null) timerText    = FindTMP("TimerText");
        if (gameOverPanel== null) gameOverPanel= GameObject.Find("GameOverPanel");
        if (finalScoreText==null) finalScoreText= FindTMP("FinalScoreText");
        if (restartButton == null)
        {
            GameObject go = GameObject.Find("RestartButton");
            if (go != null) restartButton = go.GetComponent<Button>();
        }
        if (mainMenuButton == null)
        {
            GameObject go = GameObject.Find("MainMenuButton");
            if (go != null) mainMenuButton = go.GetComponent<Button>();
        }

        // Wire buttons
        if (restartButton  != null) { restartButton.onClick.RemoveAllListeners();  restartButton.onClick.AddListener(OnRestartClicked);  }
        if (mainMenuButton != null) { mainMenuButton.onClick.RemoveAllListeners(); mainMenuButton.onClick.AddListener(OnMainMenuClicked); }

        // Hide game-over panel at start
        ShowGameOverPanel(false);

        Debug.Log($"[UIManager] scoreText={scoreText != null}, timerText={timerText != null}, " +
                  $"gameOverPanel={gameOverPanel != null}, restartBtn={restartButton != null}");
    }

    // ── Public API ─────────────────────────────────────────────────────────────
    public void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = "Score: " + score;
    }

    public void UpdateTimer(float timeLeft)
    {
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(timeLeft);
            timerText.text  = "Time: " + seconds;
            timerText.color = (seconds <= 10) ? new Color(1f, 0.3f, 0.3f) : Color.white;
        }
    }

    public void ShowGameOverPanel(bool visible, int finalScore = 0)
    {
        if (gameOverPanel   != null) gameOverPanel.SetActive(visible);
        if (visible && finalScoreText != null) finalScoreText.text = "Final Score: " + finalScore;
    }

    // ── Button Handlers ────────────────────────────────────────────────────────
    private void OnRestartClicked()  => GameManager.Instance?.RestartGame();
    private void OnMainMenuClicked() => GameManager.Instance?.GoToMainMenu();

    // ── Helpers ───────────────────────────────────────────────────────────────
    private TextMeshProUGUI FindTMP(string goName)
    {
        GameObject go = GameObject.Find(goName);
        return go != null ? go.GetComponent<TextMeshProUGUI>() : null;
    }
}
