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

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

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

        var logic = FindObjectOfType<LogicScript>();
        if (logic == null) return;

        SetPresenceGame(logic.playerScore, RankedManager.IsRanked);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode _)
    {
        _updateTimer = UpdateInterval; // sofort beim nächsten Update-Tick updaten

        switch (scene.name)
        {
            case "MainMenu":      SetPresenceMenu("Im Hauptmenü");  break;
            case "GameScene":     SetPresenceGame(0, RankedManager.IsRanked); break;
            case "ItemShop":      SetPresenceMenu("Im Shop");       break;
            case "SettingsScene": SetPresenceMenu("Einstellungen"); break;
        }
    }

    void SetPresenceGame(int score, bool ranked)
    {
        _client?.SetPresence(new RichPresence
        {
            Details    = $"Score: {score}",
            State      = ranked ? "Ranked" : "Quickplay",
            Timestamps = Timestamps.Now,
            Assets     = new DiscordRPC.Assets { LargeImageKey = "icon" }
        });
    }

    void SetPresenceMenu(string details)
    {
        _client?.SetPresence(new RichPresence
        {
            Details = details,
            Assets  = new DiscordRPC.Assets { LargeImageKey = "icon" }
        });
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        _client?.Dispose();
    }
}
