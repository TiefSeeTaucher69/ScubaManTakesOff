using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

public class LogicScript : MonoBehaviour
{
    public int playerScore;
    public int highScore;
    public TMPro.TMP_Text scoreText;
    public TMPro.TMP_Text cannabisStashText;
    public GameObject gameOverScreen;
    public GameObject menuScreen;
    public LeaderboardSenderScript leaderboardSenderScript;
    private bool hasGameOverBeenHandled = false;
    [SerializeField] private GameObject cannabisCollectedPrefab;
    [SerializeField] private Transform playerHead;
    private int collectedLeavesInCurrentRun = 0;
    private bool collectedBlattInCurrentRun = false;
    public SteffScript steff;

    private void Start()
    {
        if (steff == null)
            steff = FindObjectOfType<SteffScript>();

        highScore = PlayerPrefs.GetInt("Highscore", 0);
        leaderboardSenderScript = GameObject.Find("LeaderboardSender").GetComponent<LeaderboardSenderScript>();
        cannabisStashText.text = PlayerPrefs.GetInt("CannabisStash", 0).ToString();
        collectedLeavesInCurrentRun = 0;
    }

    [ContextMenu("Increase CannabisScore")]
    public void addCannabisScore(int scoreToAdd)
    {
        Debug.Log("Adding cannabis score: " + scoreToAdd);
        PlayerPrefs.SetInt("CannabisStash", PlayerPrefs.GetInt("CannabisStash", 0) + scoreToAdd);
        PlayerPrefs.Save();
        cannabisStashText.text = PlayerPrefs.GetInt("CannabisStash", 0).ToString();

        collectedLeavesInCurrentRun += scoreToAdd;

        if (WeeklyMissionManager.Instance != null)
        {
            WeeklyMissionManager.Instance.UpdateMission(MissionType.CollectBlatt, scoreToAdd);
            collectedBlattInCurrentRun = true;
        }
        else
        {
            Debug.LogWarning("WeeklyMissionManager.Instance ist null in addCannabisScore!");
        }

        steff?.PlaySmokeEffect();

        if (cannabisCollectedPrefab != null && playerHead != null)
        {
            Debug.Log("Instantiating Cannabis Animation Icon");
            GameObject icon = Instantiate(cannabisCollectedPrefab, playerHead.position + Vector3.up * 1f, Quaternion.identity, playerHead);

            AudioSource audio = icon.GetComponent<AudioSource>();
            if (audio != null)
            {
                audio.Play();
            }

            StartCoroutine(AnimateCannabisCollected(icon.transform));
        }
    }

    [ContextMenu("Increase Score")]
    public void addScore(int scoreToAdd)
    {
        playerScore = playerScore + scoreToAdd;
        scoreText.text = playerScore.ToString();
    }

    public void restartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void gameOver()
    {
        if (hasGameOverBeenHandled) return;
        hasGameOverBeenHandled = true;

        gameOverScreen.SetActive(true);
        Cursor.visible = true;

        // Controller-Fokus auf ersten Button setzen
        var firstButton = gameOverScreen.GetComponentInChildren<Button>();
        if (firstButton != null)
            EventSystem.current?.SetSelectedGameObject(firstButton.gameObject);

        if (RankedManager.IsRanked)
        {
            int rankedHighScore = PlayerPrefs.GetInt("RankedHighscore", 0);
            if (playerScore > rankedHighScore)
            {
                PlayerPrefs.SetInt("RankedHighscore", playerScore);
                PlayerPrefs.Save();
                Debug.Log("Neuer Ranked Highscore: " + playerScore);
                _ = leaderboardSenderScript.SendRankedScore(playerScore);
            }
            else
            {
                Debug.Log("Kein neuer Ranked Highscore. Aktueller: " + rankedHighScore);
            }
        }
        else
        {
            if (playerScore > highScore)
            {
                PlayerPrefs.SetInt("Highscore", playerScore);
                PlayerPrefs.Save();
                Debug.Log("New high score saved: " + playerScore);
                _ = leaderboardSenderScript.SendScore(playerScore);
            }
            else
            {
                Debug.Log("Kein neuer Highscore. Aktueller: " + highScore);
            }
        }

        int score = playerScore;
        int collectedLeaves = collectedLeavesInCurrentRun;
        float runTime = steff.runTime;
        bool survived30Seconds = runTime >= 30f;

        OnRunEnd(score, collectedLeaves, runTime, survived30Seconds);

        SpeedManager.ResetSpeed();
        SpeedManagerCannabisScript.ResetSpeed();
    }

