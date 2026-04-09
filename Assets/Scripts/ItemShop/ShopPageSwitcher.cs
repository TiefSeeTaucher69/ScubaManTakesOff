using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShopPageSwitcher : MonoBehaviour
{
    [System.Serializable]
    public class ShopPage
    {
        public GameObject panel;
        public Image      indicator; // Img_Indicator_* – nur für Breite des Sliders genutzt
    }

    public ShopPage[] pages;

    public Color inactiveColor = new Color(0.33f, 0.33f, 0.33f, 1f);

    [Header("Slider")]
    public RectTransform tabSlider; // grünes Slider-Image als Kind von ShopBar

    private int currentPage = 0;
    private Coroutine _slideCoroutine;
    private Coroutine _fadeCoroutine;

    void Start()
    {
        // Alle Indikatoren auf inaktive Farbe setzen (Slider übernimmt die Anzeige)
        foreach (var page in pages)
            if (page.indicator != null)
                page.indicator.color = inactiveColor;

        Canvas.ForceUpdateCanvases();
        SnapSlider();
        SwitchToPage(0);
    }

    public void SwitchToPage(int index)
    {
        for (int i = 0; i < pages.Length; i++)
            pages[i].panel.SetActive(false);

        currentPage = index;
        pages[currentPage].panel.SetActive(true);

        // Slider animieren
        if (tabSlider != null && pages[index].indicator != null)
        {
            var btn = pages[index].indicator.GetComponentInParent<Button>();
            if (btn != null)
            {
                float targetX = btn.GetComponent<RectTransform>().localPosition.x;
                float targetW = pages[index].indicator.rectTransform.rect.width;
                if (_slideCoroutine != null) StopCoroutine(_slideCoroutine);
                _slideCoroutine = StartCoroutine(AnimateSlider(tabSlider, targetX, targetW));
            }
        }

        // Panel einblenden
        var cg = pages[currentPage].panel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f;
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeIn(cg));
        }
    }

    // Für die < > Pfeil-Buttons
    public void NextPage() => SwitchToPage((currentPage + 1) % pages.Length);
    public void PrevPage() => SwitchToPage((currentPage - 1 + pages.Length) % pages.Length);

    private void SnapSlider()
    {
        if (tabSlider == null || pages.Length == 0 || pages[0].indicator == null) return;
        var btn = pages[0].indicator.GetComponentInParent<Button>();
        if (btn == null) return;
        RectTransform btnRT = btn.GetComponent<RectTransform>();
        RectTransform indRT = pages[0].indicator.rectTransform;
        Vector3 pos = tabSlider.localPosition;
        pos.x = btnRT.localPosition.x;
        pos.y = btnRT.localPosition.y + indRT.localPosition.y;
        tabSlider.localPosition = pos;
        tabSlider.sizeDelta = new Vector2(indRT.rect.width, tabSlider.sizeDelta.y);
    }

    private IEnumerator AnimateSlider(RectTransform slider, float targetX, float targetWidth, float duration = 0.2f)
    {
        float startX = slider.localPosition.x;
        float startWidth = slider.sizeDelta.x;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            Vector3 pos = slider.localPosition;
            pos.x = Mathf.Lerp(startX, targetX, t);
            slider.localPosition = pos;
            slider.sizeDelta = new Vector2(Mathf.Lerp(startWidth, targetWidth, t), slider.sizeDelta.y);
            yield return null;
        }
        Vector3 finalPos = slider.localPosition;
        finalPos.x = targetX;
        slider.localPosition = finalPos;
        slider.sizeDelta = new Vector2(targetWidth, slider.sizeDelta.y);
    }

    private IEnumerator FadeIn(CanvasGroup cg, float duration = 0.15f)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }
        cg.alpha = 1f;
    }
}
