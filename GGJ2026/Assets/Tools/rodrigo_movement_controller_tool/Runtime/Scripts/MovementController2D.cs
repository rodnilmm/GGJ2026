using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class MovementController2D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float faceX;
    public float faceY;
    public Animator[] anim;
    private Vector2 moveInput;
    private Rigidbody2D rb;
    [SerializeField] private Camera playerCamera;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // These methods must be public and match the Input Actions' callback names exactly
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        moveInput = input;

        // Only update facing direction if there is input
        if (input.sqrMagnitude > 0.01f)
        {
            faceX = input.x;
            faceY = input.y;
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        Debug.Log($"{gameObject.name} interacted!");
    }

    private void FixedUpdate()
    {
        Vector2 move = moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        // Always set animator parameters to last facing direction
        for (int i = 0; i < anim.Length; i++)
        {
            anim[i].SetFloat("x", faceX);
            anim[i].SetFloat("y", faceY);
        }
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

    public void ApplySpeedBuff(float newSpeed, float duration)
    {
        StartCoroutine(SpeedBuffCoroutine(newSpeed, duration));
    }

    private IEnumerator SpeedBuffCoroutine(float newSpeed, float duration)
    {
        float originalSpeed = moveSpeed;
        moveSpeed = newSpeed;
        yield return new WaitForSeconds(duration);
        moveSpeed = originalSpeed;
        Debug.Log($"Player speed reverted to {originalSpeed}");
        //SpeedUp.EndBuff();
    }
}
