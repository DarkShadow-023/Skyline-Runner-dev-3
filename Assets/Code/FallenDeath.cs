
using UnityEngine;

public class KillZone : MonoBehaviour
{
    public Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        CharacterController player =
            other.GetComponent<CharacterController>();

        if (player == null)
            player = other.GetComponentInParent<CharacterController>();

        if (player == null)
            return;

        if (!player.CompareTag("Player"))
            return;

        player.enabled = false;

        player.transform.root.position = respawnPoint != null
            ? respawnPoint.position
            : new Vector3(0f, 5f, 0f);

        player.enabled = true;

        Debug.Log("Player respawned!");
    }
}
