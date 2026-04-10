using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LaserManager : MonoBehaviour
{
    public GameObject laserPrefab;
    public Transform firePoint;
    public float cooldownTime = 5f;
    public TMPro.TMP_Text cooldownText; // Anzeige wie "Bereit!" oder "Bereit in: X.Xs"
    public GameObject laserUI; // UI-Element aktivieren bei Itembesitz

    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;
    private SteffScript steff;

    void Start()
    {
        steff = FindObjectOfType<SteffScript>();
        bool isRankedItem = RankedManager.IsRanked && RankedManager.WeeklyItem == "Laser";
        bool isEquipped   = !RankedManager.IsRanked && PlayerPrefs.GetInt("HasLaserItem", 0) == 1 && PlayerPrefs.GetString("ActiveItem", "") == "Laser";
        laserUI.SetActive(isEquipped || isRankedItem);
    }

    void Update()
    {
        bool isRankedItem = RankedManager.IsRanked && RankedManager.WeeklyItem == "Laser";
        bool isActiveItem = !RankedManager.IsRanked && PlayerPrefs.GetString("ActiveItem", "") == "Laser";
        if (!isRankedItem && !isActiveItem) return;

        HandleCooldownUI();

        if (steff != null && !steff.steffIsAlive) return;
        if (steff != null && steff.IsPaused()) return;

        if ((Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.JoystickButton3)) && !isOnCooldown)
        {
            bool hasItem = PlayerPrefs.GetInt("HasLaserItem", 0) == 1 || isRankedItem;
            if (hasItem)
            {
                FireLaser();
                StartCoroutine(StartCooldown());
            }
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
        Instantiate(laserPrefab, firePoint.position, firePoint.rotation);
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
        {
            cooldownText.text = $"Bereit in: {cooldownTimer:F1}s";
        }
        else
        {
            cooldownText.text = "Bereit!";
        }
    }
}
