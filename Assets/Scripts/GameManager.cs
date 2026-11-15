using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager - The brain of the entire game.
/// 
/// WHAT IT DOES:
///   Tracks the score, runs the 60-second countdown timer, decides when
///   the game is over, and tells the UI to update itself.
/// 
/// WHY WE NEED IT:
///   We need one central place that knows the "state" of the game
///   (playing, game-over). Every other script can ask GameManager
///   "is the game still running?" before doing anything important.
/// 
/// HOW IT WORKS:
///   - Uses the Singleton pattern so any script can reach it via
///     GameManager.Instance without needing a direct reference.
///   - Each frame it counts down the timer while the game is active.
///   - When time reaches 0 it fires OnGameOver() which shows the
///     Game-Over panel and stops spawning.
/// 
/// WHICH GAMEOBJECT USES IT:
///   An empty GameObject called "GameManager" in the Game Scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────────────────
    public static GameManager Instance { get; private set; }

    // ── Inspector Fields ─────────────────────────────────────────────────────
    [Header("Game Settings")]
    [Tooltip("Total seconds for one round")]
    public float gameDuration = 60f;

    // ── State ─────────────────────────────────────────────────────────────────
    public bool IsGameActive { get; private set; } = false;

    private int   _score     = 0;
    private float _timeLeft  = 0f;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────
    void Awake()
    {
        // Singleton setup: only one GameManager may exist
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        StartGame();
    }

    void Update()
    {
        if (!IsGameActive) return;

        // Count down the timer every frame
        _timeLeft -= Time.deltaTime;

        // Clamp so it never goes below zero
        _timeLeft = Mathf.Max(_timeLeft, 0f);

        // Tell the UI the new time value
        UIManager.Instance?.UpdateTimer(_timeLeft);

        // End the game when time is up
        if (_timeLeft <= 0f)
        {
            EndGame();
        }
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>Called once when the game scene loads to begin a fresh round.</summary>
    public void StartGame()
    {
        _score        = 0;
        _timeLeft     = gameDuration;
        IsGameActive  = true;

        UIManager.Instance?.UpdateScore(_score);
        UIManager.Instance?.UpdateTimer(_timeLeft);
        UIManager.Instance?.ShowGameOverPanel(false);
    }

    /// <summary>
    /// Called by BalloonController when the player successfully clicks a balloon.
    /// Adds 1 to the score and refreshes the score UI.
    /// </summary>
    public void AddScore()
    {
        if (!IsGameActive) return;

        _score++;
        UIManager.Instance?.UpdateScore(_score);
    }

    /// <summary>Stops the game and shows the Game-Over panel.</summary>
    public void EndGame()
    {
        if (!IsGameActive) return;   // Guard against double-call

        IsGameActive = false;

        // Show the final score on the game-over panel
        UIManager.Instance?.ShowGameOverPanel(true, _score);

        // Destroy all remaining balloons in the scene
        foreach (var balloon in GameObject.FindGameObjectsWithTag("balloon"))
        {
            Destroy(balloon);
        }
    }

    /// <summary>Reload the game scene to restart from scratch.</summary>
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>Go back to the Main Menu scene.</summary>
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
