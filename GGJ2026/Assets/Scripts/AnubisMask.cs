using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnubisMask : MonoBehaviour
{
    public float buffDuration = 5f;
    public float hoverRadius = 1f;
    public float hoverSpeed = 2f;
    public float immobilizeDuration = 2f;
    // optional per-target cooldown so a single target isn't spammed every frame
    public float hitCooldown = 1f;

    private Transform targetPlayer;
    private float timer;
    private bool pickedUp;
    private bool isActive;
    private readonly HashSet<MovementController2D> recentlyImmobilized = new HashSet<MovementController2D>();
    private AudioSource pickupSound;
    private AudioSource mainLoop;

    private void Awake()
    {
        mainLoop = GameObject.FindGameObjectWithTag("Musica")?.GetComponent<AudioSource>();
        pickupSound = GameObject.FindGameObjectWithTag("Anubis")?.GetComponent<AudioSource>();

        if (mainLoop != null)
        {
            mainLoop.volume = 1f;
            if (!mainLoop.isPlaying)
                mainLoop.Play();
        }
        if (pickupSound != null)
        {
            pickupSound.volume = 0f;
            if (!pickupSound.isPlaying)
                pickupSound.Play();
        }
    }

    private void Update()
    {
        // If attached to a player, hover around them
        if (targetPlayer != null)
        {
            timer += Time.deltaTime;
            float angle = timer * hoverSpeed * Mathf.PI * 2f;
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * hoverRadius;
            transform.position = targetPlayer.position + offset;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If not yet picked up: allow pickup by the first player that touches it
        if (!pickedUp && collision.CompareTag("Player"))
        {
            var playerController = collision.GetComponent<MovementController2D>();
            if (playerController != null)
            {
                pickedUp = true;
                // Attach to player and start hovering
                targetPlayer = collision.transform;
                transform.SetParent(targetPlayer); // Make this object a child of the player
                timer = 0f;

                // mark active and start lifetime coroutine
                isActive = true;
                if (pickupSound != null)
                {
                    pickupSound.volume = 1f;
                }
                if (mainLoop != null)
                {
                    mainLoop.volume = 0f;
                }
                StartCoroutine(BuffLifetime());

                // NOTE: keep collider enabled so mask can hit other players while hovering
                return;
            }
        }

        // If already picked up and active, attack other players the mask collides with
        if (isActive && targetPlayer != null && collision.CompareTag("Player"))
        {
            // ignore collisions with the owner
            if (collision.transform == targetPlayer) return;

            var otherController = collision.GetComponent<MovementController2D>();
            if (otherController != null && !recentlyImmobilized.Contains(otherController))
            {
                otherController.ApplyImmobilize(immobilizeDuration);
                // add to cooldown set to avoid re-triggering immediately
                recentlyImmobilized.Add(otherController);
                StartCoroutine(ClearHitCooldown(otherController, hitCooldown));
            }
        }
    }

    private IEnumerator BuffLifetime()
    {
        yield return new WaitForSeconds(buffDuration);
        isActive = false;
        EndBuff();
    }

    private IEnumerator ClearHitCooldown(MovementController2D target, float delay)
    {
        yield return new WaitForSeconds(delay);
        recentlyImmobilized.Remove(target);
    }

    public void EndBuff()
    {
        if (pickupSound != null)
        {
            pickupSound.volume = 0f;
        }
        if (mainLoop != null)
        {
            mainLoop.volume = 1f;
        }
        // optionally play VFX or detach before destroy
        Destroy(gameObject);
    }
}
