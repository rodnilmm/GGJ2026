using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // New Input System namespace

public class SceneLoader2 : MonoBehaviour
{
    [Header("Scene Loading Settings")]
    public float waitSeconds = 3f; // Time to wait before loading next scene
    [SerializeField] private Animator transitionAnimator; // Animator for transition effect
    [SerializeField] private AudioSource beastSFX; // Audio source for sound effect

    private bool inputDetected = false;

    void Start()
    {
        StartCoroutine(SceneStartSequence());
    }

    void Update()
    {
        // Defensive: Ensure input system is initialized and devices are available
        if (inputDetected) return;

        bool keyboardPressed = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
        bool mousePressed = Mouse.current != null && (
            Mouse.current.leftButton.wasPressedThisFrame ||
            Mouse.current.rightButton.wasPressedThisFrame ||
            Mouse.current.middleButton.wasPressedThisFrame
        );
        bool gamepadPressed = Gamepad.current != null && (
            Gamepad.current.buttonSouth.wasPressedThisFrame ||
            Gamepad.current.buttonNorth.wasPressedThisFrame ||
            Gamepad.current.buttonEast.wasPressedThisFrame ||
            Gamepad.current.buttonWest.wasPressedThisFrame ||
            Gamepad.current.startButton.wasPressedThisFrame ||
            Gamepad.current.selectButton.wasPressedThisFrame ||
            Gamepad.current.leftShoulder.wasPressedThisFrame ||
            Gamepad.current.rightShoulder.wasPressedThisFrame ||
            Gamepad.current.leftStickButton.wasPressedThisFrame ||
            Gamepad.current.rightStickButton.wasPressedThisFrame
        );

        if (keyboardPressed || mousePressed || gamepadPressed)
        {
            inputDetected = true;
            beastSFX.Play();
            StartCoroutine(LoadNextSceneAfterDelay());
        }
    }

    private IEnumerator SceneStartSequence()
    {
        yield return new WaitForSeconds(0.5f); // Wait for crossfade (adjust as needed)
        // Wait for input before loading next scene
    }

    private IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(waitSeconds);
        if (transitionAnimator != null)
        {
            transitionAnimator.SetTrigger("Start");
        }
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("No next scene in build settings.");
        }
    }
}
