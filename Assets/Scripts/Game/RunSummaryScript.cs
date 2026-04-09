using System.Collections;
using TMPro;
using UnityEngine;

public class RunSummaryScript : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private TMP_Text scoreValueText;
    [SerializeField] private TMP_Text newRecordLabel;
    [SerializeField] private CanvasGroup newRecordGroup;
    [SerializeField] private TMP_Text leavesText;
    [SerializeField] private TMP_Text timeText;

    public void ZeigeSummary(int score, int leaves, float runTime, bool neuerRekord)
    {
        leavesText.text = leaves.ToString();
        timeText.text = FormatZeit(runTime);

        if (newRecordGroup != null)
            newRecordGroup.alpha = 0f;

        StartCoroutine(ZaehleHoch(score, neuerRekord));
    }

    private IEnumerator ZaehleHoch(int ziel, bool neuerRekord)
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

    private string FormatZeit(float sekunden)
    {
        int minuten = (int)(sekunden / 60f);
        int sek = (int)(sekunden % 60f);
        return minuten > 0 ? $"{minuten}:{sek:D2}" : $"0:{sek:D2}";
    }
}
