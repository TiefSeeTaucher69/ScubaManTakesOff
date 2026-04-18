using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WeeklyMissionUI : MonoBehaviour
{
    [System.Serializable]
    public class MissionUIEntry
    {
        public TMPro.TMP_Text descriptionText;
        public Slider progressBar;
        public TMPro.TMP_Text progressText;
        public Image completedIcon;
    }

    public List<MissionUIEntry> missionUIEntries;

    private WeeklyMissionManager missionManager;

    private void Start()
    {
       
    }
    void OnEnable()
    {
        missionManager = WeeklyMissionManager.Instance;

        if (missionManager != null)
        {
            missionManager.OnMissionsLoaded += UpdateUI;
            UpdateUI();
        }
        else
        {
            Debug.LogWarning("WeeklyMissionManager.Instance ist null in WeeklyMissionUI.OnEnable");
        }
    }

    void OnDisable()
    {
        if (missionManager != null)
        {
            missionManager.OnMissionsLoaded -= UpdateUI;
        }
    }

    public void UpdateUI()
    {
        Debug.Log("WeeklyMissionUI.UpdateUI aufgerufen");

        if (!gameObject.activeInHierarchy)
            return;

        if (missionManager == null)
        {
            missionManager = WeeklyMissionManager.Instance;
            if (missionManager == null)
            {
                Debug.LogError("missionManager ist null in WeeklyMissionUI.UpdateUI");
                return;
            }
        }

        if (missionManager.activeMissions == null)
        {
            Debug.LogError("activeMissions ist null in WeeklyMissionUI.UpdateUI");
            return;
        }

        Debug.Log($"Anzahl Missionen: {missionManager.activeMissions.Count}");
        Debug.Log($"Anzahl UI-Einträge: {missionUIEntries.Count}");

        for (int i = 0; i < missionUIEntries.Count; i++)
        {
            var uiEntry = missionUIEntries[i];
            if (uiEntry == null)
            {
                Debug.LogWarning($"MissionUIEntry[{i}] ist null");
                continue;
            }

            if (i < missionManager.activeMissions.Count)
            {
                var mission = missionManager.activeMissions[i];
                Debug.Log($"Mission {i}: {mission.description} – Fortschritt: {mission.current}/{mission.goal}");

                if (uiEntry.descriptionText != null)
                {
                    uiEntry.descriptionText.text = mission.description;
                    uiEntry.descriptionText.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning($"descriptionText bei UI-Eintrag {i} ist null");
                }

                if (uiEntry.progressBar != null)
                {
                    uiEntry.progressBar.maxValue = mission.goal;
                    uiEntry.progressBar.value = mission.current;
                    uiEntry.progressBar.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning($"progressBar bei UI-Eintrag {i} ist null");
                }

                if (mission.isCompleted)
                {
                    // ✅ Mission abgeschlossen: Fortschrittsanzeige ausblenden, Haken anzeigen
                    if (uiEntry.progressText != null)
                        uiEntry.progressText.gameObject.SetActive(false);

                    if (uiEntry.completedIcon != null)
                        uiEntry.completedIcon.gameObject.SetActive(true);

                    if (uiEntry.progressBar != null)
                        uiEntry.progressBar.value = mission.goal; // optional: vollständig gefüllt
                }
                else
                {
                    // 🔄 Mission noch nicht fertig: Fortschrittsanzeige zeigen, Haken ausblenden
                    if (uiEntry.progressText != null)
                    {
                        uiEntry.progressText.text = $"{mission.current} / {mission.goal}";
                        uiEntry.progressText.gameObject.SetActive(true);
                    }

                    if (uiEntry.completedIcon != null)
                        uiEntry.completedIcon.gameObject.SetActive(false);
                }

            }
            else
            {
                Debug.Log($"Deaktiviere UI-Eintrag {i}, da keine Mission vorhanden");

                if (uiEntry.progressBar != null)
                    uiEntry.progressBar.gameObject.SetActive(false);

                if (uiEntry.descriptionText != null)
                    uiEntry.descriptionText.gameObject.SetActive(false);

                if (uiEntry.progressText != null)
                    uiEntry.progressText.gameObject.SetActive(false);
            }
        }
    }

}
