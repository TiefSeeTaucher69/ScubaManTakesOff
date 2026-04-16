using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BiomeShopHandler : MonoBehaviour
{
    [Header("Biome-Items (im Inspector befüllen)")]
    public List<ItemShopHandler.ShopItemData> biomes = new();
    public Transform biomesContent;   // ScrollView > Viewport > Content

    [Header("Prefab & Stash-Anzeige")]
    public GameObject shopCardPrefab;
    public TMP_Text   cannabisStashText;

    private readonly List<ShopCardScript> allCards = new();

    void Start()
    {
        if (!PlayerPrefs.HasKey("ActiveBiome"))
            PlayerPrefs.SetString("ActiveBiome", "Mountain");
        if (PlayerPrefs.GetInt("HasBiomeMountain", 0) == 0)
            CloudSaveManager.Instance.SaveInt("HasBiomeMountain", 1);

        foreach (var data in biomes)
        {
            if (string.IsNullOrEmpty(data.activePrefsKey))
                data.activePrefsKey = "ActiveBiome";

            GameObject go   = Instantiate(shopCardPrefab, biomesContent);
            var        card = go.GetComponent<ShopCardScript>();
            if (card != null)
            {
                card.Setup(data, RefreshAll);
                allCards.Add(card);
            }
        }

        UpdateStashDisplay();
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
