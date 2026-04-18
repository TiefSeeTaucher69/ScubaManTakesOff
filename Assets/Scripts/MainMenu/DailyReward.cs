using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class DailyReward : MonoBehaviour
{
    public int cannabisRewardToAdd = 5; // Belohnung für tägliches Einloggen

    [Header("UI Elemente")]
    public GameObject panel;
    public TMPro.TMP_Text statusText;
    public Button rewardButton;
    public Image rewardImage;
    public Sprite rewardSprite;
    public TMPro.TMP_Text cannabisStashText;
    


    [Header("Audio")]
    public AudioSource coinAudioSource;

    private string currentDate;
    private const string rewardKey = "lastRewardDate";

    void Start()
    {
        if (RemoteConfigManager.Instance != null)
            cannabisRewardToAdd = RemoteConfigManager.Instance.DailyRewardCannabis;

        Debug.Log("🔄 Starte DailyReward...");
        rewardButton.interactable = false;
        rewardButton.onClick.AddListener(OnRewardButtonClicked);
        rewardImage.sprite = rewardSprite;
        panel.SetActive(false);

        DateTime utcNow = DateTime.UtcNow;

        TimeZoneInfo berlinZone = null;
        try { berlinZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin"); }
        catch (TimeZoneNotFoundException)
        {
            try { berlinZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"); }
            catch (TimeZoneNotFoundException) { Debug.LogWarning("⚠️ Berlin-Zeitzone nicht gefunden, benutze UTC"); }
        }

        DateTime localNow = berlinZone != null
            ? TimeZoneInfo.ConvertTimeFromUtc(utcNow, berlinZone)
            : utcNow;

        currentDate = localNow.ToString("yyyy-MM-dd");
        Debug.Log("📅 Aktuelles Datum: " + currentDate);

        CheckRewardAvailability(localNow.Date);
    }

    void CheckRewardAvailability(DateTime serverDate)
    {
        string savedDateString = PlayerPrefs.GetString(rewardKey, "2000-01-01");
        Debug.Log("📦 Gespeichertes Datum: " + savedDateString);

        if (DateTime.TryParse(savedDateString, out DateTime savedDate))
        {
            if (savedDate < serverDate)
            {
                Debug.Log("Tägliche Belohnung verfügbar!");
                ShowPanel("Tägliche Belohnung verfügbar!", true);
            }
            else
            {
                Debug.Log("Heute bereits abgeholt.");
                // Panel **nicht** anzeigen, wenn Belohnung heute schon abgeholt wurde
                panel.SetActive(false);
            }
        }
        else
        {
            Debug.Log("Kein gespeichertes Datum vorhanden. Erste Belohnung möglich.");
            ShowPanel("Tägliche Belohnung verfügbar!", true);
        }
    }


    void ShowPanel(string message, bool buttonActive)
    {
        statusText.text = message;
        rewardButton.interactable = buttonActive;
        panel.SetActive(true);
        StartCoroutine(FadeInPanel());
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
    }

    void OnRewardButtonClicked()
    {
        Debug.Log("🎉 Belohnung eingesammelt!");
        if (coinAudioSource != null)
            coinAudioSource.Play();

        rewardButton.interactable = false;

        int newStash = PlayerPrefs.GetInt("CannabisStash", 0) + cannabisRewardToAdd;
        Debug.Log("Adding cannabis score: " + cannabisRewardToAdd);
        CloudSaveManager.Instance.SaveBatch(new System.Collections.Generic.Dictionary<string, object>
        {
            { "CannabisStash",  newStash     },
            { rewardKey,        currentDate  }
        });
        if (cannabisStashText != null) cannabisStashText.text = newStash.ToString();

        ToastManager.Show($"Daily reward! +{cannabisRewardToAdd} Cannabis", ToastType.Reward);
        ClosePanel();
    }

    IEnumerator FadeInPanel()
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
        }

        panel.SetActive(true);

        float duration = 0.5f;
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / duration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

}

