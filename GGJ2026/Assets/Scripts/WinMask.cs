using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinMask : MonoBehaviour
{
    private bool pickedUp;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (pickedUp) return;

        if (collision.CompareTag("Player"))
        {
            var player = collision.gameObject;
            pickedUp = true;

            // Attach mask to the player so it hovers/visually follows them (optional)
            transform.SetParent(player.transform);
            transform.localPosition = Vector3.zero;

            // Disable collider so it won't trigger again
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            // Find the GameManager instance and call HandleWin
            var gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                gameManager.HandleWin(player);
            }
            else
            {
                Debug.LogWarning("GameManager instance not found in the scene.");
            }
        }
    }
}
