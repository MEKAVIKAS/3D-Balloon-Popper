using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// MainMenuManager - Controls the Main Menu scene.
/// 
/// This version finds its own buttons by name in Start() so it works
/// even if Inspector references were not saved correctly in the scene.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Optional: assign in Inspector (auto-found if null)")]
    public Button startGameButton;
    public Button quitButton;

    void Start()
    {
        // Auto-find buttons by name if Inspector refs are missing
        if (startGameButton == null)
        {
            GameObject go = GameObject.Find("StartGameButton");
            if (go != null) startGameButton = go.GetComponent<Button>();
        }
        if (quitButton == null)
        {
            GameObject go = GameObject.Find("QuitButton");
            if (go != null) quitButton = go.GetComponent<Button>();
        }

        // Wire up click listeners
        if (startGameButton != null)
        {
            startGameButton.onClick.RemoveAllListeners();
            startGameButton.onClick.AddListener(OnStartGameClicked);
            Debug.Log("[MainMenuManager] StartGameButton found and wired.");
        }
        else
        {
            Debug.LogWarning("[MainMenuManager] StartGameButton NOT found!");
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(OnQuitClicked);
        }
    }

    private void OnStartGameClicked()
    {
        Debug.Log("[MainMenuManager] Loading GameScene...");
        SceneManager.LoadScene("GameScene");
    }

    private void OnQuitClicked()
    {
        Debug.Log("[MainMenuManager] Quitting...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
