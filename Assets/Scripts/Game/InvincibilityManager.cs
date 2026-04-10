using UnityEngine;
using UnityEngine.UI; // F�r TextMeshPro: using TMPro;
using System.Collections;

public class InvincibilityManager : MonoBehaviour
{
    public Collider2D playerCollider;
    public SpriteRenderer spriteRenderer;
    public TMPro.TMP_Text cooldownText; // Oder TMP_Text f�r TextMeshPro

    public float invincibilityDuration = 2f;
    public float cooldownTime = 10f;
    public GameObject invincibilityUI;

    private bool isInvincible = false;
    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;
    private SteffScript steff;

    private void Start()
    {
        steff = FindObjectOfType<SteffScript>();

        bool isRankedItem = RankedManager.IsRanked && RankedManager.WeeklyItem == "Invincible";
        bool isEquipped   = !RankedManager.IsRanked && PlayerPrefs.GetInt("HasInvincibleItem", 0) == 1 && PlayerPrefs.GetString("ActiveItem", "") == "Invincible";
        invincibilityUI.SetActive(isEquipped || isRankedItem);
    }

    void Update()
    {
        bool isRankedItem = RankedManager.IsRanked && RankedManager.WeeklyItem == "Invincible";
        bool isActiveItem = !RankedManager.IsRanked && PlayerPrefs.GetString("ActiveItem", "") == "Invincible";
        if (!isRankedItem && !isActiveItem) return;
        HandleCooldownUI();

        if (steff != null && !steff.steffIsAlive) return;
        if (steff != null && steff.IsPaused()) return;

        if ((Input.GetKeyDown(KeyCode.E) && !isInvincible && !isOnCooldown) || (Input.GetKeyDown(KeyCode.Mouse0) && !isInvincible && !isOnCooldown) || (Input.GetKeyDown(KeyCode.JoystickButton3) && !isInvincible && !isOnCooldown))
        {
            bool hasItem = PlayerPrefs.GetInt("HasInvincibleItem", 0) == 1 || isRankedItem;
            if (hasItem)
            {
                StartCoroutine(InvincibilityCoroutine());
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

    void HandleCooldownUI()
    {
        if (isInvincible)
        {
            cooldownText.text = "Unverwundbar!";
        }
        else if (isOnCooldown)
        {
            cooldownText.text = $"Bereit in: {cooldownTimer:F1}s";
        }
        else
        {
            cooldownText.text = "Bereit!";
        }
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        playerCollider.enabled = false;

        float elapsed = 0f;
        float blinkInterval = 0.4f;

        while (elapsed < invincibilityDuration)
        {
            // Blinken durch Alpha
            Color color = spriteRenderer.color;
            color.a = (color.a == 1f) ? 0.3f : 1f;
            spriteRenderer.color = color;

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;

            blinkInterval = Mathf.Max(0.05f, blinkInterval * 0.8f);
        }

        // Zur�cksetzen
        Color resetColor = spriteRenderer.color;
        resetColor.a = 1f;
        spriteRenderer.color = resetColor;

        playerCollider.enabled = true;
        isInvincible = false;

        // Jetzt startet der Cooldown!
        isOnCooldown = true;
        cooldownTimer = cooldownTime;
    }
}
