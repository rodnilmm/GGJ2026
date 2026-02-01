using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// When picked up, this mask slows other players for a duration.
/// </summary>
public class ThotMask : MonoBehaviour
{
    [Tooltip("How long the slow effect lasts once picked up.")]
    public float buffDuration = 5f;

    [Tooltip("Multiplier applied to other players' moveSpeed (0.5 = half speed).")]
    [Range(0f, 1f)]
    public float slowMultiplier = 0.5f;

    [Tooltip("Radius of hover around the player.")]
    public float hoverRadius = 1f;
    [Tooltip("Speed of hover motion.")]
    public float hoverSpeed = 2f;

    private Transform targetPlayer;
    private float timer;
    private bool pickedUp;

    private void Update()
    {
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
        if (pickedUp) return;
        if (!collision.CompareTag("Player")) return;

        var playerController = collision.GetComponent<MovementController2D>();
        if (playerController == null) return;

        // Mark as picked up and attach to the picker
        pickedUp = true;
        targetPlayer = collision.transform;
        transform.SetParent(targetPlayer);
        timer = 0f;

        // Prevent further pickups
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Apply slow to all other players
        ApplySlowToOtherPlayers(collision.gameObject);

        // Optionally destroy after buffDuration (or keep hovering while active)
        StartCoroutine(BuffLifetime());
    }

    private void ApplySlowToOtherPlayers(GameObject picker)
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var go in players)
        {
            if (go == null || go == picker) continue;
            var mc = go.GetComponent<MovementController2D>();
            if (mc == null) continue;

            // Use current moveSpeed so slow is relative to that player's current speed
            float newSpeed = mc.moveSpeed * slowMultiplier;
            mc.ApplySpeedBuff(newSpeed, buffDuration);
        }
    }

    private IEnumerator BuffLifetime()
    {
        yield return new WaitForSeconds(buffDuration);
        EndBuff();
    }

    public void EndBuff()
    {
        Destroy(gameObject);
    }
}
