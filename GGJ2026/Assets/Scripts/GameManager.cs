using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Tooltip("If set, the scene with this name will be loaded when a player picks up the mask. Leave empty to freeze the game instead.")]
    public string winSceneName = "MainMenu";

    [Tooltip("When loading a win scene, wait this many realtime seconds before switching.")]
    public float sceneLoadDelay = 7f;
    [SerializeField] TextMeshProUGUI winText;
    [SerializeField] Canvas winCanvas;
    public void HandleWin(GameObject winner)
    {
        Debug.Log($"{winner.name} wins!");

        // Try enabling the winner's camera (if MovementController2D exposes that)
        var mc = winner.GetComponent<MovementController2D>();
        if (mc != null)
        {
            mc.SwitchOnCamera();
        }

        // Disable all players' MovementController2D to stop further input/movement
        var allControllers = FindObjectsByType<MovementController2D>(FindObjectsSortMode.None);
        foreach (var controller in allControllers)
        {
            controller.enabled = false;
        }

        // Show win text if assigned
        if (winText != null)
        {
            winText.text = $"{winner.name} wins!";
            winCanvas.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("GameManager.winText is not assigned. Assign a TextMeshProUGUI in the inspector to show the winner.");
        }

        // If a win scene is configured, load it after a short realtime delay (so timeScale changes don't affect waiting)
        if (!string.IsNullOrEmpty(winSceneName))
        {
            StartCoroutine(LoadWinSceneRealtime());
            return;
        }

        // Otherwise freeze the game and keep the mask attached to the winner
        Time.timeScale = 0f;
        // Consider showing additional UI here to announce the winner.
    }

    private IEnumerator LoadWinSceneRealtime()
    {
        // Use realtime wait so this works even if Time.timeScale is changed elsewhere
        yield return new WaitForSecondsRealtime(sceneLoadDelay);
        SceneManager.LoadScene(winSceneName);
    }
}
