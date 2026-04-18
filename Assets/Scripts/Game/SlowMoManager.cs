using UnityEngine;
using System.Collections;

public class SlowMoManager : MonoBehaviour
{
    public float slowMoDuration = 5f;
    public float cooldownTime = 10f;
    public float slowMoMultiplier = 0.4f;
    public TMPro.TMP_Text cooldownText;
    public GameObject slowMoUI;

    private bool isSlowMoActive = false;
    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;
    private SteffScript steff;

    void Start()
    {
        steff = FindFirstObjectByType<SteffScript>();

        if (RemoteConfigManager.Instance != null)
        {
            slowMoDuration   = RemoteConfigManager.Instance.SlowMoDuration;
            cooldownTime     = RemoteConfigManager.Instance.SlowMoCooldown;
            slowMoMultiplier = RemoteConfigManager.Instance.SlowMoSpeedMultiplier;
        }

        // Migration: players who owned the old 0/1 flag get 1 free stack
        if (PlayerPrefs.GetInt("HasSlowMoItem", 0) == 1 && PlayerPrefs.GetInt("ItemCount_SlowMo", 0) == 0)
            CloudSaveManager.Instance.SaveInt("ItemCount_SlowMo", 1);

        bool isRankedItem = RankedManager.IsRanked && RankedManager.WeeklyItem == "SlowMo";
        bool isEquipped   = !RankedManager.IsRanked
                            && PlayerPrefs.GetInt("ItemCount_SlowMo", 0) > 0
                            && PlayerPrefs.GetString("ActiveItem", "") == "SlowMo";

        slowMoUI.SetActive(isEquipped || isRankedItem);

        // Consume 1 stack at run start (Ranked never consumes)
        if (isEquipped)
        {
            int count = PlayerPrefs.GetInt("ItemCount_SlowMo", 0);
            CloudSaveManager.Instance.SaveInt("ItemCount_SlowMo", Mathf.Max(0, count - 1));
        }
    }

    void Update()
    {
        bool isRankedItem = RankedManager.IsRanked && RankedManager.WeeklyItem == "SlowMo";
        bool isActiveItem = !RankedManager.IsRanked && PlayerPrefs.GetString("ActiveItem", "") == "SlowMo";
        if (!isRankedItem && !isActiveItem) return;

        if (!slowMoUI.activeSelf) return;

        HandleCooldownUI();

        if (steff != null && !steff.steffIsAlive) return;
        if (steff != null && steff.IsPaused()) return;

        if ((Input.GetKeyDown(KeyCode.E)              && !isSlowMoActive && !isOnCooldown) ||
            (Input.GetKeyDown(KeyCode.Mouse0)          && !isSlowMoActive && !isOnCooldown) ||
            (Input.GetKeyDown(KeyCode.JoystickButton3) && !isSlowMoActive && !isOnCooldown))
        {
            StartCoroutine(ActivateSlowMo());
        }

        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isOnCooldown = false;
                cooldownTimer = 0f;
            }
        }
    }

    IEnumerator ActivateSlowMo()
    {
        isSlowMoActive = true;
        SpeedManager.SlowMoMultiplier = slowMoMultiplier;
        SpeedManagerCannabisScript.SlowMoMultiplier = slowMoMultiplier;

        yield return new WaitForSeconds(slowMoDuration);

        SpeedManager.SlowMoMultiplier = 1f;
        SpeedManagerCannabisScript.SlowMoMultiplier = 1f;
        isSlowMoActive = false;

        isOnCooldown = true;
        cooldownTimer = cooldownTime;
    }

    void HandleCooldownUI()
    {
        if (isSlowMoActive)
            cooldownText.text = "Slow-Mo Active!";
        else if (isOnCooldown)
            cooldownText.text = $"Ready in: {cooldownTimer:F1}s";
        else
            cooldownText.text = "Ready!";
    }
}
