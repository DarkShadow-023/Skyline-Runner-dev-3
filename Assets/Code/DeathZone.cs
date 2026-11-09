
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    public GameObject deathScreen;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerDeath.Die(deathScreen);
        }
    }
}