    public void backtoMenu()
    {
        Debug.Log("Going to main menu");
        SceneManager.LoadScene("MainMenu");
        WeeklyMissionManager.Instance.NotifyMissionsLoaded();
    }

    private IEnumerator AnimateCannabisCollected(Transform iconTransform)
    {
        Vector3 startPos = iconTransform.localPosition;
        Quaternion startRot = iconTransform.rotation;

        float jumpHeight = 1f;
        float rotationAmount = 360f;
        float duration = 0.6f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float height = 4 * jumpHeight * t * (1 - t);
            iconTransform.localPosition = startPos + Vector3.up * height;
            iconTransform.rotation = startRot * Quaternion.Euler(0, rotationAmount * t, 0);

            yield return null;
        }

        Destroy(iconTransform.gameObject);
    }

    public void OnRunEnd(int score, int collectedLeaves, float runTime, bool survived30Seconds)
    {
        Debug.Log($"OnRunEnd called: Score={score}, Leaves={collectedLeaves}, Time={runTime}, Survived30s={survived30Seconds}");

        if (WeeklyMissionManager.Instance != null)
        {
            Debug.Log("Updating missions from OnRunEnd");

            // ⏫ NEU: Gesamtscore berechnen
            int previousTotalScore = PlayerPrefs.GetInt("TotalScore", 0);
            int newTotalScore = previousTotalScore + score;
            PlayerPrefs.SetInt("TotalScore", newTotalScore);
            PlayerPrefs.Save();

            WeeklyMissionManager.Instance.UpdateMission(MissionType.TotalScore, score);

            WeeklyMissionManager.Instance.UpdateMission(MissionType.TimeInOneRun, (int)runTime);
            WeeklyMissionManager.Instance.UpdateMission(MissionType.TotalTime, (int)runTime);
            WeeklyMissionManager.Instance.UpdateMission(MissionType.TotalRuns, 1);
            WeeklyMissionManager.Instance.UpdateMission(MissionType.CollectInOneRun, collectedLeaves);

            if (collectedBlattInCurrentRun)
            {
                WeeklyMissionManager.Instance.UpdateMission(MissionType.CollectStreak, 1);
                Debug.Log("CollectStreak updated +1");
            }
            else
            {
                WeeklyMissionManager.Instance.UpdateMission(MissionType.CollectStreak, 0);
                Debug.Log("CollectStreak reset to 0");
            }

            if (survived30Seconds)
            {
                WeeklyMissionManager.Instance.UpdateMission(MissionType.TimeStreak, 1);
                Debug.Log("TimeStreak updated +1");
            }
            else
            {
                WeeklyMissionManager.Instance.UpdateMission(MissionType.TimeStreak, 0);
                Debug.Log("TimeStreak reset to 0");
            }
        }
        else
        {
            Debug.LogWarning("WeeklyMissionManager.Instance ist null in OnRunEnd!");
        }

        collectedLeavesInCurrentRun = 0;
        collectedBlattInCurrentRun = false;
    }


    [ContextMenu("Test Mission Update")]
    public void TestMissionUpdate()
    {
        if (WeeklyMissionManager.Instance != null)
        {
            Debug.Log("Testing mission update...");
            WeeklyMissionManager.Instance.UpdateMission(MissionType.CollectBlatt, 1);
            WeeklyMissionManager.Instance.UpdateMission(MissionType.TotalScore, 100);
            WeeklyMissionManager.Instance.UpdateMission(MissionType.TotalRuns, 1);
        }
        else
        {
            Debug.LogWarning("WeeklyMissionManager.Instance ist null!");
        }
    }
}
