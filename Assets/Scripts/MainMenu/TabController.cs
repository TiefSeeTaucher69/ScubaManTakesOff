using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TabController : MonoBehaviour
{
    [System.Serializable]
    public class Tab
    {
        public Button button;
        public GameObject panel;
        public Image indicator; // schmaler Unterstrich unter dem Tab-Button (bleibt grau)
    }

    public Tab[] tabs;

    [Header("Navigations-Buttons (optional)")]
    public Button prevButton; // Btn_LEFT
    public Button nextButton; // Btn_RIGHT

    [Header("Slider")]
    public RectTransform tabSlider; // grünes Slider-Image als Kind von TabBar

    private int currentTab = 0;
    private Coroutine _slideCoroutine;
    private Coroutine _fadeCoroutine;

    void Start()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            int index = i;
            tabs[i].button.onClick.AddListener(() => SwitchTab(index));
        }

        if (prevButton != null)
            prevButton.onClick.AddListener(VorigerTab);

        if (nextButton != null)
            nextButton.onClick.AddListener(NaechsterTab);

        Canvas.ForceUpdateCanvases();

        // Initial snap vor SwitchTab damit Coroutine von korrekter Position startet
        if (tabSlider != null && tabs.Length > 0 && tabs[0].indicator != null)
        {
            RectTransform btnRT = tabs[0].button.GetComponent<RectTransform>();
            RectTransform indRT = tabs[0].indicator.rectTransform;
            SnapSlider(tabSlider, btnRT, indRT);
        }
        SwitchTab(0);
    }

    void Update()
    {
        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            if (gamepad.leftShoulder.wasPressedThisFrame)  VorigerTab();
            if (gamepad.rightShoulder.wasPressedThisFrame) NaechsterTab();
        }
    }

    void VorigerTab()   => SwitchTab((currentTab - 1 + tabs.Length) % tabs.Length);
    void NaechsterTab() => SwitchTab((currentTab + 1) % tabs.Length);

    public void SwitchTab(int index)
    {
        for (int i = 0; i < tabs.Length; i++)
            tabs[i].panel.SetActive(false);

        currentTab = index;
        tabs[currentTab].panel.SetActive(true);

        // Panel einblenden
        var cg = tabs[currentTab].panel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f;
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeIn(cg));
        }

        if (tabSlider == null || tabs[index].indicator == null) return;

        RectTransform btnRT = tabs[index].button.GetComponent<RectTransform>();
        float targetX = btnRT.localPosition.x;
        float targetWidth = tabs[index].indicator.rectTransform.rect.width;

        if (_slideCoroutine != null) StopCoroutine(_slideCoroutine);
        _slideCoroutine = StartCoroutine(AnimateSlider(tabSlider, targetX, targetWidth));
    }

    private IEnumerator FadeIn(CanvasGroup cg, float duration = 0.2f)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    // Setzt X + Y einmalig aus Button + Indicator (Y ändert sich zwischen Tabs nie)
    private void SnapSlider(RectTransform slider, RectTransform buttonRT, RectTransform indicatorRT)
    {
        Vector3 pos = slider.localPosition;
        pos.x = buttonRT.localPosition.x;
        pos.y = buttonRT.localPosition.y + indicatorRT.localPosition.y;
        slider.localPosition = pos;
        slider.sizeDelta = new Vector2(indicatorRT.rect.width, slider.sizeDelta.y);
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
}
