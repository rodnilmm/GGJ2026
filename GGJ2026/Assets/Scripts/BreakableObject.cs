using UnityEngine;

public class BreakableObject : MonoBehaviour
{

    [Tooltip("The prefab to instantiate when this object breaks.")]
    public GameObject brokenPrefab;
    [Tooltip("Sound to play when the object breaks.")]
    public AudioClip breakSound;
    [Tooltip("Volume of the break sound.")]
    [Range(0f, 1f)]
    public float breakSoundVolume = 1f;

    [Tooltip("Number of hits required to break this object.")]
    public int hitsToBreak = 3;

    // Current remaining hits before breaking
    private int remainingHits;

    private void Awake()
    {
        // Ensure at least one hit is required
        remainingHits = Mathf.Max(1, hitsToBreak);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Example condition: break on collision with an object tagged "Player"
        if (collision.gameObject.CompareTag("Player"))
        {
            TakeHit();
        }
    }

    // Public method so other scripts (projectiles, traps) can also call it
    public void TakeHit()
    {
        remainingHits--;
        if (remainingHits <= 0)
        {
            Break();
        }
        else
        {
            // Optional: add hit feedback (flash, small sound, particle) here
        }
    }

    // Forcefully break the object immediately (ignores remainingHits)
    public void ForceBreak()
    {
        Break();
    }

    private void Break()
    {
        // Play break sound
        if (breakSound != null)
        {
            AudioSource.PlayClipAtPoint(breakSound, transform.position, breakSoundVolume);
        }
        // Instantiate broken prefab
        if (brokenPrefab != null)
        {
            Instantiate(brokenPrefab, transform.position, transform.rotation);
        }
        // Destroy this object
        Destroy(gameObject);
    }
}
