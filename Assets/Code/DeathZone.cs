
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    public GameObject gameHUD;
    public GameObject deathHUD;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
            return;

        CharacterController controller =
            other.GetComponentInParent<CharacterController>();

        if (controller == null)
            return;

        hasTriggered = true;

        Debug.Log("PLAYER DIED!");

        // Stop the player
        controller.enabled = false;

        // Disable player movement scripts
        MonoBehaviour[] scripts =
            controller.GetComponentsInParent<MonoBehaviour>();

        foreach (MonoBehaviour script in scripts)
        {
            if (script != this &&
                script.GetType().Name.Contains("Movement"))
            {
                script.enabled = false;
            }
        }

        // Hide gameplay Canvas
        if (gameHUD != null)
            gameHUD.SetActive(false);

        // Show death Canvas
        if (deathHUD != null)
        {
            deathHUD.SetActive(true);
            Debug.Log("Death Canvas activated!");
        }

        // Unlock and show mouse
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
