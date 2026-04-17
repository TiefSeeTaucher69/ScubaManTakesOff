using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemShopHandler : MonoBehaviour
{
    // ── Datenklasse für ein Shop-Item ──────────────────────────────────────

    [System.Serializable]
    public class ShopItemData
    {
        public string  itemName;
        public Sprite  icon;
        public int     cost;
        public bool    alwaysOwned;   // z.B. Benjo-Bird (Gratis-Skin)
        public string  ownedKey;      // PlayerPrefs-Key "HasXxx"
        public string  activeValue;   // Wert der in activePrefsKey gespeichert wird
        public string  activePrefsKey;// "ActiveItem" / "ActiveTrail" / "ActiveSkin"
        public bool    isStackable;   // true für Items (kaufbar als Vorrat)
        public string  countKey;      // "ItemCount_Invincible" etc. (nur für stackable)
    }

    // ── Item-Listen (im Inspector befüllen) ────────────────────────────────

    [Header("Items")]
    public List<ShopItemData> items;
    public Transform          itemsContent;   // ScrollView > Viewport > Content

    [Header("Trails")]
    public List<ShopItemData> trails;
    public Transform          trailsContent;

    [Header("Skins")]
    public List<ShopItemData> skins;
    public Transform          skinsContent;

    // ── Prefab & UI ────────────────────────────────────────────────────────

    [Header("Prefab & Stash-Anzeige")]
    public GameObject shopCardPrefab;
    public TMP_Text   cannabisStashText;

    // ── Intern: alle erzeugten Karten ─────────────────────────────────────

    private readonly List<ShopCardScript> allCards = new();

    // ──────────────────────────────────────────────────────────────────────

    void Start()
    {
        if (!PlayerPrefs.HasKey("ActiveSkin"))
            PlayerPrefs.SetString("ActiveSkin", "benjo-bird");

        GenerateCards(items,  itemsContent,  "ActiveItem");
        GenerateCards(trails, trailsContent, "ActiveTrail");
        GenerateCards(skins,  skinsContent,  "ActiveSkin");
        UpdateStashDisplay();
    }

    void GenerateCards(List<ShopItemData> dataList, Transform content, string activePrefsKey)
    {
        if (content == null || shopCardPrefab == null) return;

        foreach (var data in dataList)
        {
            // activePrefsKey aus der Liste übernehmen falls nicht gesetzt
            if (string.IsNullOrEmpty(data.activePrefsKey))
                data.activePrefsKey = activePrefsKey;

            GameObject go   = Instantiate(shopCardPrefab, content);
            var        card = go.GetComponent<ShopCardScript>();

            if (card != null)
            {
                card.Setup(data, RefreshAll);
                allCards.Add(card);
            }
        }
    }

    void RefreshAll()
    {
        foreach (var card in allCards)
            card.UpdateState();

        UpdateStashDisplay();
    }

    void UpdateStashDisplay()
    {
        if (cannabisStashText != null)
            cannabisStashText.text = PlayerPrefs.GetInt("CannabisStash", 0).ToString();
    }
}
