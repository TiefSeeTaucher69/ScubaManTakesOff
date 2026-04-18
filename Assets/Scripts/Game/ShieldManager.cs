using UnityEngine;

public class ShieldManager : MonoBehaviour
{
    public float shieldDuration = 8f;
    public float cooldownTime = 12f;
    public TMPro.TMP_Text cooldownText;
    public GameObject shieldUI;
    public GameObject shieldVisual;

    public static bool IsShieldActive { get; private set; } = false;

    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;
    private float shieldTimeRemaining = 0f;
    private SteffScript steff;

    void Start()
    {
        steff = FindFirstObjectByType<SteffScript>();

        if (RemoteConfigManager.Instance != null)
        {
            shieldDuration = RemoteConfigManager.Instance.ShieldDuration;
            cooldownTime   = RemoteConfigManager.Instance.ShieldCooldown;
        }

        // Migration: players who owned the old 0/1 flag get 1 free stack
        if (PlayerPrefs.GetInt("HasShieldItem", 0) == 1 && PlayerPrefs.GetInt("ItemCount_Shield", 0) == 0)
            CloudSaveManager.Instance.SaveInt("ItemCount_Shield", 1);

        bool isRankedItem = RankedManager.IsRanked && RankedManager.WeeklyItem == "Shield";
        bool isEquipped   = !RankedManager.IsRanked
                            && PlayerPrefs.GetInt("ItemCount_Shield", 0) > 0
                            && PlayerPrefs.GetString("ActiveItem", "") == "Shield";

        shieldUI.SetActive(isEquipped || isRankedItem);

        if (shieldVisual != null) shieldVisual.SetActive(false);
        IsShieldActive = false;

        // Consume 1 stack at run start (Ranked never consumes)
        if (isEquipped)
        {
            int count = PlayerPrefs.GetInt("ItemCount_Shield", 0);
            CloudSaveManager.Instance.SaveInt("ItemCount_Shield", Mathf.Max(0, count - 1));
        }
    }

    void Update()
    {
        bool isRankedItem = RankedManager.IsRanked && RankedManager.WeeklyItem == "Shield";
        bool isActiveItem = !RankedManager.IsRanked && PlayerPrefs.GetString("ActiveItem", "") == "Shield";
        if (!isRankedItem && !isActiveItem) return;

        if (!shieldUI.activeSelf) return;

        HandleCooldownUI();

        if (steff != null && !steff.steffIsAlive) return;
        if (steff != null && steff.IsPaused()) return;

        if (IsShieldActive)
        {
            shieldTimeRemaining -= Time.deltaTime;
            if (shieldTimeRemaining <= 0f)
            {
                DeactivateShield();
                isOnCooldown = true;
                cooldownTimer = cooldownTime;
            }
        }

        if ((Input.GetKeyDown(KeyCode.E)              && !IsShieldActive && !isOnCooldown) ||
            (Input.GetKeyDown(KeyCode.Mouse0)          && !IsShieldActive && !isOnCooldown) ||
            (Input.GetKeyDown(KeyCode.JoystickButton3) && !IsShieldActive && !isOnCooldown))
        {
            ActivateShield();
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

    void ActivateShield()
    {
        IsShieldActive = true;
        shieldTimeRemaining = shieldDuration;
        if (shieldVisual != null) shieldVisual.SetActive(true);
    }

    public void AbsorbHit(GameObject hitObject)
    {
        PipeBreaker.Break(hitObject);
        DeactivateShield();
        isOnCooldown = true;
        cooldownTimer = cooldownTime;
    }

    void DeactivateShield()
    {
        IsShieldActive = false;
        shieldTimeRemaining = 0f;
        if (shieldVisual != null) shieldVisual.SetActive(false);
    }

    void HandleCooldownUI()
    {
        if (IsShieldActive)
            cooldownText.text = $"Shield Active! {shieldTimeRemaining:F1}s";
        else if (isOnCooldown)
            cooldownText.text = $"Ready in: {cooldownTimer:F1}s";
        else
            cooldownText.text = "Ready!";
    }

    void OnDestroy()
    {
        IsShieldActive = false;
    }
}
