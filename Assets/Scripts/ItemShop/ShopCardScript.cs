using System.Collections;
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
            costText.text = itemData.alwaysOwned ? "Gratis" : itemData.cost.ToString();

        buyButton.onClick.AddListener(OnBuyClicked);
        activateButton.onClick.AddListener(OnActivateClicked);

        UpdateState();
    }

    void OnBuyClicked()
    {
        if (data.alwaysOwned) return;
        int stash = PlayerPrefs.GetInt("CannabisStash", 0);
        if (stash < data.cost || PlayerPrefs.GetInt(data.ownedKey, 0) == 1) return;

        PlayerPrefs.SetInt(data.ownedKey, 1);
        PlayerPrefs.SetInt("CannabisStash", stash - data.cost);
        PlayerPrefs.Save();
        onChanged?.Invoke();
    }

    void OnActivateClicked()
    {
        bool owned = data.alwaysOwned || PlayerPrefs.GetInt(data.ownedKey, 0) == 1;
        if (!owned) return;

        PlayerPrefs.SetString(data.activePrefsKey, data.activeValue);
        PlayerPrefs.Save();
        onChanged?.Invoke();
    }

    public void UpdateState()
    {
        bool owned  = data.alwaysOwned || PlayerPrefs.GetInt(data.ownedKey, 0) == 1;
        bool active = PlayerPrefs.GetString(data.activePrefsKey) == data.activeValue;
        int  stash  = PlayerPrefs.GetInt("CannabisStash", 0);

        // Buy-Button: nur sichtbar wenn nicht besessen
        buyButton.gameObject.SetActive(!owned);
        if (!owned)
        {
            bool canAfford = stash >= data.cost;
            var buyText = buyButton.GetComponentInChildren<TMP_Text>();
            if (buyText != null)
                buyText.color = canAfford ? Color.black : new Color(0.85f, 0.15f, 0.15f);

            // Button-Hintergrund direkt setzen
            buyButton.image.color = canAfford ? Color.white : new Color(1f, 0.85f, 0.85f);
        }

        // Activate-Button: nur sichtbar wenn besessen
        activateButton.gameObject.SetActive(owned);
        var activateText = activateButton.GetComponentInChildren<TMP_Text>();
        if (activateText != null)
            activateText.text = active ? "Aktiv" : "Aktivieren";

        // Farbe direkt auf Image setzen — ColorBlock aktualisiert sich sonst nur bei State-Transitions
        Color activateBtnColor = active ? new Color(0.13f, 0.77f, 0.37f) : new Color(0.80f, 0.80f, 0.80f);
        activateButton.image.color = activateBtnColor;
    }
}
