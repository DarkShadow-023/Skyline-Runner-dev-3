
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FireTrap : MonoBehaviour
{
    public GameObject deathScreen;

    private bool isDead = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isDead)
        {
            isDead = true;
            deathScreen.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
