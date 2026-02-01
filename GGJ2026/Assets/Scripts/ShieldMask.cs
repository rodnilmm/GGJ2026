using System.Collections;
using UnityEngine;

public class ShieldMask : MonoBehaviour
{
    [Tooltip("Number of Anubis hits the shield can absorb before breaking.")]
    public int durability = 1;

    [Tooltip("Optional: sound to play when the shield blocks an Anubis hit.")]
    public AudioClip blockSound;
    [Tooltip("Volume of the block sound.")]
    [Range(0f, 1f)]
    public float blockSoundVolume = 1f;

    [Tooltip("How long the shield buff lasts once picked up.")]
    public float buffDuration = 5f;
    [Tooltip("Radius of hover around the player.")]
    public float hoverRadius = 1f;
    [Tooltip("Speed of hover motion.")]
    public float hoverSpeed = 2f;

    // Remaining durability
    private int remainingDurability;
    private string childObjectName = "Bubble";
    private GameObject buffChildObject;
    // Hover / pickup state
    private Transform targetPlayer;
    private float timer;
    private bool pickedUp;
    private bool isActive;

    private void Awake()
    {
        remainingDurability = Mathf.Max(1, durability);
    }

    // Exposed flag used by MovementController2D to decide whether to ignore immobilize
    public bool IsActive => isActive && enabled && gameObject.activeInHierarchy;

    // Called by MovementController2D when an immobilize attempt was blocked
    public void OnBlockedMaskHit()
    {
        if (blockSound != null)
        {
            AudioSource.PlayClipAtPoint(blockSound, transform.position, blockSoundVolume);
        }

        remainingDurability--;
        if (remainingDurability <= 0)
        {
            DestroyShield();
        }
        else
        {
            // Optional: play hit feedback (flash, particle) here
        }
    }

    private void DestroyShield()
    {
        // Optionally play VFX here
        Destroy(gameObject);
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

                if (!string.IsNullOrEmpty(childObjectName))
                {
                    buffChildObject = targetPlayer.Find(childObjectName)?.gameObject;
                    if (buffChildObject != null)
                        buffChildObject.SetActive(true);
                }

                // mark active and start lifetime coroutine
                isActive = true;
                StartCoroutine(BuffLifetime());

                // NOTE: keep collider enabled so shield can neutralize masks while hovering
                return;
            }
        }

        // If the thing that collided is an AnubisMask (or a child of it), neutralize that mask
        var anubis = collision.GetComponentInParent<AnubisMask>();
        // If the thing that collided is a SobekMask (or a child of it), neutralize that mask too
        var sobek = collision.GetComponentInParent<SobekMask>();

        var speed = collision.GetComponentInParent<SpeedUp>();

        if (anubis != null || sobek != null || speed!= null)
        {
            if (anubis != null)
            {
                anubis.EndBuff();
            }
            if (sobek != null)
            {
                sobek.EndBuff();
            }
            if (speed != null)
            {
                speed.EndBuff();
            }

            // Provide feedback and reduce durability (shared behavior)
            OnBlockedMaskHit();
        }
    }

    private IEnumerator BuffLifetime()
    {
        yield return new WaitForSeconds(buffDuration);
        isActive = false;
        // Optionally detach before destroy
        DestroyShield();
    }
}
