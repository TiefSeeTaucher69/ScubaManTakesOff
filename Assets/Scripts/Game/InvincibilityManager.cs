using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InvincibilityManager : MonoBehaviour
{
    public Collider2D playerCollider;
    public SpriteRenderer spriteRenderer;
    public TMPro.TMP_Text cooldownText;

    public float invincibilityDuration = 2f;
    public float cooldownTime = 10f;
    public GameObject invincibilityUI;

    private bool isInvincible = false;
    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;
    private Coroutine _blinkCoroutine;
    private SteffScript steff;

    private void Start()
    {
        steff = FindFirstObjectByType<SteffScript>();

        if (RemoteConfigManager.Instance != null)
        {
            invincibilityDuration = RemoteConfigManager.Instance.InvincibilityDuration;
            cooldownTime          = RemoteConfigManager.Instance.InvincibilityCooldown;
        }

        // Migration: players who owned the old 0/1 flag get 1 free stack
        if (PlayerPrefs.GetInt("HasInvincibleItem", 0) == 1 && PlayerPrefs.GetInt("ItemCount_Invincible", 0) == 0)
            PlayerPrefs.SetInt("ItemCount_Invincible", 1);

        bool isRankedItem = RankedManager.IsRanked && RankedManager.WeeklyItem == "Invincible";
        bool isEquipped   = !RankedManager.IsRanked
                            && PlayerPrefs.GetInt("ItemCount_Invincible", 0) > 0
                            && PlayerPrefs.GetString("ActiveItem", "") == "Invincible";

        invincibilityUI.SetActive(isEquipped || isRankedItem);

        // Consume 1 stack at run start (Ranked never consumes)
        if (isEquipped)
        {
            int count = PlayerPrefs.GetInt("ItemCount_Invincible", 0);
            CloudSaveManager.Instance.SaveInt("ItemCount_Invincible", Mathf.Max(0, count - 1));
        }
    }

    void Update()
    {
        bool isRankedItem = RankedManager.IsRanked && RankedManager.WeeklyItem == "Invincible";
        bool isActiveItem = !RankedManager.IsRanked && PlayerPrefs.GetString("ActiveItem", "") == "Invincible";
        if (!isRankedItem && !isActiveItem) return;

        // UI was activated in Start() only if item was available; skip processing if UI is off
        if (!invincibilityUI.activeSelf) return;

        HandleCooldownUI();

        if (steff != null && !steff.steffIsAlive) return;
        if (steff != null && steff.IsPaused()) return;

        if ((Input.GetKeyDown(KeyCode.E)              && !isInvincible && !isOnCooldown) ||
            (Input.GetKeyDown(KeyCode.Mouse0)          && !isInvincible && !isOnCooldown) ||
            (Input.GetKeyDown(KeyCode.JoystickButton3) && !isInvincible && !isOnCooldown))
        {
            StartCoroutine(InvincibilityCoroutine());
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

    void HandleCooldownUI()
    {
        if (isInvincible)
            cooldownText.text = "Invincible!";
        else if (isOnCooldown)
            cooldownText.text = $"Ready in: {cooldownTimer:F1}s";
        else
            cooldownText.text = "Ready!";
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        playerCollider.enabled = false;

        _blinkCoroutine = StartCoroutine(BlinkEffect());
        yield return new WaitForSeconds(invincibilityDuration);

        if (_blinkCoroutine != null) { StopCoroutine(_blinkCoroutine); _blinkCoroutine = null; }

        Color resetColor = spriteRenderer.color;
        resetColor.a = 1f;
        spriteRenderer.color = resetColor;

        playerCollider.enabled = true;
        isInvincible = false;

        isOnCooldown = true;
        cooldownTimer = cooldownTime;
    }

    private IEnumerator BlinkEffect()
    {
        float elapsed = 0f;
        float blinkInterval = 0.4f;

        while (elapsed < invincibilityDuration)
        {
            Color color = spriteRenderer.color;
            color.a = (color.a == 1f) ? 0.3f : 1f;
            spriteRenderer.color = color;

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
            blinkInterval = Mathf.Max(0.05f, blinkInterval * 0.8f);
        }
    }
}
