using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MissionType
{
    CollectBlatt,
    CollectInOneRun,
    TotalScore,
    TimeInOneRun,
    TotalRuns,
    TotalTime,
    TotalJumps,
    CollectStreak,
    TimeStreak
}

[System.Serializable]
public class Mission
{
    public string description;
    public string id;
    public int goal;
    public int current;
    public bool isCompleted;
    public MissionType type;
}

public class WeeklyMissionManager : MonoBehaviour
{
    public static WeeklyMissionManager Instance { get; private set; }

    public event Action OnMissionsLoaded;
    public WeeklyMissionRewardScript weeklyMissionRewardScript;

    public List<Mission> allPossibleMissions;
    public List<Mission> activeMissions;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (transform.parent != null) transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadMissions(); // already calls CheckCompletedMissions() internally
    }

    public void LoadMissions()
    {
        DateTime now = DateTime.UtcNow;
        int daysBack = now.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)now.DayOfWeek - 1;
        DateTime thisMonday = now.Date.AddDays(-daysBack);

        if (!PlayerPrefs.HasKey("WeeklyMissionStartTime"))
        {
            Debug.Log("Keine gespeicherten Missionen gefunden - generiere neue");
            GenerateNewWeeklyMissions(thisMonday);
        }
        else
        {
            if (long.TryParse(PlayerPrefs.GetString("WeeklyMissionStartTime"), out long savedTime))
            {
                DateTime savedMonday = DateTime.FromBinary(savedTime);
                if (thisMonday > savedMonday)
                {
                    Debug.Log("Woche ist vorbei - generiere neue Missionen");
                    GenerateNewWeeklyMissions(thisMonday);
                }
                else
                {
                    Debug.Log("Lade bestehende Missionen");
                    LoadMissionsFromPrefs();
                }
            }
            else
            {
                Debug.LogWarning("WeeklyMissionStartTime ungültig – neue Missionen werden generiert.");
                GenerateNewWeeklyMissions(thisMonday);
            }
        }

        OnMissionsLoaded?.Invoke();
        CheckCompletedMissions();
    }

    private int GetWeekSeed(DateTime date)
    {
        System.Globalization.Calendar cal = System.Globalization.CultureInfo.InvariantCulture.Calendar;
        int week = cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        return date.Year * 100 + week;
    }

    public void GenerateNewWeeklyMissions(DateTime weekStart)
    {
        activeMissions = new List<Mission>();

        int seed = GetWeekSeed(weekStart);
        Debug.Log("Missions-Seed für diese Woche: " + seed);
        var rng = new System.Random(seed);
        var shuffled = allPossibleMissions.OrderBy(x => rng.Next()).ToList();
        var usedIds = new HashSet<string>();
        int added = 0;

        foreach (var mission in shuffled)
        {
            if (!usedIds.Contains(mission.id))
            {
                usedIds.Add(mission.id);

                activeMissions.Add(new Mission
                {
                    id = mission.id,
                    description = mission.description,
                    goal = mission.goal,
                    current = 0,
                    isCompleted = false,
                    type = mission.type
                });

                added++;
                if (added >= 3)
                    break;
            }
        }

        Debug.Log($"Neue Missionen generiert: {activeMissions.Count}");
        foreach (var mission in activeMissions)
        {
            Debug.Log($"Mission: {mission.description}, Type: {mission.type}, Goal: {mission.goal}");
        }

        CloudSaveManager.Instance.SaveString("WeeklyMissionStartTime", weekStart.ToBinary().ToString());
        SaveMissionsToPrefs();

        // Clear all reward collected flags for new missions
        ClearAllRewardCollectedFlags();
    }

    public void SaveMissionsToPrefs()
    {
        string json = JsonUtility.ToJson(new MissionWrapper { missions = activeMissions });
        CloudSaveManager.Instance.SaveString("WeeklyMissions", json);
        Debug.Log("Missionen gespeichert: " + json);
    }

    public void LoadMissionsFromPrefs()
    {
        if (!PlayerPrefs.HasKey("WeeklyMissions")) return;

        try
        {
            string json = PlayerPrefs.GetString("WeeklyMissions");
            Debug.Log("Lade Missionen aus PlayerPrefs: " + json);

            MissionWrapper wrapper = JsonUtility.FromJson<MissionWrapper>(json);

            if (wrapper == null || wrapper.missions == null)
            {
                Debug.LogWarning("Weekly Missions JSON ist leer oder ungültig. Neue Missionen werden generiert.");
                DateTime utcNow = DateTime.UtcNow;
                int daysBack = utcNow.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)utcNow.DayOfWeek - 1;
                GenerateNewWeeklyMissions(utcNow.Date.AddDays(-daysBack));
                return;
            }

            activeMissions = wrapper.missions;
            FixMissionTypes();

            Debug.Log($"Missionen erfolgreich geladen: {activeMissions.Count}");
            foreach (var mission in activeMissions)
            {
                Debug.Log($"Geladene Mission: {mission.description}, Type: {mission.type}, Current: {mission.current}, Goal: {mission.goal}");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Fehler beim Laden der Weekly Missions: " + e.Message);
            activeMissions = new List<Mission>();

            DateTime utcNow = DateTime.UtcNow;
            int daysBack = utcNow.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)utcNow.DayOfWeek - 1;
            GenerateNewWeeklyMissions(utcNow.Date.AddDays(-daysBack));
        }
    }

    private void FixMissionTypes()
    {
        foreach (var activeMission in activeMissions)
        {
            var originalMission = allPossibleMissions.FirstOrDefault(m => m.id == activeMission.id);
            if (originalMission != null)
                activeMission.type = originalMission.type;
        }
    }

    public void UpdateMission(MissionType type, int amount)
    {
        Debug.Log($"UpdateMission aufgerufen: Type={type}, Amount={amount}");

        bool anyUpdated = false;

        foreach (var m in activeMissions)
        {
            if (m.type == type && !m.isCompleted)
            {
                Debug.Log($"Updating mission: {m.description}, Current: {m.current}, Goal: {m.goal}");

                int oldCurrent = m.current;

                switch (type)
                {
                    case MissionType.CollectInOneRun:
                    case MissionType.TimeInOneRun:
                        if (amount > m.current)
                            m.current = amount;
                        break;
                    case MissionType.CollectStreak:
                    case MissionType.TimeStreak:
                        m.current = amount > 0 ? m.current + 1 : 0;
                        break;
                    default:
                        m.current += amount;
                        break;
                }
                if (m.current >= m.goal)
                {
                    m.current = m.goal;
                    m.isCompleted = true;
                    Debug.Log("Mission abgeschlossen: " + m.description);

                    if (!RewardAlreadyCollected(m.id))
                    {
                        ToastManager.Show($"Mission complete: {m.description}", ToastType.Success);

                        if (weeklyMissionRewardScript != null)
                        {
                            Debug.Log($"ShowReward wird aufgerufen für Mission: {m.description}");
                            weeklyMissionRewardScript.ShowReward(m.description, m.id);
                        }
                    }
                }

                if (oldCurrent != m.current)
                {
                    anyUpdated = true;
                    Debug.Log($"Mission updated: {m.description} - {oldCurrent} -> {m.current}");
                }
            }
        }

        if (anyUpdated)
        {
            SaveMissionsToPrefs();
            // UI wird jetzt über Event aktualisiert
        }
        else
        {
            Debug.Log($"Keine Mission für Type {type} gefunden oder alle bereits abgeschlossen");
        }
    }

    public void CheckCompletedMissions()
    {
        Debug.Log("🔍 CheckCompletedMissions aufgerufen");

        if (activeMissions == null || activeMissions.Count == 0)
        {
            Debug.LogWarning("Keine aktiven Missionen gefunden.");
            return;
        }

        foreach (var mission in activeMissions)
        {
            Debug.Log($"Mission: {mission.description}, Current: {mission.current}, Goal: {mission.goal}, isCompleted: {mission.isCompleted}");

            if (mission.isCompleted && !RewardAlreadyCollected(mission.id))
            {
                Debug.Log("Mission abgeschlossen erkannt (Belohnung noch nicht eingesammelt): " + mission.description);

                if (weeklyMissionRewardScript != null)
                {
                    Debug.Log("Zeige Belohnungspanel für abgeschlossene Mission: " + mission.description);
                    weeklyMissionRewardScript.ShowReward(mission.description, mission.id);
                }
                else
                {
                    Debug.LogWarning("weeklyMissionRewardScript ist null!");
                }
            }
        }
    }

    public bool RewardAlreadyCollected(string missionId)
    {
        return PlayerPrefs.GetInt($"MissionRewardCollected_{missionId}", 0) == 1;
    }

    public void MarkRewardCollected(string missionId)
    {
        CloudSaveManager.Instance.SaveInt($"MissionRewardCollected_{missionId}", 1);
        Debug.Log($"Belohnung für Mission {missionId} als eingesammelt markiert.");
    }

    private void ClearAllRewardCollectedFlags()
    {
        foreach (var mission in activeMissions)
        {
            PlayerPrefs.DeleteKey($"MissionRewardCollected_{mission.id}");
        }
        PlayerPrefs.Save();
        // Hinweis: Gelöschte Keys werden nicht aktiv aus der Cloud entfernt,
        // LoadAllAsync überschreibt sie beim nächsten Login neu.
    }

    [System.Serializable]
    private class MissionWrapper
    {
        public List<Mission> missions;
    }

    public void NotifyMissionsLoaded()
    {
        OnMissionsLoaded?.Invoke();
    }

    public void ReloadMissions()
    {
        LoadMissions();
    }

    /// <summary>
    /// Wird von WeeklyMissionRewardScript aufgerufen, wenn Belohnung eingesammelt wurde.
    /// </summary>
    /// <param name="missionId"></param>
    public void OnRewardCollected(string missionId)
    {
        MarkRewardCollected(missionId);
    }
}
