using System.Collections;
using UnityEngine;

public class HoverDropdown : MonoBehaviour
{
    [SerializeField] private CanvasGroup dropdownGroup;
    [SerializeField] private float animDuration = 0.18f;
    [SerializeField] private float hideDelay    = 0.08f;
    [SerializeField] private RectTransform[] triggerZones; // Btn_PLAY + PlayBar (für OnEnable-Check)

    private Coroutine _fadeRoutine;
    private Coroutine _hideRoutine;

    void Awake()
    {
        if (dropdownGroup != null)
        {
            dropdownGroup.alpha          = 0f;
            dropdownGroup.interactable   = false;
            dropdownGroup.blocksRaycasts = false;
        }
    }

    void OnEnable()
    {
        // Wenn Panel aktiviert wird und Maus bereits drauf ist → sofort zeigen
        StartCoroutine(CheckMouseOnEnable());
    }

    private IEnumerator CheckMouseOnEnable()
    {
        yield return null; // ein Frame warten bis Layout fertig ist
        Vector2 mousePos = Input.mousePosition;
        foreach (var zone in triggerZones)
        {
            if (zone != null && RectTransformUtility.RectangleContainsScreenPoint(zone, mousePos, null))
            {
                OnEnterTrigger();
                yield break;
            }
        }
    }

    public void OnEnterTrigger()
    {
        if (!gameObject.activeInHierarchy) return;
        if (_hideRoutine != null) { StopCoroutine(_hideRoutine); _hideRoutine = null; }
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(Fade(1f));
    }

    public void OnExitTrigger()
    {
        if (!gameObject.activeInHierarchy) return;
        if (_hideRoutine != null) StopCoroutine(_hideRoutine);
        _hideRoutine = StartCoroutine(DelayedHide());
    }

    private IEnumerator DelayedHide()
    {
        yield return new WaitForSecondsRealtime(hideDelay);
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(Fade(0f));
    }

    private IEnumerator Fade(float target)
    {
        dropdownGroup.interactable   = target > 0f;
        dropdownGroup.blocksRaycasts = target > 0f;

        float start = dropdownGroup.alpha;
        float elapsed = 0f;
        while (elapsed < animDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            dropdownGroup.alpha = Mathf.Lerp(start, target, elapsed / animDuration);
            yield return null;
        }
        dropdownGroup.alpha = target;
    }
}
