using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinMask : MonoBehaviour
{
    [Tooltip("If set, the scene with this name will be loaded when a player picks up the mask. Leave empty to freeze the game instead.")]
    public string winSceneName = "EndingScene";

    [Tooltip("When loading a win scene, wait this many realtime seconds before switching.")]
    public float sceneLoadDelay = 0.5f;

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

            HandleWin(player);
        }
    }

    private void HandleWin(GameObject winner)
    {
        Debug.Log($"{winner.name} wins!");

        // Try enabling the winner's camera (if MovementController2D exposes that)
        var mc = winner.GetComponent<MovementController2D>();
        if (mc != null)
        {
            mc.SwitchOnCamera();
        }

        // Disable all players' MovementController2D to stop further input/movement
        var allControllers = FindObjectsOfType<MovementController2D>();
        foreach (var controller in allControllers)
        {
            controller.enabled = false;
        }

        // If a win scene is configured, load it after a short realtime delay (so timeScale changes don't affect waiting)
        if (!string.IsNullOrEmpty(winSceneName))
        {
            StartCoroutine(LoadWinSceneRealtime());
            return;
        }

        // Otherwise freeze the game and keep the mask attached to the winner
        Time.timeScale = 0f;
        // Consider showing UI here to announce the winner.
    }

    private IEnumerator LoadWinSceneRealtime()
    {
        // Use realtime wait so this works even if Time.timeScale is changed elsewhere
        yield return new WaitForSecondsRealtime(sceneLoadDelay);
        SceneManager.LoadScene(winSceneName);
    }
}
