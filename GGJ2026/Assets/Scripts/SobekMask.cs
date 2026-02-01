using System.Collections;
using UnityEngine;

public class SobekMask : MonoBehaviour
{
    [Tooltip("If true, this mask will instantly destroy breakable objects it touches.")]
    public bool destroysBlocksOnContact = true;

    [Tooltip("How long the mask buff lasts once picked up.")]
    public float buffDuration = 5f;
    [Tooltip("Radius of hover around the player.")]
    public float hoverRadius = 1f;
    [Tooltip("Speed of hover motion.")]
    public float hoverSpeed = 2f;

    // Hover / pickup state
    private Transform targetPlayer;
    private float timer;
    private bool pickedUp;
    private bool isActive;

    // Update is called once per frame
    void Update()
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

    private void HandleCollisionWithCollider(Collider2D collider)
    {
        if (collider == null) return;

        // If not yet picked up: allow pickup by the first player that touches it
        if (!pickedUp && collider.CompareTag("Player"))
        {
            var playerController = collider.GetComponent<MovementController2D>();
            if (playerController != null)
            {
                pickedUp = true;
                // Attach to player and start hovering
                targetPlayer = collider.transform;
                transform.SetParent(targetPlayer);
                timer = 0f;

                isActive = true;
                StartCoroutine(BuffLifetime());

                // Keep collider enabled so mask can destroy blocks while hovering
                return;
            }
        }

        // If configured, instantly destroy breakable objects on contact
        if (destroysBlocksOnContact)
        {
            var breakable = collider.GetComponentInParent<BreakableObject>();
            if (breakable != null)
            {
                breakable.ForceBreak();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollisionWithCollider(collision);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollisionWithCollider(collision.collider);
    }

    private IEnumerator BuffLifetime()
    {
        yield return new WaitForSeconds(buffDuration);
        isActive = false;
        EndBuff();
    }

    public void EndBuff()
    {
        // Optionally detach or play VFX, then destroy
        Destroy(gameObject);
    }
}
