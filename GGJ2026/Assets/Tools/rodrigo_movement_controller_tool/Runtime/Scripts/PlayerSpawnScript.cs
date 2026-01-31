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
        new Color(77f / 255f, 101f / 255f, 180f / 255f, 1f),                      // Blue
        new Color(30f / 255f, 188f / 255f, 115f / 255f, 1f),                      // Green
        // new Color(0.95f, 0.85f, 0.2f, 1f),                 // Yellow (commented out)
        new Color(131f / 255f, 28f / 255f, 93f / 255f, 1f),                      // Magenta
        new Color(0.2f, 0.9f, 0.9f, 1f)                       // Cyan
    };

    // Name of the child GameObject whose sprite color should be changed
    [SerializeField] private string colorChildName = "Ropa";

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        playerInput.transform.position = SpawnPoints[m_playerCount].transform.position;

        // Find the child GameObject by name and change its SpriteRenderer color
        Transform colorChild = playerInput.transform.Find(colorChildName);
        if (colorChild != null)
        {
            SpriteRenderer sr = colorChild.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = playerColors[m_playerCount % playerColors.Length];
            }
        }

        m_playerCount++;
    }
}
