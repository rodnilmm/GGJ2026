using UnityEngine;
using UnityEngine.InputSystem;

public class InteractibleObject2D : MonoBehaviour
{
    [Header("UI Canvas to Activate")]
    public GameObject canvasToActivate;

    private bool playerInRange = false;

    // Reference to the player's input actions
    private PlayerInput playerInput;

    private void OnTriggerEnter2D (Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (canvasToActivate != null)
                canvasToActivate.SetActive(true);
            playerInRange = true;

            // Get PlayerInput from the player object
            playerInput = other.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.actions["Interact"].performed += OnInteract;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (canvasToActivate != null)
                canvasToActivate.SetActive(false);
            playerInRange = false;

            if (playerInput != null)
            {
                playerInput.actions["Interact"].performed -= OnInteract;
                playerInput = null;
            }
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (playerInRange)
        {
            // Place your interaction logic here
            Debug.Log("Player interacted with object!");
        }
    }
}
