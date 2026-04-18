using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using UnityEngine;
using UnityEngine.SceneManagement;
using DiscordRPC;

public class DiscordRichPresenceManager : MonoBehaviour
{
    public static DiscordRichPresenceManager Instance { get; private set; }

    [SerializeField] private string clientId = "DEINE_CLIENT_ID_HIER";

    private DiscordRpcClient _client;
    private float            _updateTimer;
    private const float      UpdateInterval = 5f;

    // -1 = not on leaderboard / not fetched yet
    public static int QuickplayRank = -1;
    public static int RankedRank    = -1;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (string.IsNullOrEmpty(clientId) || clientId == "DEINE_CLIENT_ID_HIER")
        {
            Debug.LogWarning("[Discord] clientId not set in Inspector — Rich Presence disabled.");
            return;
        }
        _client = new DiscordRpcClient(clientId);
        _client.Initialize();

        SceneManager.sceneLoaded += OnSceneLoaded;
        SetPresenceMenu("Im Hauptmenü");
    }

    void Update()
    {
        _client?.Invoke();

        _updateTimer += Time.unscaledDeltaTime;
        if (_updateTimer < UpdateInterval) return;
        _updateTimer = 0f;

        if (SceneManager.GetActiveScene().name != "GameScene") return;

        var logic = FindFirstObjectByType<LogicScript>();
        if (logic == null) return;

        SetPresenceGame(logic.playerScore, RankedManager.IsRanked);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _updateTimer = UpdateInterval;

        switch (scene.name)
        {
            case "MainMenu":
                SetPresenceMenu("Im Hauptmenü");
                var _ = FetchRanksAndRefreshAsync();
                break;
            case "GameScene":
                SetPresenceGame(0, RankedManager.IsRanked);
                break;
        }
    }

    // Called on MainMenu load and after score submission.
    public static async Task FetchRanksAndRefreshAsync()
    {
        if (!AuthenticationService.Instance.IsSignedIn) return;

        try
        {
            var entry = await LeaderboardsService.Instance
                .GetPlayerScoreAsync(LeaderboardSenderScript.LeaderboardId);
            QuickplayRank = entry.Rank + 1; // 0-based → 1-based
        }
        catch { QuickplayRank = -1; }

        try
        {
            var entry = await LeaderboardsService.Instance
                .GetPlayerScoreAsync(LeaderboardSenderScript.RankedLeaderboardId);
            RankedRank = entry.Rank + 1;
        }
        catch { RankedRank = -1; }

        // Refresh presence with updated ranks
        if (Instance == null) return;
        string scene = SceneManager.GetActiveScene().name;
        if (scene == "MainMenu")
            Instance.SetPresenceMenu("Im Hauptmenü");
        else if (scene == "GameScene")
        {
            var logic = FindFirstObjectByType<LogicScript>();
            if (logic != null)
                Instance.SetPresenceGame(logic.playerScore, RankedManager.IsRanked);
        }
    }

    void SetPresenceGame(int score, bool ranked)
    {
        int    rank     = ranked ? RankedRank : QuickplayRank;
        string rankText = rank > 0 ? $" | #{rank}" : "";

        _client?.SetPresence(new RichPresence
        {
            Details    = $"Score: {score}",
            State      = (ranked ? "Ranked" : "Quickplay") + rankText,
            Timestamps = Timestamps.Now,
            Assets     = new DiscordRPC.Assets { LargeImageKey = "icon" }
        });
    }

    void SetPresenceMenu(string details)
    {
        string rankState = BuildRankState();

        _client?.SetPresence(new RichPresence
        {
            Details = details,
            // Discord requires State to be null or at least 2 chars
            State   = rankState.Length >= 2 ? rankState : null,
            Assets  = new DiscordRPC.Assets { LargeImageKey = "icon" }
        });
    }

    static string BuildRankState()
    {
        var parts = new List<string>();
        if (QuickplayRank > 0) parts.Add($"#{QuickplayRank} Quickplay");
        if (RankedRank    > 0) parts.Add($"#{RankedRank} Ranked");
        return string.Join(" | ", parts);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        _client?.Dispose();
    }
}
