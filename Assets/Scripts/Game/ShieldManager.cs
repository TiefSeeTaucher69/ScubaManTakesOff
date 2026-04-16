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
        steff = FindObjectOfType<SteffScript>();

        bool isRankedItem = RankedManager.IsRanked && RankedManager.WeeklyItem == "Shield";
        bool isEquipped   = !RankedManager.IsRanked && PlayerPrefs.GetInt("HasShieldItem", 0) == 1
                            && PlayerPrefs.GetString("ActiveItem", "") == "Shield";
        shieldUI.SetActive(isEquipped || isRankedItem);

        if (shieldVisual != null) shieldVisual.SetActive(false);
        IsShieldActive = false;
    }

    void Update()
    {
        bool isRankedItem = RankedManager.IsRanked && RankedManager.WeeklyItem == "Shield";
        bool isActiveItem = !RankedManager.IsRanked && PlayerPrefs.GetString("ActiveItem", "") == "Shield";
        if (!isRankedItem && !isActiveItem) return;
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
            bool hasItem = PlayerPrefs.GetInt("HasShieldItem", 0) == 1 || isRankedItem;
            if (hasItem)
            {
                ActivateShield();
            }
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

    // Called by SteffScript.OnCollisionEnter2D when the shield absorbs a pipe hit
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
