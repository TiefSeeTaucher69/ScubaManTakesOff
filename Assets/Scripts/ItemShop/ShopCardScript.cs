using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Wird auf jede generierte Shop-Karte gesetzt.
/// Benötigt: Img_Icon, Txt_ItemName, Txt_Cost, Btn_Buy, Btn_Activate (mit je einem TMP-Text-Child)
/// </summary>
public class ShopCardScript : MonoBehaviour
{
    [Header("UI-Referenzen (werden per GetComponent gefunden)")]
    public Image    icon;
    public TMP_Text itemNameText;
    public TMP_Text costText;
    public Button   buyButton;
    public Button   activateButton;

    private ItemShopHandler.ShopItemData data;
    private System.Action onChanged;

    void Start()
    {
        transform.localScale = Vector3.one * 0.8f;
        StartCoroutine(PopIn());
    }

    private IEnumerator PopIn(float duration = 0.15f)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.one * Mathf.Lerp(0.8f, 1.0f, elapsed / duration);
            yield return null;
        }
        transform.localScale = Vector3.one;
    }

    public void Setup(ItemShopHandler.ShopItemData itemData, System.Action onChangedCallback)
    {
        data      = itemData;
        onChanged = onChangedCallback;

        if (icon != null)
            icon.sprite = itemData.icon;

        if (itemNameText != null)
            itemNameText.text = itemData.itemName;

        if (costText != null)
            costText.text = itemData.alwaysOwned ? "Free" : itemData.cost.ToString();

        buyButton.onClick.AddListener(OnBuyClicked);
        activateButton.onClick.AddListener(OnActivateClicked);

        UpdateState();
    }

    void OnBuyClicked()
    {
        if (data.alwaysOwned) return;

        // Stackable items: can be bought repeatedly, each purchase adds 1 run-use
        if (data.isStackable)
        {
            int stash = PlayerPrefs.GetInt("CannabisStash", 0);
            if (stash < data.cost)
            {
                ToastManager.Show("Not enough Cannabis!", ToastType.Warning);
                return;
            }
            int newStash = stash - data.cost;
            int newCount = PlayerPrefs.GetInt(data.countKey, 0) + 1;
            CloudSaveManager.Instance.SaveBatch(new Dictionary<string, object>
            {
                { data.countKey,   newCount },
                { "CannabisStash", newStash }
            });
            ToastManager.Show($"{data.itemName} purchased! (×{newCount})", ToastType.Success);
            onChanged?.Invoke();
            return;
        }

        // Non-stackable: original one-time purchase logic
        if (PlayerPrefs.GetInt(data.ownedKey, 0) == 1) return;

        int currentStash = PlayerPrefs.GetInt("CannabisStash", 0);
        if (currentStash < data.cost)
        {
            ToastManager.Show("Not enough Cannabis!", ToastType.Warning);
            return;
        }

        int updatedStash = currentStash - data.cost;
        CloudSaveManager.Instance.SaveBatch(new Dictionary<string, object>
        {
            { data.ownedKey,    1            },
            { "CannabisStash",  updatedStash }
        });
        ToastManager.Show($"{data.itemName} purchased!", ToastType.Success);
        onChanged?.Invoke();
    }

    void OnActivateClicked()
    {
        // Items are equipped via the Loadout tab, not the Shop
        if (data.isStackable) return;

        bool owned = data.alwaysOwned || PlayerPrefs.GetInt(data.ownedKey, 0) == 1;
        if (!owned) return;

        bool alreadyActive = PlayerPrefs.GetString(data.activePrefsKey) == data.activeValue;
        CloudSaveManager.Instance.SaveString(data.activePrefsKey, data.activeValue);
        if (!alreadyActive)
            ToastManager.Show($"{data.itemName} activated!", ToastType.Info);
        onChanged?.Invoke();
    }

    public void UpdateState()
    {
        // Stackable items: always show buy button, show stack count, hide activate button
        if (data.isStackable)
        {
            int count = PlayerPrefs.GetInt(data.countKey, 0);
            int stash = PlayerPrefs.GetInt("CannabisStash", 0);

            buyButton.gameObject.SetActive(true);
            bool canAfford = stash >= data.cost;
            var buyText = buyButton.GetComponentInChildren<TMP_Text>();
            if (buyText != null)
                buyText.color = canAfford ? Color.black : new Color(0.85f, 0.15f, 0.15f);
            buyButton.image.color = canAfford ? Color.white : new Color(1f, 0.85f, 0.85f);

            activateButton.gameObject.SetActive(false);

            if (costText != null)
                costText.text = count > 0 ? $"×{count}" : data.cost.ToString();
            return;
        }

        // Non-stackable: buy until owned, then show "Bought" (equipping happens in Loadout)
        bool owned        = data.alwaysOwned || PlayerPrefs.GetInt(data.ownedKey, 0) == 1;
        int  currentStash = PlayerPrefs.GetInt("CannabisStash", 0);

        buyButton.gameObject.SetActive(!owned);
        if (!owned)
        {
            bool canAfford = currentStash >= data.cost;
            var buyText = buyButton.GetComponentInChildren<TMP_Text>();
            if (buyText != null)
                buyText.color = canAfford ? Color.black : new Color(0.85f, 0.15f, 0.15f);
            buyButton.image.color = canAfford ? Color.white : new Color(1f, 0.85f, 0.85f);
        }

        activateButton.gameObject.SetActive(owned);
        activateButton.interactable = false;
        var activateText = activateButton.GetComponentInChildren<TMP_Text>();
        if (activateText != null)
        {
            activateText.text  = "Bought";
            activateText.color = Color.green;
        }
        activateButton.image.color = new Color(0.45f, 0.45f, 0.45f);
    }
}
