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

    Action<ToastController> _onComplete;
    RectTransform _rect;
    float _currentSlotY;
    float _leftPadding;

    void Awake()
    {
        _rect = GetComponent<RectTransform>();
    }

    public void Init(string text, Sprite banner, Color timerColor, float duration,
                     float slotY, float leftPadding, Action<ToastController> onComplete)
    {
        _background.sprite = banner;
        _message.text      = text;
        _timerFill.color   = timerColor;
        _onComplete        = onComplete;
        _currentSlotY      = slotY;
        _leftPadding       = leftPadding;

        float width = _rect.sizeDelta.x;
        if (width <= 0f) width = 420f;
        _rect.anchoredPosition = new Vector2(-width, slotY);

        StartCoroutine(ShowToast(duration));
    }

    public void MoveToSlot(float targetY)
    {
        StartCoroutine(AnimateToSlot(targetY));
    }

    IEnumerator AnimateToSlot(float targetY)
    {
        float startY  = _rect.anchoredPosition.y;
        float startX  = _rect.anchoredPosition.x;
        float t       = 0f;
        float moveTime = 0.2f;

        while (t < moveTime)
        {
            t += Time.unscaledDeltaTime;
            _rect.anchoredPosition = new Vector2(startX, Mathf.Lerp(startY, targetY, t / moveTime));
            yield return null;
        }
        _rect.anchoredPosition = new Vector2(startX, targetY);
        _currentSlotY = targetY;
    }

    IEnumerator ShowToast(float duration)
    {
        yield return null; // ein Frame warten (Layout)

        float width = _rect.sizeDelta.x;
        if (width <= 0f) width = 420f;

        // Slide-in von links
        Vector2 startPos = new Vector2(-width, _currentSlotY);
        Vector2 endPos   = new Vector2(_leftPadding, _currentSlotY);
        float slideTime  = 0.3f;
        float t = 0f;

        while (t < slideTime)
        {
            t += Time.unscaledDeltaTime;
            _rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t / slideTime);
            yield return null;
        }
        _rect.anchoredPosition = endPos;

        // Timer-Countdown: localScale.x 1 → 0
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

        // Slide-out nach links (nutzt _currentSlotY falls MoveToSlot aufgerufen wurde)
        t = 0f;
        startPos = _rect.anchoredPosition;
        endPos   = new Vector2(-width, _currentSlotY);

        while (t < slideTime)
        {
            t += Time.unscaledDeltaTime;
            _rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t / slideTime);
            yield return null;
        }

        _onComplete?.Invoke(this);
        Destroy(gameObject);
    }
}
