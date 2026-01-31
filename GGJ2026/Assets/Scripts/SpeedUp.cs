using UnityEngine;

public class SpeedUp : MonoBehaviour
{
    // Called when another collider enters the trigger collider attached to this object
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the collider belongs to the player
        if (collision.CompareTag("Player"))
        {
            // Try to get a component on the player that handles speed (replace 'PlayerController' with your actual script)
            var playerController = collision.GetComponent<MovementController2D>();
            if (playerController != null)
            {
                playerController.moveSpeed = 10f;
                Debug.Log("Player speed increased to 10f");
            }

            // Destroy the pickup object after collection
            Destroy(gameObject);
        }
    }
}
