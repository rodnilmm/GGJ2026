using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController2D : MonoBehaviour
{
    [Header("Movement Settings")]
    public bool isMummy = false;
    public bool Mining = false;
    public float moveSpeed = 5f;
    public float faceX;
    public float faceY;
    public Animator[] anim;
    private Vector2 moveInput;
    private Rigidbody2D rb;
    public AnimationClip picoAnim;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioSource picoSound;

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
            for (int i = 0; i < anim.Length; i++)
            {
                anim[i].SetBool("Walking", true);
            }
            faceX = input.x;
            faceY = input.y;
        }
        else
        {
            for (int i = 0; i < anim.Length; i++)
            {
                anim[i].SetBool("Walking", false);
            }
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (Mining) return;
        Mining = true;
        Debug.Log($"{gameObject.name} interacted!");
        isMummy = true;
        StartCoroutine(InteractAnim());
    }

    IEnumerator InteractAnim()
    {
        for (int i = 0; i < anim.Length; i++)
        {
            anim[i].SetBool("Pico", true);
            anim[i].SetBool("Walking", false);
        }
        yield return new WaitForSeconds(picoAnim.length);
        picoSound.Play();
        for (int i = 0; i < anim.Length; i++)
        {
            anim[i].SetBool("Pico", false);
        }
        isMummy = false;
        Mining = false;
    }

    private void FixedUpdate()
    {
        if (isMummy || Mining) return;
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

    // Public API used by pickups to set a temporary moveSpeed
    public void ApplySpeedBuff(float newSpeed, float duration)
    {
        StartCoroutine(SpeedBuffCoroutine(newSpeed, duration));
    }

    private IEnumerator SpeedBuffCoroutine(float newSpeed, float duration)
    {
        // Always apply the temporary speed change regardless of pickup child presence.
        float originalSpeed = moveSpeed;
        moveSpeed = newSpeed;

        // Ensure animator state reflects the slowed movement (optional)
        if (moveInput.sqrMagnitude <= 0.01f)
        {
            for (int i = 0; i < anim.Length; i++)
            {
                anim[i].SetBool("Walking", false);
            }
        }

        yield return new WaitForSeconds(duration);

        // restore previous speed
        moveSpeed = originalSpeed;
        Debug.Log($"Player speed reverted to {originalSpeed}");

        // If the buff was created by a pickup existing as a child, let that pickup end itself.
        var speedUp = GetComponentInChildren<SpeedUp>();
        if (speedUp != null)
        {
            speedUp.EndBuff();
        }
    }

    // Immobilize API: external scripts call this to stop the player for a duration
    public void ApplyImmobilize(float duration)
    {
        // If the player currently has an active shield, ignore immobilize attempts.
        var shield = GetComponentInChildren<ShieldMask>();
        if (shield != null && shield.IsActive)
        {
            // Let the shield consume one durability and provide feedback
            shield.OnBlockedMaskHit();
            return;
        }

        StartCoroutine(ImmobilizeCoroutine(duration));
    }

    private IEnumerator ImmobilizeCoroutine(float duration)
    {
        float originalSpeed = moveSpeed;
        // stop movement
        moveSpeed = 0f;

        // update animators to not walking
        for (int i = 0; i < anim.Length; i++)
        {
            anim[i].SetBool("Walking", false);
        }
        for (int i = 0; i < anim.Length; i++)
        {
            anim[i].SetBool("Mummy", true);
        }

        yield return new WaitForSeconds(duration);

        // restore previous speed
        moveSpeed = originalSpeed;
        for (int i = 0; i < anim.Length; i++)
        {
            anim[i].SetBool("Mummy", false);
        }
    }
}
