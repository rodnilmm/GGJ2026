using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawnScript : MonoBehaviour
{
    public Transform[] SpawnPoints;
    private int m_playerCount;

    // Define a set of specific RGBA colors for players
    private readonly Color[] playerColors = new Color[]
    {
        new Color(232f / 255f, 59f / 255f, 59f / 255f, 1f),   // Red
        new Color(77f / 255f, 101f / 255f, 180f / 255f, 1f), // Blue
        new Color(30f / 255f, 188f / 255f, 115f / 255f, 1f), // Green
        // new Color(0.95f, 0.85f, 0.2f, 1f),                 // Yellow (commented out)
        new Color(131f / 255f, 28f / 255f, 93f / 255f, 1f),  // Magenta
        new Color(0.2f, 0.9f, 0.9f, 1f)                      // Cyan
    };

    // Name of the child GameObject whose sprite color should be changed
    [SerializeField] private string colorChildName = "Ropa";
    [SerializeField] private string colorChildName2 = "Camera";
    [SerializeField] private string colorChildName3 = "PlayerFrame";

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        // Place the player at the next spawn point (keep existing behavior)
        playerInput.transform.position = SpawnPoints[m_playerCount].transform.position;

        // Give the spawned player a distinct name (Player 1, Player 2, ...)
        playerInput.gameObject.name = $"Player {m_playerCount + 1}";

        // Find the direct child (e.g. "Ropa")
        Transform colorChild = playerInput.transform.Find(colorChildName);

        // Find the Camera child, then the nested PlayerFrame under that camera
        Transform cameraChild = playerInput.transform.Find(colorChildName2);
        Transform playerFrameChild = null;
        if (cameraChild != null)
        {
            playerFrameChild = cameraChild.Find(colorChildName3);
        }
        else
        {
            // fallback: try a path search (in case hierarchy differs)
            playerFrameChild = playerInput.transform.Find($"{colorChildName2}/{colorChildName3}");
        }

        // Resolve SpriteRenderers safely and apply color
        var color = playerColors[m_playerCount % playerColors.Length];
        SpriteRenderer sr = colorChild != null ? colorChild.GetComponent<SpriteRenderer>() : null;
        SpriteRenderer srFrame = playerFrameChild != null ? playerFrameChild.GetComponent<SpriteRenderer>() : null;

        if (sr != null)
            sr.color = color;
        if (srFrame != null)
            srFrame.color = color;

        m_playerCount++;
    }
}
