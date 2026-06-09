using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Color baseColor;
    public Vector2 baseSize;
    public Image image;

    [Header("Tweak these to taste")]
    public float hoverBrightness = 0.15f;
    public float hoverScale = 1.06f;
    public float duration = 0.12f;

    private RectTransform rt;
    private Coroutine current;

    private void Awake() => rt = GetComponent<RectTransform>();

    public void OnPointerEnter(PointerEventData _) => Animate(
        Color.Lerp(baseColor, Color.white, hoverBrightness),
        baseSize * hoverScale);

    public void OnPointerExit(PointerEventData _) => Animate(baseColor, baseSize);

    private void Animate(Color targetColor, Vector2 targetSize)
    {
        if (current != null) StopCoroutine(current);
        current = StartCoroutine(DoAnimate(targetColor, targetSize));
    }

    private IEnumerator DoAnimate(Color targetColor, Vector2 targetSize)
    {
        Color startColor = image.color;
        Vector2 startSize = rt.sizeDelta;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float smooth = t * t * (3f - 2f * t);          // smoothstep

            image.color = Color.Lerp(startColor, targetColor, smooth);
            rt.sizeDelta = Vector2.Lerp(startSize, targetSize, smooth);

            elapsed += Time.deltaTime;
            yield return null;
        }

        image.color = targetColor;
        rt.sizeDelta = targetSize;
    }
}
