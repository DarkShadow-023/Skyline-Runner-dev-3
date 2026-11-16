
using UnityEngine;
using TMPro;

public class RestartTextColour : MonoBehaviour
{
    public TMP_Text restartText;

    public Color[] colours =
    {
        Color.red,
        new Color(1f, 0.4f, 0f),
        Color.yellow,
        Color.white
    };

    public float speed = 2f;

    void Update()
    {
        float t = Mathf.PingPong(Time.time * speed, colours.Length - 1);

        int index = Mathf.FloorToInt(t);
        int nextIndex = Mathf.Min(index + 1, colours.Length - 1);

        restartText.color = Color.Lerp(
            colours[index],
            colours[nextIndex],
            t - index
        );
    }
}
