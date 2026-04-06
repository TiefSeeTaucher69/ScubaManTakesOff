using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    [Header("Scoreboard-Bar")]
    [SerializeField] private Button btnScoreQuickplay;
    [SerializeField] private Button btnScoreRanked;
    [SerializeField] private Image indicatorScoreQuickplay;
    [SerializeField] private Image indicatorScoreRanked;

    private readonly Color colorActive   = new Color(0.13f, 0.77f, 0.37f, 1f); // #22C55E
    private readonly Color colorInactive = new Color(0.33f, 0.33f, 0.33f, 1f); // #555555
    private System.Threading.CancellationTokenSource _scoreCts;

    [ContextMenu("Add Cannabis")]
    void DebugAddCannabis()
    {
        PlayerPrefs.SetInt("CannabisStash", 9999);
        PlayerPrefs.Save();
    }


    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
        Debug.Log("Game Started");
    }

    public void LoadItemShop()
    {
        Debug.Log("Loading Item Shop Scene");
        SceneManager.LoadScene("ItemShop");
    }

    async void Start()
    {
        Cursor.visible = true;

        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        int highscore = PlayerPrefs.GetInt("Highscore", 0);
        highscoreText.text = highscore.ToString();
        Debug.Log("Highscore loaded: " + highscore);

        string username = PlayerPrefs.GetString("Username", "Guest");
        usernameText.text = username.ToString();

        RankedManager.IsRanked = false;

        if (btnPlayQuickplay != null) btnPlayQuickplay.onClick.AddListener(OnPlayQuickplay);
        if (btnPlayRanked != null)    btnPlayRanked.onClick.AddListener(OnPlayRanked);
        if (btnScoreQuickplay != null) btnScoreQuickplay.onClick.AddListener(OnScoreboardQuickplay);
        if (btnScoreRanked != null)    btnScoreRanked.onClick.AddListener(OnScoreboardRanked);

        OnPlayQuickplay();
        OnScoreboardQuickplay();
        InitRankedInfo();

        cannabisStash.text = PlayerPrefs.GetInt("CannabisStash", 0).ToString();
        Debug.Log("Cannabis stash loaded: " + cannabisStash.text);

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
        if (indicatorPlayQuickplay != null) indicatorPlayQuickplay.color = colorActive;
        if (indicatorPlayRanked != null)    indicatorPlayRanked.color    = colorInactive;
    }

    private void OnPlayRanked()
    {
        RankedManager.IsRanked = true;
        if (pnlRankedInfo != null) pnlRankedInfo.SetActive(true);
        if (indicatorPlayQuickplay != null) indicatorPlayQuickplay.color = colorInactive;
        if (indicatorPlayRanked != null)    indicatorPlayRanked.color    = colorActive;
        UpdateRankedResetText();
    }

    private async void OnScoreboardQuickplay()
    {
        _scoreCts?.Cancel();
        _scoreCts = new System.Threading.CancellationTokenSource();
        var token = _scoreCts.Token;
        ClearScores();
        if (indicatorScoreQuickplay != null) indicatorScoreQuickplay.color = colorActive;
        if (indicatorScoreRanked != null)    indicatorScoreRanked.color    = colorInactive;
        var scores = await leaderboardGetterScript.GetScores();
        if (!token.IsCancellationRequested) ShowScores(scores);
    }

    private async void OnScoreboardRanked()
    {
        _scoreCts?.Cancel();
        _scoreCts = new System.Threading.CancellationTokenSource();
        var token = _scoreCts.Token;
        ClearScores();
        if (indicatorScoreQuickplay != null) indicatorScoreQuickplay.color = colorInactive;
        if (indicatorScoreRanked != null)    indicatorScoreRanked.color    = colorActive;
        var scores = await leaderboardGetterScript.GetRankedScores();
        if (!token.IsCancellationRequested) ShowScores(scores);
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
                txtRankField.text     = "#" + entry.rank;
                txtUsernameField.text = entry.username;
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
