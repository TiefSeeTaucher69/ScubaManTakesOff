using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.LSS;
using UnityEngine.UI;
using Unity.Services.Authentication;
using Unity.Services.Core;
using static LeaderboardSenderScript;

public class MenuHandlerScript : MonoBehaviour
{
    public TMPro.TMP_Text highscoreText;
    public TMPro.TMP_Text cannabisStash;
    public TMPro.TMP_Text usernameText;
    public LeaderboardGetterScript leaderboardGetterScript; // Reference to the script that fetches scores
    public Transform scoreListContainer;
    public GameObject scoreEntryPrefab; // Prefab for displaying each score entry
    public WeeklyMissionUI weeklyMissionUI; // Reference to the WeeklyMissionUI script
    public WeeklyMissionRewardScript weeklyMissionRewardScript; // Inspector zuweisen
    public GameObject quitPanel;

    [Header("Play-Bar")]
    [SerializeField] private Button btnPlayQuickplay;
    [SerializeField] private Button btnPlayRanked;
    [SerializeField] private Image indicatorPlayQuickplay;
    [SerializeField] private Image indicatorPlayRanked;
    [SerializeField] private GameObject pnlRankedInfo;
    [SerializeField] private Image imgRankedItem;
    [SerializeField] private TMPro.TMP_Text txtRankedItem;
    [SerializeField] private TMPro.TMP_Text txtRankedReset;
    [SerializeField] private Sprite spriteInvincible;
    [SerializeField] private Sprite spriteShrink;
    [SerializeField] private Sprite spriteLaser;
    [SerializeField] private Sprite spriteSlowMo;
    [SerializeField] private Sprite spriteShield;

    [Header("Scoreboard-Bar")]
    [SerializeField] private Button btnScoreQuickplay;
    [SerializeField] private Button btnScoreRanked;
    [SerializeField] private Image indicatorScoreQuickplay;
    [SerializeField] private Image indicatorScoreRanked;

    [Header("Slider")]
    [SerializeField] private RectTransform playSlider;
    [SerializeField] private RectTransform scoreSlider;

    private Coroutine _playSlide;
    private Coroutine _scoreSlide;
    private System.Threading.CancellationTokenSource _scoreCts;

    [ContextMenu("Add Cannabis")]
    void DebugAddCannabis()
    {
        PlayerPrefs.SetInt("CannabisStash", 9999);
        PlayerPrefs.Save();
    }


    public void StartGame()
    {
        LSS_LoadingScreen.LoadScene("GameScene", "StandardPAK");
        Debug.Log("Game Started");
    }

    async void Start()
    {
        Cursor.visible = true;

        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }

        int highscore = PlayerPrefs.GetInt("Highscore", 0);
        if (highscoreText != null) highscoreText.text = highscore.ToString();
        Debug.Log("Highscore loaded: " + highscore);

        string username = PlayerPrefs.GetString("Username", "Guest");
        if (usernameText != null) usernameText.text = username;

        RankedManager.IsRanked = false;

        if (btnPlayQuickplay != null) btnPlayQuickplay.onClick.AddListener(OnPlayQuickplay);
        if (btnPlayRanked != null)    btnPlayRanked.onClick.AddListener(OnPlayRanked);
        if (btnScoreQuickplay != null) btnScoreQuickplay.onClick.AddListener(OnScoreboardQuickplay);
        if (btnScoreRanked != null)    btnScoreRanked.onClick.AddListener(OnScoreboardRanked);

        Canvas.ForceUpdateCanvases();
        SnapSlider(playSlider, btnPlayQuickplay, indicatorPlayQuickplay);
        OnPlayQuickplay();
        SnapSlider(scoreSlider, btnScoreQuickplay, indicatorScoreQuickplay);
        OnScoreboardQuickplay();
        InitRankedInfo();

        if (cannabisStash != null)
            cannabisStash.text = PlayerPrefs.GetInt("CannabisStash", 0).ToString();

