using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RunSummaryScript : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private TMP_Text scoreValueText;
    [SerializeField] private TMP_Text newRecordLabel;
    [SerializeField] private CanvasGroup newRecordGroup;
    [SerializeField] private TMP_Text leavesText;
    [SerializeField] private TMP_Text timeText;

    [Header("XP")]
    [SerializeField] private TMP_Text    Txt_XPGained;
    [SerializeField] private TMP_Text    Txt_LevelCurrent;
    [SerializeField] private Image       Img_XPBarFill;
    [SerializeField] private TMP_Text    Txt_XPProgress;
    [SerializeField] private CanvasGroup CG_LevelUp;

    public void ZeigeSummary(int score, int leaves, float runTime, bool neuerRekord,
                              int xpGained = 0, int oldTotalXP = 0)
    {
        leavesText.text = leaves.ToString();
        timeText.text = FormatZeit(runTime);

        if (newRecordGroup != null)
            newRecordGroup.alpha = 0f;

        int newTotalXP = Mathf.Min(oldTotalXP + xpGained, XPManager.XPCap);
        int oldLevel   = XPManager.GetLevel(oldTotalXP);
        int newLevel   = XPManager.GetLevel(newTotalXP);

        if (Txt_XPGained != null)
            Txt_XPGained.text = $"+{xpGained} XP";
        if (Txt_LevelCurrent != null)
            Txt_LevelCurrent.text = $"Level {oldLevel}";
        if (Img_XPBarFill != null)
            Img_XPBarFill.fillAmount = BarFillAt(oldTotalXP);
        if (Txt_XPProgress != null)
            Txt_XPProgress.text = XPProgressText(oldTotalXP);
        if (CG_LevelUp != null)
            CG_LevelUp.alpha = 0f;

        StartCoroutine(ZaehleHoch(score, neuerRekord, oldTotalXP, newTotalXP, oldLevel, newLevel));
    }

    private IEnumerator ZaehleHoch(int ziel, bool neuerRekord,
                                    int oldTotalXP, int newTotalXP, int oldLevel, int newLevel)
    {
        float dauer = 0.8f;
        float vergangen = 0f;

        while (vergangen < dauer)
        {
            vergangen += Time.deltaTime;
            float t = Mathf.Clamp01(vergangen / dauer);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            scoreValueText.text = Mathf.RoundToInt(eased * ziel).ToString();
            yield return null;
        }

        scoreValueText.text = ziel.ToString();

        if (neuerRekord && newRecordGroup != null)
            StartCoroutine(BlendEin(newRecordGroup));

        yield return new WaitForSeconds(0.3f);

        yield return StartCoroutine(AnimiereXPBar(oldTotalXP, newTotalXP, oldLevel, newLevel));
    }

    private IEnumerator AnimiereXPBar(int oldTotalXP, int newTotalXP, int oldLevel, int newLevel)
    {
        const float segmentDuration = 0.7f;
        const float levelFadeIn     = 0.25f;
        const float levelHold       = 0.5f;
        const float levelFadeOut    = 0.2f;

        int currentXP = oldTotalXP;

        for (int lvl = oldLevel; lvl <= newLevel; lvl++)
        {
            bool isLastSegment = lvl == newLevel;
            int  segmentTarget = isLastSegment ? newTotalXP : XPManager.TotalXPForLevel(lvl + 1);

            float startFill = BarFillAt(currentXP);
            float endFill   = isLastSegment ? BarFillAt(newTotalXP) : 1f;

            float elapsed = 0f;
            while (elapsed < segmentDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / segmentDuration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                if (Img_XPBarFill != null)
                    Img_XPBarFill.fillAmount = Mathf.Lerp(startFill, endFill, eased);
                int liveXP = Mathf.RoundToInt(Mathf.Lerp(currentXP, segmentTarget, eased));
                if (Txt_XPProgress != null)
                    Txt_XPProgress.text = XPProgressText(liveXP);
                yield return null;
            }

            if (Img_XPBarFill != null) Img_XPBarFill.fillAmount = endFill;
            if (Txt_XPProgress != null) Txt_XPProgress.text = XPProgressText(segmentTarget);
            currentXP = segmentTarget;

            if (!isLastSegment && CG_LevelUp != null)
            {
                // Fade in "LEVEL UP!"
                float t2 = 0f;
                while (t2 < levelFadeIn)
                {
                    t2 += Time.deltaTime;
                    CG_LevelUp.alpha = Mathf.Clamp01(t2 / levelFadeIn);
                    yield return null;
                }
                CG_LevelUp.alpha = 1f;
                yield return new WaitForSeconds(levelHold);

                // Fade out and reset bar
                t2 = 0f;
                while (t2 < levelFadeOut)
                {
                    t2 += Time.deltaTime;
                    CG_LevelUp.alpha = 1f - Mathf.Clamp01(t2 / levelFadeOut);
                    yield return null;
                }
                CG_LevelUp.alpha = 0f;

                if (Img_XPBarFill != null) Img_XPBarFill.fillAmount = 0f;
                if (Txt_LevelCurrent != null) Txt_LevelCurrent.text = $"Level {lvl + 1}";
                yield return new WaitForSeconds(0.15f);
            }
        }

        if (newLevel > oldLevel)
            ToastManager.Show($"Level Up! You reached Level {newLevel}!", ToastType.Reward);
    }

    private IEnumerator BlendEin(CanvasGroup cg)
    {
        float dauer = 0.3f;
        float vergangen = 0f;

        while (vergangen < dauer)
        {
            vergangen += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(vergangen / dauer);
            yield return null;
        }

        cg.alpha = 1f;
    }

    private float BarFillAt(int totalXP)
    {
        int level = XPManager.GetLevel(totalXP);
        if (level >= XPManager.MaxLevel) return 1f;
        float required = XPManager.GetXPRequired(totalXP);
        return required > 0 ? Mathf.Clamp01(XPManager.GetXPInLevel(totalXP) / required) : 0f;
    }

    private string XPProgressText(int totalXP)
    {
        int level = XPManager.GetLevel(totalXP);
        if (level >= XPManager.MaxLevel) return "MAX LEVEL";
        return $"{XPManager.GetXPInLevel(totalXP)} / {XPManager.GetXPRequired(totalXP)} XP";
    }

    private string FormatZeit(float sekunden)
    {
        int minuten = (int)(sekunden / 60f);
        int sek = (int)(sekunden % 60f);
        return minuten > 0 ? $"{minuten}:{sek:D2}" : $"0:{sek:D2}";
    }
}
