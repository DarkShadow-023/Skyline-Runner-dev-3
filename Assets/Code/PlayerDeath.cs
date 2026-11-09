
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PlayerDeath
{
    private static bool isDead = false;

    public static void Die(GameObject deathScreen)
    {
        if (isDead) return;

        isDead = true;

        deathScreen.SetActive(true);

        // Show and unlock mouse
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Freeze game
        Time.timeScale = 0f;
    }

    public static void Restart()
    {
        Time.timeScale = 1f;
        isDead = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}
