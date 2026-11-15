using UnityEngine;

/// <summary>
/// BalloonController - Attached to every balloon in the game.
/// 
/// WHAT IT DOES:
///   Makes the balloon float upward, destroys it if it reaches the top,
///   and handles mouse clicks so the balloon pops.
/// 
/// HOW IT WORKS:
///   - In Update() the balloon moves up by floatSpeed every frame.
///   - OnMouseDown() is called by Unity automatically when the player
///     clicks this GameObject's collider. Works because the project
///     now uses "Both" Input Systems mode.
/// 
/// WHICH GAMEOBJECT USES IT:
///   Every Balloon prefab instance (spawned by BalloonSpawner).
/// </summary>
[RequireComponent(typeof(Collider))]
public class BalloonController : MonoBehaviour
{
    // ── Inspector Fields ───────────────────────────────────────────────────────
    [Header("Movement")]
    public float floatSpeed = 2f;

    [Header("Boundaries")]
    public float destroyYPosition = 12f;

    [Header("Effects")]
    public GameObject popEffectPrefab;
    public AudioClip  popSound;

    // ── Private state ──────────────────────────────────────────────────────────
    private bool _popped = false;

    // ── Unity Lifecycle ────────────────────────────────────────────────────────
    void Update()
    {
        // Float upward every frame
        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime, Space.World);

        // Auto-destroy when off the top of screen
        if (transform.position.y >= destroyYPosition)
            Destroy(gameObject);
    }

    /// <summary>
    /// Unity calls this when the player left-clicks this collider.
    /// Works with "Both" input systems (activeInputHandler: 2).
    /// </summary>
    void OnMouseDown()
    {
        if (_popped) return;
        if (GameManager.Instance == null || !GameManager.Instance.IsGameActive) return;
        Pop();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private void Pop()
    {
        _popped = true;

        // Particle burst
        if (popEffectPrefab != null)
        {
            GameObject effect = Instantiate(popEffectPrefab, transform.position, Quaternion.identity);
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                Renderer rend = GetComponent<Renderer>();
                if (rend != null)
                {
                    var main = ps.main;
                    main.startColor = rend.material.color;
                }
            }
            Destroy(effect, 2f);
        }

        // Sound
        if (popSound != null)
            AudioSource.PlayClipAtPoint(popSound, transform.position, 0.8f);

        // Award score
        GameManager.Instance.AddScore();

        // Remove balloon
        Destroy(gameObject);
    }
}