        Debug.Log("WeeklyMissionRewardScript im MenuHandler: " + (weeklyMissionRewardScript != null));
        var missionManager = WeeklyMissionManager.Instance;
        if (missionManager != null)
        {
            missionManager.OnMissionsLoaded += OnMissionsLoaded;
            missionManager.weeklyMissionRewardScript = weeklyMissionRewardScript;
            // Jetzt Missionen neu laden und UI danach updaten
            missionManager.ReloadMissions();
        }
        else
        {
            Debug.LogWarning("WeeklyMissionManager.Instance ist null im MenuHandler");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            if (quitPanel != null) quitPanel.SetActive(true);
        }
    }

    public void QuitGame() => Application.Quit();
    public void CloseQuitPanel() { if (quitPanel != null) quitPanel.SetActive(false); }

    private void OnPlayQuickplay()
    {
        RankedManager.IsRanked = false;
        if (pnlRankedInfo != null) pnlRankedInfo.SetActive(false);
        SlidePlayTo(indicatorPlayQuickplay);
    }

    private void OnPlayRanked()
    {
        RankedManager.IsRanked = true;
        if (pnlRankedInfo != null) pnlRankedInfo.SetActive(true);
        SlidePlayTo(indicatorPlayRanked);
        UpdateRankedResetText();
    }

    private async void OnScoreboardQuickplay()
    {
        _scoreCts?.Cancel();
        _scoreCts = new System.Threading.CancellationTokenSource();
        var token = _scoreCts.Token;
        ClearScores();
        SlideScoreTo(indicatorScoreQuickplay);
        var scores = await leaderboardGetterScript.GetScores();
        if (!token.IsCancellationRequested) ShowScores(scores);
    }

    private async void OnScoreboardRanked()
    {
        _scoreCts?.Cancel();
        _scoreCts = new System.Threading.CancellationTokenSource();
        var token = _scoreCts.Token;
        ClearScores();
        SlideScoreTo(indicatorScoreRanked);
        var scores = await leaderboardGetterScript.GetRankedScores();
        if (!token.IsCancellationRequested) ShowScores(scores);
    }

    private void SnapSlider(RectTransform slider, Button btn, Image indicator)
    {
        if (slider == null || btn == null || indicator == null) return;
        RectTransform btnRT = btn.GetComponent<RectTransform>();
        RectTransform indRT = indicator.rectTransform;
        Vector3 pos = slider.localPosition;
        pos.x = btnRT.localPosition.x;
        pos.y = btnRT.localPosition.y + indRT.localPosition.y;
        slider.localPosition = pos;
        slider.sizeDelta = new Vector2(indRT.rect.width, slider.sizeDelta.y);
    }

    private void SlidePlayTo(Image targetIndicator)
    {
        if (playSlider == null || targetIndicator == null) return;
        Button btn = targetIndicator.GetComponentInParent<Button>();
        if (btn == null) return;
        float x = btn.GetComponent<RectTransform>().localPosition.x;
        float w = targetIndicator.rectTransform.rect.width;
        if (_playSlide != null) StopCoroutine(_playSlide);
        _playSlide = StartCoroutine(AnimateSlider(playSlider, x, w));
    }

    private void SlideScoreTo(Image targetIndicator)
    {
        if (scoreSlider == null || targetIndicator == null) return;
        Button btn = targetIndicator.GetComponentInParent<Button>();
        if (btn == null) return;
        float x = btn.GetComponent<RectTransform>().localPosition.x;
        float w = targetIndicator.rectTransform.rect.width;
        if (_scoreSlide != null) StopCoroutine(_scoreSlide);
        _scoreSlide = StartCoroutine(AnimateSlider(scoreSlider, x, w));
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

    private void ClearScores()
    {
        foreach (Transform child in scoreListContainer)
            Destroy(child.gameObject);
    }

    private void InitRankedInfo()
    {
        if (txtRankedItem != null)
            txtRankedItem.text = RankedManager.GetWeeklyItemDisplayName();

        if (imgRankedItem != null)
        {
            imgRankedItem.sprite = RankedManager.WeeklyItem switch
            {
                "Invincible" => spriteInvincible,
                "Shrink"     => spriteShrink,
                "Laser"      => spriteLaser,
                "SlowMo"     => spriteSlowMo,
                "Shield"     => spriteShield,
                _            => null
            };
        }

        UpdateRankedResetText();
    }

    private void UpdateRankedResetText()
    {
        if (txtRankedReset == null) return;
        System.TimeSpan t = RankedManager.GetTimeUntilReset();
        txtRankedReset.text = $"Reset in: {(int)t.TotalDays}T {t.Hours}Std {t.Minutes}Min";
    }

    public void ShowScores(List<ScoreData> scores)
    {
        Debug.Log(scores == null ? "ShowScores: scores ist null" : $"ShowScores: {scores.Count} Eintr\u00e4ge");
        if (scores == null)
        {
            Debug.LogError("ShowScores wurde mit null aufgerufen.");
            return;
        }

        foreach (Transform child in scoreListContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var entry in scores)
        {
            if (entry == null) continue;

            GameObject entryGO = Instantiate(scoreEntryPrefab, scoreListContainer);
            Transform rankT     = entryGO.transform.Find("Txt_Rank");
            Transform usernameT = entryGO.transform.Find("Txt_Username");
            Transform scoreT    = entryGO.transform.Find("Txt_Score");
            var txtRankField     = rankT     != null ? rankT.GetComponent<TMPro.TMP_Text>()     : null;
            var txtUsernameField = usernameT != null ? usernameT.GetComponent<TMPro.TMP_Text>() : null;
            var txtScoreField    = scoreT    != null ? scoreT.GetComponent<TMPro.TMP_Text>()    : null;

            if (txtRankField != null && txtUsernameField != null && txtScoreField != null)
            {
                string fullPlayerName = (AuthenticationService.Instance != null && AuthenticationService.Instance.IsSignedIn)
                    ? AuthenticationService.Instance.PlayerName : null;
                bool isLocalPlayer = !string.IsNullOrEmpty(fullPlayerName) && entry.username == fullPlayerName;

                txtRankField.text     = "#" + entry.rank;
                txtUsernameField.text = isLocalPlayer
                    ? entry.username + " <color=#00e676>(You)</color>"
                    : entry.username;
                txtScoreField.text    = entry.score.ToString();

                Color entryColor = entry.rank switch
                {
                    1 => new Color(1.00f, 0.84f, 0.00f), // Gold
                    2 => new Color(0.75f, 0.75f, 0.75f), // Silber
                    3 => new Color(0.80f, 0.50f, 0.20f), // Bronze
                    _ => Color.white
                };
                txtRankField.color     = entryColor;
                txtUsernameField.color = entryColor;
                txtScoreField.color    = entryColor;
            }
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(scoreListContainer.GetComponent<RectTransform>());
    }

    private void OnMissionsLoaded()
    {
        Debug.Log("Missions wurden geladen - UpdateUI wird aufgerufen");
        if (weeklyMissionUI != null)
        {
            weeklyMissionUI.UpdateUI();
        }
    }

    private void OnDestroy()
    {
        _scoreCts?.Cancel();
        var missionManager = WeeklyMissionManager.Instance;
        if (missionManager != null)
        {
            missionManager.OnMissionsLoaded -= OnMissionsLoaded;
        }
    }
}
