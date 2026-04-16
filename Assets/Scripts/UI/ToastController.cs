using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToastController : MonoBehaviour
{
    [SerializeField] Image    _background;
    [SerializeField] TMP_Text _message;
    [SerializeField] Image    _timerFill;

    Action _onComplete;
    RectTransform _rect;

    void Awake()
    {
        _rect = GetComponent<RectTransform>();
    }

    public void Init(string text, Sprite banner, Color timerColor, float duration,
                     float slotY, float leftPadding, Action onComplete)
    {
        _background.sprite = banner;
        _message.text      = text;
        _timerFill.color   = timerColor;
        _onComplete        = onComplete;

        // Anchors sind bereits im Prefab auf oben-links gesetzt
        // Startposition: off-screen links, korrekte Y-Slot-Position
        float width = _rect.sizeDelta.x;
        if (width <= 0f) width = 420f;
        _rect.anchoredPosition = new Vector2(-width, slotY);

        StartCoroutine(ShowToast(duration, slotY, leftPadding));
    }

    IEnumerator ShowToast(float duration, float slotY, float leftPadding)
    {
        yield return null; // ein Frame warten (Layout)

        float width = _rect.sizeDelta.x;
        if (width <= 0f) width = 420f;

        // Slide-in von links
        Vector2 startPos = new Vector2(-width, slotY);
        Vector2 endPos   = new Vector2(leftPadding, slotY);
        float slideTime  = 0.3f;
        float t = 0f;

        while (t < slideTime)
        {
            t += Time.unscaledDeltaTime;
            _rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t / slideTime);
            yield return null;
        }
        _rect.anchoredPosition = endPos;

        // Timer-Countdown: localScale.x 1 → 0 (Pivot links → schrumpft von rechts)
        float elapsed = 0f;
        _timerFill.rectTransform.localScale = new Vector3(1f, 1f, 1f);
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float remaining = 1f - Mathf.Clamp01(elapsed / duration);
            _timerFill.rectTransform.localScale = new Vector3(remaining, 1f, 1f);
            yield return null;
        }
        _timerFill.rectTransform.localScale = Vector3.zero;

        // Slide-out nach links
        t = 0f;
        startPos = _rect.anchoredPosition;
        endPos   = new Vector2(-width, slotY);

        while (t < slideTime)
        {
            t += Time.unscaledDeltaTime;
            _rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t / slideTime);
            yield return null;
        }

        _onComplete?.Invoke();
        Destroy(gameObject);
    }
}
