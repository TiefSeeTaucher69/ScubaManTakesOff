using System.Threading.Tasks;
using Unity.Services.Leaderboards;
using UnityEngine;

public class LeaderboardSenderScript : MonoBehaviour
{
    public const string LeaderboardId = "SMtakesoffLeaderboard";
    public const string RankedLeaderboardId = "SMtakesoffRankedLeaderboard";

    public async Task SendScore(int score)
    {
        try
        {
            await LeaderboardsService.Instance.AddPlayerScoreAsync(LeaderboardId, score);
            Debug.Log("Score erfolgreich gesendet: " + score);
            _ = DiscordRichPresenceManager.FetchRanksAndRefreshAsync();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Fehler beim Senden des Scores: " + e.Message);
        }
    }

    public async Task SendRankedScore(int score)
    {
        try
        {
            await LeaderboardsService.Instance.AddPlayerScoreAsync(RankedLeaderboardId, score);
            Debug.Log("Ranked Score erfolgreich gesendet: " + score);
            _ = DiscordRichPresenceManager.FetchRanksAndRefreshAsync();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Fehler beim Senden des Ranked Scores: " + e.Message);
        }
    }

    [System.Serializable]
    public class ScoreData
    {
        public string username;
        public int score;
        public int rank;
    }
}
