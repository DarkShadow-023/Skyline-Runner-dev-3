
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

    private Image heartImage;
    private Color originalHeartColor;

    void Start()
    {
        // Check Inspector references
        if (timerText == null || heart == null ||
            gameHUD == null || deathHUD == null)
        {
            Debug.LogError("GameTimer: Missing Inspector references!");
            enabled = false;
            return;
        }

        // Get heart image
        heartImage = heart.GetComponent<Image>();

        if (heartImage == null)
        {
            Debug.LogError("GameTimer: Heart needs an Image component!");
            enabled = false;
            return;
        }

        // Store original heart appearance
        originalHeartColor = heartImage.color;

        heart.localScale = Vector3.one;
        heartImage.color = originalHeartColor;

        // Start timer
        timeLeft = startingTime;

        deathHUD.SetActive(false);
        gameHUD.SetActive(true);

        UpdateTimerDisplay();
    }

    void Update()
    {
        if (hasStopped)
            return;

        // Countdown
        timeLeft -= Time.deltaTime;
        timeLeft = Mathf.Max(0f, timeLeft);

        UpdateTimerDisplay();

        // Heartbeat speed
        float speed = 1.5f;

        if (timeLeft <= 20f)
        {
            float danger = 1f - (timeLeft / 20f);
            speed = Mathf.Lerp(1.5f, 3.5f, danger);
        }

        // Heart pulse
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

    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(timeLeft / 60);
        int seconds = Mathf.FloorToInt(timeLeft % 60);

        timerText.text = string.Format(
            "{0:00}:{1:00}", minutes, seconds
        );
    }

    IEnumerator HeartExplosion()
    {
        timerText.text = "YOUR HEART HAS STOPPED";
        timerText.color = Color.red;

        Vector3 originalScale = heart.localScale;

        // Heart expands
        float elapsed = 0f;
        float duration = 0.25f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);

            heart.localScale = Vector3.Lerp(
                originalScale,
                originalScale * 3f,
                progress
            );

            yield return null;
        }

        // Heart fades away
        Color startColor = heartImage.color;
        elapsed = 0f;
        float fadeDuration = 0.2f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);

            Color faded = startColor;
            faded.a = alpha;

            heartImage.color = faded;

            yield return null;
        }

        heartImage.color = new Color(
            originalHeartColor.r,
            originalHeartColor.g,
            originalHeartColor.b,
            0f
        );

        // Wait 3 seconds
        yield return new WaitForSeconds(3f);

        // Show death screen
        gameHUD.SetActive(false);
        deathHUD.SetActive(true);
    }
}
