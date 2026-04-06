using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PetShopHandler : MonoBehaviour
{
    [System.Serializable]
    public class PetShopItemData
    {
        public string       petName;      // Anzeigename
        public Sprite       icon;         // Standby-Icon
        public List<Sprite> readyFrames;  // Sprites der Ready-Animation (für Hover)
        public int          cost = 100;
        public string       ownedKey;     // z.B. "HasPetBlackCat"
        public string       activeValue;  // z.B. "BlackCat" → wird in "ActivePet" gespeichert
        public GameObject   prefab;       // Prefab für PetManager (GameScene-Spawn)
    }

    [Header("Pets")]
    public List<PetShopItemData> pets;
    public Transform             petsContent;
    public GameObject            shopCardPrefab;
    public TMP_Text              cannabisStashText;

    private readonly List<ShopCardScript> _allCards = new();

    void Start()
    {
        UpdateStash();
        foreach (var data in pets)
        {
            var itemData = new ItemShopHandler.ShopItemData
            {
                itemName       = data.petName,
                icon           = data.icon,
                cost           = data.cost,
                ownedKey       = data.ownedKey,
                activeValue    = data.activeValue,
                activePrefsKey = "ActivePet"
            };
            var go   = Instantiate(shopCardPrefab, petsContent);
            var card = go.GetComponent<ShopCardScript>();
            if (card == null) continue;
            card.Setup(itemData, RefreshAll);
            _allCards.Add(card);

            var trigger = go.AddComponent<PetCardHoverTrigger>();
            trigger.Init(card.icon, data.readyFrames);
        }
    }

    void RefreshAll()
    {
        foreach (var c in _allCards) c.UpdateState();
        UpdateStash();
    }

    void UpdateStash()
    {
        if (cannabisStashText != null)
            cannabisStashText.text = PlayerPrefs.GetInt("CannabisStash", 0).ToString();
    }
}
