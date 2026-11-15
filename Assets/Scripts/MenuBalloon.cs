using UnityEngine;

/// <summary>
/// MenuBalloon - Simple decorative balloon floating in the background of the main menu.
/// 
/// WHAT IT DOES:
///   Moves upward and sideways in a gentle wobble, then resets to the bottom
///   so the main menu always has balloons drifting across it.
/// 
/// WHY WE NEED IT:
///   We want the main menu to look lively with floating balloons in the
///   background. These balloons are purely decorative — the player cannot
///   click them. When a balloon reaches the top it teleports back to the
///   bottom at a new random X position.
/// 
/// WHICH GAMEOBJECT USES IT:
///   Each decorative balloon in the MainMenu scene.
/// </summary>
public class MenuBalloon : MonoBehaviour
{
    [Header("Movement")]
    public float floatSpeed  = 1.5f;
    public float wobbleSpeed = 1f;
    public float wobbleAmount = 0.3f;

    [Header("Boundaries")]
    public float topY    =  12f;
    public float bottomY = -12f;
    public float rangeX  =  8f;

    private float _wobbleOffset;

    void Start()
    {
        // Give each balloon a different wobble phase so they don't all move in sync
        _wobbleOffset = Random.Range(0f, Mathf.PI * 2f);
        
        // Randomize starting Y so they don't all appear at once
        float startY = Random.Range(bottomY, topY);
        transform.position = new Vector3(transform.position.x, startY, transform.position.z);
    }

    void Update()
    {
        // Float upward
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // Gentle left-right wobble using sine wave
        float wobbleX = Mathf.Sin(Time.time * wobbleSpeed + _wobbleOffset) * wobbleAmount;
        Vector3 pos = transform.position;
        pos.x += wobbleX * Time.deltaTime;
        transform.position = pos;

        // Reset to bottom when it reaches the top
        if (transform.position.y > topY)
        {
            Vector3 resetPos = transform.position;
            resetPos.y = bottomY;
            resetPos.x = Random.Range(-rangeX, rangeX);
            transform.position = resetPos;
        }
    }
}
