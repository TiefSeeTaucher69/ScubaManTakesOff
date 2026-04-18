using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class WeeklyMissionRewardScript : MonoBehaviour
{
    private struct PendingReward { public string name; public string id; }

    public GameObject panel;
    public TMPro.TMP_Text statusText;
    public Button collectButton;
    public Image rewardImage;
    public AudioSource audioSource;
    public Sprite rewardSprite;
    public TMPro.TMP_Text cannabisStashText;

    public int cannabisReward = 15;

    private string currentMissionId;
    private readonly Queue<PendingReward> _rewardQueue = new Queue<PendingReward>();
    private bool _showing = false;

    void Awake()
    {
        if (RemoteConfigManager.Instance != null)
            cannabisReward = RemoteConfigManager.Instance.MissionRewardCannabis;

        panel.SetActive(false);
        collectButton.onClick.AddListener(OnCollectClicked);
        rewardImage.sprite = rewardSprite;
    }

    public void ShowReward(string missionName, string missionId)
    {
        Debug.Log("🌿 Zeige Weekly Mission Belohnung: " + missionName);

        if (panel == null)
        {
            Debug.LogError("Panel ist NULL!");
            return;
        }

        _rewardQueue.Enqueue(new PendingReward { name = missionName, id = missionId });
        if (!_showing)
            ShowNext();
    }

    private void ShowNext()
    {
        if (_rewardQueue.Count == 0)
        {
            _showing = false;
            return;
        }

        _showing = true;
        PendingReward reward = _rewardQueue.Dequeue();
        currentMissionId = reward.id;

        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = panel.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        statusText.text = $"Mission complete:\n<b>{reward.name}</b>";
        collectButton.interactable = true;
        panel.SetActive(true);

        StartCoroutine(FadeInPanel());
    }

    void OnCollectClicked()
    {
        Debug.Log("🌿 Weekly Reward eingesammelt");

        collectButton.interactable = false;
        panel.SetActive(false);

        if (audioSource != null)
            audioSource.Play();

        int stash = PlayerPrefs.GetInt("CannabisStash", 0) + cannabisReward;
        CloudSaveManager.Instance.SaveInt("CannabisStash", stash);

        if (cannabisStashText != null)
            cannabisStashText.text = stash.ToString();

        if (!string.IsNullOrEmpty(currentMissionId))
        {
            WeeklyMissionManager.Instance.OnRewardCollected(currentMissionId);
            currentMissionId = null;
        }

        ToastManager.Show($"Mission complete! +{cannabisReward} Cannabis", ToastType.Reward);

        ShowNext();
    }

    IEnumerator FadeInPanel()
    {
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            Debug.LogError("CanvasGroup fehlt im FadeInPanel!");
            yield break;
        }

        float duration = 0.5f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0, 1, t / duration);
            yield return null;
        }
        cg.alpha = 1f;
        Debug.Log("FadeInPanel beendet, Alpha = 1");
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
    }
}
