using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController3D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Vector2 moveInput;
    private Rigidbody rb;
    [SerializeField] private Camera playerCamera;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // These methods must be public and match the Input Actions' callback names exactly
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        //Debug.Log($"{gameObject.name} move input: {moveInput}");
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        Debug.Log($"{gameObject.name} interacted!");
    }

    private void FixedUpdate()
    {
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y) * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
        //Debug.Log($"Moving {gameObject.name} to {rb.position}");
    }

    // Add this method to enable or switch on the camera for this player
    public void SwitchOnCamera()
    {
        if (playerCamera != null)
        {
            playerCamera.enabled = true;
        }
        // Optionally, add logic to enable a specific camera or handle split-screen, etc.
    }
}
