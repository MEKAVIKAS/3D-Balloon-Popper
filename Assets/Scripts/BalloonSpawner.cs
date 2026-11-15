using UnityEngine;

/// <summary>
/// BalloonSpawner - Factory that creates balloons at random positions over time.
/// 
/// WHAT IT DOES:
///   Waits for a configurable interval, then spawns one balloon from the
///   Balloon prefab at a random X position along the bottom of the screen.
///   It assigns a random color material, a random float speed, and passes
///   the pop-effect prefab and sound to each new balloon.
/// 
/// WHY WE NEED IT:
///   We want a continuous stream of colourful balloons appearing at unpredictable
///   positions so the game is different every time. Having a dedicated spawner
///   keeps the spawning logic separate from the balloon's own movement logic.
/// 
/// HOW IT WORKS:
///   - Uses InvokeRepeating() to call SpawnBalloon() at a set interval.
///   - Checks GameManager.IsGameActive before spawning — no balloons after
///     the game ends.
///   - Randomly picks a material from the balloonMaterials array and applies
///     it to the new balloon's Renderer.
/// 
/// WHICH GAMEOBJECT USES IT:
///   An empty GameObject called "BalloonSpawner" in the Game Scene.
/// </summary>
public class BalloonSpawner : MonoBehaviour
{
    // ── Inspector Fields ───────────────────────────────────────────────────────
    [Header("Balloon Setup")]
    [Tooltip("The Balloon prefab to instantiate")]
    public GameObject balloonPrefab;

    [Tooltip("URP materials for each balloon colour (Red, Blue, Yellow, Green, Purple)")]
    public Material[] balloonMaterials;

    [Tooltip("Pop particle effect prefab")]
    public GameObject popEffectPrefab;

    [Tooltip("Pop sound audio clip")]
    public AudioClip popSound;

    [Header("Spawning")]
    [Tooltip("Seconds between each spawn")]
    public float spawnInterval = 1.2f;

    [Tooltip("How far left/right balloons can appear")]
    public float spawnRangeX = 7f;

    [Tooltip("Y position where balloons appear (bottom)")]
    public float spawnY = -8f;

    [Tooltip("Z depth of balloons in the scene")]
    public float spawnZ = 0f;

    [Header("Float Speed")]
    [Tooltip("Minimum float speed for a balloon")]
    public float minSpeed = 1.5f;

    [Tooltip("Maximum float speed for a balloon")]
    public float maxSpeed = 4.5f;

    // ── Unity Lifecycle ────────────────────────────────────────────────────────
    void Start()
    {
        // Start spawning immediately, then every spawnInterval seconds
        InvokeRepeating(nameof(SpawnBalloon), 0.3f, spawnInterval);
    }

    // ── Spawn Logic ────────────────────────────────────────────────────────────

    /// <summary>Creates one balloon at a random X position.</summary>
    private void SpawnBalloon()
    {
        // Don't spawn if game is not active
        if (GameManager.Instance == null || !GameManager.Instance.IsGameActive)
        {
            CancelInvoke(nameof(SpawnBalloon));
            return;
        }

        if (balloonPrefab == null)
        {
            Debug.LogWarning("BalloonSpawner: balloonPrefab is not assigned!");
            return;
        }

        // Random horizontal position
        float randomX = Random.Range(-spawnRangeX, spawnRangeX);
        Vector3 spawnPos = new Vector3(randomX, spawnY, spawnZ);

        // Create the balloon
        GameObject newBalloon = Instantiate(balloonPrefab, spawnPos, Quaternion.identity);

        // ── Assign random material / colour ──────────────────────────────────
        if (balloonMaterials != null && balloonMaterials.Length > 0)
        {
            int matIndex = Random.Range(0, balloonMaterials.Length);
            Renderer rend = newBalloon.GetComponent<Renderer>();
            if (rend != null && balloonMaterials[matIndex] != null)
            {
                // Use an instance so changing color doesn't affect other balloons
                rend.material = balloonMaterials[matIndex];
            }
        }

        // ── Assign random float speed ─────────────────────────────────────────
        BalloonController controller = newBalloon.GetComponent<BalloonController>();
        if (controller != null)
        {
            controller.floatSpeed     = Random.Range(minSpeed, maxSpeed);
            controller.popEffectPrefab = popEffectPrefab;
            controller.popSound        = popSound;
        }
    }
}
