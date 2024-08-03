using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    public TextMeshProUGUI textMesh2;
    public TextMeshPro textMesh;
    public float floatSpeed = 0.7f;
    public float fadeDuration = 5f;
    private float startTime;

    private Color originalColor;

    void Start()
    {
        originalColor = textMesh.color;
        startTime = Time.time;
        Destroy(gameObject, fadeDuration * 2);
    }

    void Update()
    {
        transform.position += floatSpeed * Time.deltaTime * Vector3.up;

        float alpha = Mathf.Lerp(originalColor.a, 0, (Time.time - startTime) / fadeDuration);
        textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
    }

    public void SetText(string text)
    {
        textMesh.text = text;
    }

    public void Initialize(string text)
    {
        textMesh.text = text;
    }
    public void Initialize(string text, Color color, float floatSpeed, float fadeDuration)
    {
        Initialize(text);
        textMesh.color = color;
        this.floatSpeed = floatSpeed;
        this.fadeDuration = fadeDuration;
        originalColor = color;
    }
}