using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LaserManager : MonoBehaviour
{
    public GameObject laserPrefab;
    public Transform firePoint;
    public float cooldownTime = 5f;
    public TMPro.TMP_Text cooldownText;
    public GameObject laserUI;

    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;
    private SteffScript steff;

    void Start()
    {
        steff = FindFirstObjectByType<SteffScript>();

        if (RemoteConfigManager.Instance != null)
            cooldownTime = RemoteConfigManager.Instance.LaserCooldown;

        // Migration: players who owned the old 0/1 flag get 1 free stack
        if (PlayerPrefs.GetInt("HasLaserItem", 0) == 1 && PlayerPrefs.GetInt("ItemCount_Laser", 0) == 0)
            PlayerPrefs.SetInt("ItemCount_Laser", 1);

        bool isRankedItem = RankedManager.IsRanked && RankedManager.WeeklyItem == "Laser";
        bool isEquipped   = !RankedManager.IsRanked
                            && PlayerPrefs.GetInt("ItemCount_Laser", 0) > 0
                            && PlayerPrefs.GetString("ActiveItem", "") == "Laser";

        laserUI.SetActive(isEquipped || isRankedItem);

        // Consume 1 stack at run start (Ranked never consumes)
        if (isEquipped)
        {
            int count = PlayerPrefs.GetInt("ItemCount_Laser", 0);
            CloudSaveManager.Instance.SaveInt("ItemCount_Laser", Mathf.Max(0, count - 1));
        }
    }

    void Update()
    {
        bool isRankedItem = RankedManager.IsRanked && RankedManager.WeeklyItem == "Laser";
        bool isActiveItem = !RankedManager.IsRanked && PlayerPrefs.GetString("ActiveItem", "") == "Laser";
        if (!isRankedItem && !isActiveItem) return;

        if (!laserUI.activeSelf) return;

        HandleCooldownUI();

        if (steff != null && !steff.steffIsAlive) return;
        if (steff != null && steff.IsPaused()) return;

        if ((Input.GetKeyDown(KeyCode.E)              && !isOnCooldown) ||
            (Input.GetKeyDown(KeyCode.Mouse0)          && !isOnCooldown) ||
            (Input.GetKeyDown(KeyCode.JoystickButton3) && !isOnCooldown))
        {
            FireLaser();
            StartCoroutine(StartCooldown());
        }

        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                cooldownTimer = 0f;
                isOnCooldown = false;
            }
        }
    }

    void FireLaser()
    {
        Quaternion horizontalDir = DirectionFlipManager.IsFlipped
            ? Quaternion.Euler(0f, 180f, 0f)
            : Quaternion.identity;
        Instantiate(laserPrefab, firePoint.position, horizontalDir);
    }

    IEnumerator StartCooldown()
    {
        isOnCooldown = true;
        cooldownTimer = cooldownTime;
        yield return null;
    }

    void HandleCooldownUI()
    {
        if (isOnCooldown)
            cooldownText.text = $"Ready in: {cooldownTimer:F1}s";
        else
            cooldownText.text = "Ready!";
    }
}
