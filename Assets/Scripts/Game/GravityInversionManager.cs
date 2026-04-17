using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GravityInversionManager : MonoBehaviour
{
    [Header("Referenzen")]
    public SteffScript steff;
    public GameObject indicatorPanel;
    public CanvasGroup indicatorCanvasGroup;
    public Image tintImage;
    public Image[] arrowImages;

    [Header("Timing")]
    public float firstEventMin  = 30f;
    public float firstEventMax  = 50f;
    public float cooldownMin    = 5f;
    public float cooldownMax    = 30f;
    public float warnDuration   = 3.0f;
    public float invertDuration = 7f;

    public static bool IsInverted { get; private set; }
    public static bool IsActive   { get; private set; }

    private float _timer;
    private bool  _eventRunning;
    private Coroutine _pulseCoroutine;
    private Coroutine _wipeCoroutine;
    private Vector3[] _basePositions;
    private float _originalGravityScale = 1f;
    private float _panelWidth;

    private static readonly Color ColInvert = new Color(0.95f, 0.38f, 0.08f, 0.28f);

    void Start()
    {
        if (RemoteConfigManager.Instance != null)
        {
            firstEventMin  = RemoteConfigManager.Instance.GravityFirstTriggerMin;
            firstEventMax  = RemoteConfigManager.Instance.GravityFirstTriggerMax;
            cooldownMin    = RemoteConfigManager.Instance.GravityCooldownMin;
            cooldownMax    = RemoteConfigManager.Instance.GravityCooldownMax;
            invertDuration = RemoteConfigManager.Instance.GravityInvertDuration;
        }

        _timer = Random.Range(firstEventMin, firstEventMax);
        if (steff != null) _originalGravityScale = steff.myRigitbody.gravityScale;
        if (indicatorPanel != null) indicatorPanel.SetActive(false);

        if (tintImage != null)
        {
            tintImage.type = Image.Type.Simple;
            // Rechts ankern — gleiche Wipe-Richtung wie DirectionFlipManager (von rechts rein)
            var rt = tintImage.rectTransform;
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot     = new Vector2(1f, 0.5f);
            rt.sizeDelta = Vector2.zero;
        }

        _basePositions = new Vector3[arrowImages != null ? arrowImages.Length : 0];
        for (int i = 0; i < _basePositions.Length; i++)
            if (arrowImages[i] != null)
                _basePositions[i] = arrowImages[i].transform.localPosition;
    }

    void Update()
    {
        if (_eventRunning && !steff.steffIsAlive) { StopAllCoroutines(); ResetState(); return; }
        if (_eventRunning || !steff.steffIsAlive || RankedManager.IsRanked) return;

        _timer -= Time.deltaTime;
        if (_timer > 0f) return;

        if (DirectionFlipManager.IsActive) { _timer = Random.Range(cooldownMin, cooldownMax); return; }
        StartCoroutine(TriggerInversion());
    }

    private IEnumerator TriggerInversion()
    {
        _eventRunning = true;
        IsActive      = true;

        indicatorPanel.SetActive(true);
        indicatorCanvasGroup.alpha = 1f;

        // 1 Frame warten damit Unity Layout berechnet
        yield return null;

        _panelWidth = indicatorPanel.GetComponent<RectTransform>().rect.width;
        var tintRT = tintImage.rectTransform;
        tintRT.sizeDelta        = new Vector2(_panelWidth, 0f);
        tintRT.anchoredPosition = new Vector2(_panelWidth, 0f); // rechts außerhalb
        tintImage.color         = ColInvert;

        // ↑-Arrows von unten + Tint-Wipe von oben gleichzeitig
        StartCoroutine(ShowArrows(-90f, new Vector2(0f, -220f)));
        _wipeCoroutine = StartCoroutine(WipeTint(0f, 1f, warnDuration));

        // ═══ INVERSION-MOMENT: Wipe-Linie erreicht Bildschirmmitte ═══
        yield return new WaitForSeconds(warnDuration * 0.5f);
        IsInverted = true;
        if (CameraShakeScript.Instance != null)
            CameraShakeScript.Instance.Shake(0.25f, 0.25f);

        yield return new WaitForSeconds(warnDuration * 0.5f);

        // ═══ AKTIV-PHASE ═══
        _pulseCoroutine = StartCoroutine(WavePulse());
        yield return new WaitForSeconds(invertDuration);

        // ═══ REVERT-VORWARNUNG: Arrows auf ↓, Wipe rückwärts von unten ═══
        StopPulse();
        for (int i = 0; i < arrowImages.Length; i++)
            if (arrowImages[i] != null)
                arrowImages[i].transform.localEulerAngles = new Vector3(0f, 0f, 90f);

        if (_wipeCoroutine != null) { StopCoroutine(_wipeCoroutine); _wipeCoroutine = null; }
        _wipeCoroutine = StartCoroutine(WipeTint(1f, 0f, warnDuration)); // Tint gleitet wieder heraus

        // ═══ REVERT-MOMENT: Wipe-Linie erreicht Bildschirmmitte ═══
        yield return new WaitForSeconds(warnDuration * 0.5f);
        IsInverted = false;
        if (CameraShakeScript.Instance != null)
            CameraShakeScript.Instance.Shake(0.18f, 0.18f);

        yield return new WaitForSeconds(warnDuration * 0.5f);

        // ═══ AUSBLENDEN ═══
        yield return StartCoroutine(HideArrows(new Vector2(0f, -220f)));
        tintRT.sizeDelta        = Vector2.zero;
        tintRT.anchoredPosition = Vector2.zero;
        indicatorPanel.SetActive(false);

        _timer = Random.Range(cooldownMin, cooldownMax);
        _eventRunning = false;
        IsActive      = false;
    }

    // ── Tint gleitet via anchoredPosition.x: f=0 versteckt rechts, f=1 voll sichtbar
    private IEnumerator WipeTint(float fromF, float toF, float dur)
    {
        float startX = _panelWidth * (1f - fromF);
        float endX   = _panelWidth * (1f - toF);
        var rt = tintImage.rectTransform;
        rt.anchoredPosition = new Vector2(startX, rt.anchoredPosition.y);

        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float x = Mathf.Lerp(startX, endX, t / dur);
            rt.anchoredPosition = new Vector2(x, rt.anchoredPosition.y);
            yield return null;
        }
        rt.anchoredPosition = new Vector2(endX, rt.anchoredPosition.y);
    }

    // ── Pfeile kaskaden rein ─────────────────────────────────────────────────
    private IEnumerator ShowArrows(float rotZ, Vector2 entryOffset)
    {
        for (int i = 0; i < arrowImages.Length; i++)
        {
            if (arrowImages[i] == null) continue;
            arrowImages[i].transform.localEulerAngles = new Vector3(0f, 0f, rotZ);
            SetAlpha(arrowImages[i], 0f);
            arrowImages[i].transform.localScale    = Vector3.zero;
            arrowImages[i].transform.localPosition = _basePositions[i] + (Vector3)entryOffset;
        }
        for (int i = 0; i < arrowImages.Length; i++)
            StartCoroutine(PunchIn(arrowImages[i], i * 0.11f, i));

        yield return new WaitForSeconds(arrowImages.Length * 0.11f + 0.42f);
    }

    private IEnumerator PunchIn(Image arrow, float delay, int idx)
    {
        if (arrow == null) yield break;
        yield return new WaitForSeconds(delay);

        Vector3 startPos = arrow.transform.localPosition;
        Vector3 endPos   = _basePositions[idx];
        float dur = 0.36f, t = 0f;

        while (t < dur)
        {
            t += Time.deltaTime;
            float p     = Mathf.Clamp01(t / dur);
            float scale = p < 0.65f ? Mathf.Lerp(0f, 1.3f, p / 0.65f)
                                    : Mathf.Lerp(1.3f, 1.0f, (p - 0.65f) / 0.35f);
            arrow.transform.localScale    = Vector3.one * scale;
            arrow.transform.localPosition = Vector3.Lerp(startPos, endPos, SmoothStep01(p));
            SetAlpha(arrow, Mathf.Clamp01(p * 3f));
            yield return null;
        }
        arrow.transform.localScale    = Vector3.one;
        arrow.transform.localPosition = endPos;
        SetAlpha(arrow, 1f);
    }

    // ── Pfeile kaskaden raus ─────────────────────────────────────────────────
    private IEnumerator HideArrows(Vector2 exitOffset)
    {
        StopPulse();
        for (int i = arrowImages.Length - 1; i >= 0; i--)
            StartCoroutine(SlideOut(arrowImages[i], (arrowImages.Length - 1 - i) * 0.08f, i, exitOffset));

        yield return new WaitForSeconds(arrowImages.Length * 0.08f + 0.28f);
    }

    private IEnumerator SlideOut(Image arrow, float delay, int idx, Vector2 exitOffset)
    {
        if (arrow == null) yield break;
        yield return new WaitForSeconds(delay);

        Vector3 startPos = _basePositions[idx];
        Vector3 endPos   = startPos + (Vector3)exitOffset;
        float dur = 0.22f, t = 0f;

        while (t < dur)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / dur);
            arrow.transform.localPosition = Vector3.Lerp(startPos, endPos, p * p);
            SetAlpha(arrow, Mathf.Lerp(1f, 0f, p));
            yield return null;
        }
        arrow.transform.localPosition = _basePositions[idx];
        arrow.transform.localScale    = Vector3.one;
        SetAlpha(arrow, 0f);
    }

    // ── Wellen-Puls ──────────────────────────────────────────────────────────
    private IEnumerator WavePulse()
    {
        float t = 0f;
        while (true)
        {
            t += Time.deltaTime;
            for (int i = 0; i < arrowImages.Length; i++)
            {
                if (arrowImages[i] == null) continue;
                float s = 1f + 0.09f * Mathf.Sin(t * 4.5f + i * 0.85f);
                arrowImages[i].transform.localScale = Vector3.one * s;
            }
            yield return null;
        }
    }

    private void StopPulse()
    {
        if (_pulseCoroutine != null) { StopCoroutine(_pulseCoroutine); _pulseCoroutine = null; }
        foreach (var img in arrowImages)
            if (img != null) img.transform.localScale = Vector3.one;
    }

    private void ResetState()
    {
        IsInverted    = false;
        IsActive      = false;
        _eventRunning = false;
        StopPulse();
        if (_wipeCoroutine != null) { StopCoroutine(_wipeCoroutine); _wipeCoroutine = null; }
        if (steff != null) steff.myRigitbody.gravityScale = _originalGravityScale;
        if (tintImage != null)
        {
            tintImage.rectTransform.sizeDelta        = Vector2.zero;
            tintImage.rectTransform.anchoredPosition = Vector2.zero;
        }
        for (int i = 0; i < arrowImages.Length; i++)
        {
            if (arrowImages[i] == null) continue;
            arrowImages[i].transform.localPosition = _basePositions[i];
            arrowImages[i].transform.localScale    = Vector3.one;
            SetAlpha(arrowImages[i], 0f);
        }
        if (indicatorPanel != null) { indicatorCanvasGroup.alpha = 1f; indicatorPanel.SetActive(false); }
        _timer = Random.Range(cooldownMin, cooldownMax);
    }

    private static void SetAlpha(Image img, float a)
    {
        var c = img.color; c.a = a; img.color = c;
    }

    private static float SmoothStep01(float t) => t * t * (3f - 2f * t);

    private void OnDestroy()
    {
        IsInverted = false;
        IsActive   = false;
    }
}
