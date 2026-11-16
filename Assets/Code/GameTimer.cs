
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GameTimer : MonoBehaviour
{
    public TMP_Text timerText;
    public RectTransform heart;

    public GameObject gameHUD;
    public GameObject deathHUD;

    public float startingTime = 90f;

    private float timeLeft;
    private bool hasStopped = false;

    void Start()
    {
        timeLeft = startingTime;

        deathHUD.SetActive(false);
        gameHUD.SetActive(true);
    }

    void Update()
    {
        if (hasStopped)
            return;

        timeLeft -= Time.deltaTime;
        timeLeft = Mathf.Max(0, timeLeft);

        int minutes = Mathf.FloorToInt(timeLeft / 60);
        int seconds = Mathf.FloorToInt(timeLeft % 60);

        timerText.text = string.Format(
            "{0:00}:{1:00}", minutes, seconds
        );

        // Heartbeat speeds up during the last 20 seconds
        float speed = 1.5f;

        if (timeLeft <= 20f)
        {
            float danger = 1f - (timeLeft / 20f);
            speed = Mathf.Lerp(1.5f, 3.5f, danger);
        }

        float pulse = 1f +
            Mathf.Sin(Time.time * speed * 6.28f) * 0.12f;

        heart.localScale = Vector3.one * pulse;

        // Timer colours
        if (timeLeft <= 15f)
            timerText.color = Color.red;
        else if (timeLeft <= 30f)
            timerText.color = new Color(1f, 0.6f, 0f);
        else
            timerText.color = Color.white;

        // Time runs out
        if (timeLeft <= 0f)
        {
            hasStopped = true;
            StartCoroutine(HeartExplosion());
        }
    }

    IEnumerator HeartExplosion()
    {
        timerText.text = "YOUR HEART HAS STOPPED";
        timerText.color = Color.red;

        Image heartImage = heart.GetComponent<Image>();
        Vector3 originalScale = heart.localScale;

        // Heart expands
        float elapsed = 0f;
        float duration = 0.25f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress = elapsed / duration;

            heart.localScale = Vector3.Lerp(
                originalScale,
                originalScale * 3f,
                progress
            );

            yield return null;
        }

        // Heart fades away
        if (heartImage != null)
        {
            Color originalColor = heartImage.color;
            elapsed = 0f;

            while (elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;

                Color faded = originalColor;
                faded.a = 1f - (elapsed / 0.2f);

                heartImage.color = faded;

                yield return null;
            }
        }

        // Wait 3 seconds
        yield return new WaitForSeconds(3f);

        // Hide normal HUD and show death screen
        gameHUD.SetActive(false);
        deathHUD.SetActive(true);
    }
}
