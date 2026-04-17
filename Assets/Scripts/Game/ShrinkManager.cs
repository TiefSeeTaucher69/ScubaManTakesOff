using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShrinkManager : MonoBehaviour
{
    public float shrinkDuration = 5f;
    public float cooldownTime = 10f;
    public Vector3 shrinkScale = new Vector3(0.5f, 0.5f, 1f);
    public TMPro.TMP_Text cooldownText;
    public GameObject shrinkUI;

    private Vector3 originalScale;
    private bool isShrunk = false;
    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;
    private SteffScript steff;

    void Start()
    {
        originalScale = transform.localScale;
        steff = FindObjectOfType<SteffScript>();

        if (RemoteConfigManager.Instance != null)
        {
            shrinkDuration = RemoteConfigManager.Instance.ShrinkDuration;
            cooldownTime   = RemoteConfigManager.Instance.ShrinkCooldown;
            float s        = RemoteConfigManager.Instance.ShrinkScale;
            shrinkScale    = new Vector3(s, s, 1f);
        }

        // Migration: players who owned the old 0/1 flag get 1 free stack
        if (PlayerPrefs.GetInt("HasShrinkItem", 0) == 1 && PlayerPrefs.GetInt("ItemCount_Shrink", 0) == 0)
            PlayerPrefs.SetInt("ItemCount_Shrink", 1);

        bool isRankedItem = RankedManager.IsRanked && RankedManager.WeeklyItem == "Shrink";
        bool isEquipped   = !RankedManager.IsRanked
                            && PlayerPrefs.GetInt("ItemCount_Shrink", 0) > 0
                            && PlayerPrefs.GetString("ActiveItem", "") == "Shrink";

        shrinkUI.SetActive(isEquipped || isRankedItem);

        // Consume 1 stack at run start (Ranked never consumes)
        if (isEquipped)
        {
            int count = PlayerPrefs.GetInt("ItemCount_Shrink", 0);
            CloudSaveManager.Instance.SaveInt("ItemCount_Shrink", Mathf.Max(0, count - 1));
        }
    }

    void Update()
    {
        bool isRankedItem = RankedManager.IsRanked && RankedManager.WeeklyItem == "Shrink";
        bool isActiveItem = !RankedManager.IsRanked && PlayerPrefs.GetString("ActiveItem", "") == "Shrink";
        if (!isRankedItem && !isActiveItem) return;

        if (!shrinkUI.activeSelf) return;

        HandleCooldownUI();

        if (steff != null && !steff.steffIsAlive) return;
        if (steff != null && steff.IsPaused()) return;

        if ((Input.GetKeyDown(KeyCode.E)              && !isShrunk && !isOnCooldown) ||
            (Input.GetKeyDown(KeyCode.Mouse0)          && !isShrunk && !isOnCooldown) ||
            (Input.GetKeyDown(KeyCode.JoystickButton3) && !isShrunk && !isOnCooldown))
        {
            StartCoroutine(ActivateShrink());
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

    IEnumerator ActivateShrink()
    {
        isShrunk = true;
        transform.localScale = shrinkScale;

        yield return new WaitForSeconds(shrinkDuration);

        transform.localScale = originalScale;
        isShrunk = false;

        isOnCooldown = true;
        cooldownTimer = cooldownTime;
    }

    void HandleCooldownUI()
    {
        if (isShrunk)
            cooldownText.text = "Shrunk!";
        else if (isOnCooldown)
            cooldownText.text = $"Ready in: {cooldownTimer:F1}s";
        else
            cooldownText.text = "Ready!";
    }
}
