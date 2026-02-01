using UnityEngine;

public class SpeedUp : MonoBehaviour
{
    public float speedBuff = 10f;
    public float buffDuration = 5f;
    public float hoverRadius = 1f;
    public float hoverSpeed = 2f;

    private Transform targetPlayer;
    private float timer;

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
        if (collision.CompareTag("Player"))
        {
            var playerController = collision.GetComponent<MovementController2D>();
            if (playerController != null)
            {
                // Attach to player and start hovering BEFORE calling ApplySpeedBuff
                targetPlayer = collision.transform;
                transform.SetParent(targetPlayer); // Make this object a child of the player
                timer = 0f;
                // Disable collider so it can't be picked up again
                var col = GetComponent<Collider2D>();
                if (col != null) col.enabled = false;

                // Now call ApplySpeedBuff — MovementController2D can find this child
                playerController.ApplySpeedBuff(speedBuff, buffDuration);
            }
        }
    }

    // Called by the player when the buff ends (optional, if you want to destroy the pickup after buff)
    public void EndBuff()
    {
        Destroy(gameObject);
    }
}
