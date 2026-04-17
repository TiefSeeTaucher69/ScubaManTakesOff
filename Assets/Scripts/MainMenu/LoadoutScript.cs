using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main controller for Pnl_Loadout in MainMenu.
/// Manages 5 equipment slots (Item, Skin, Trail, Pet, Biome) + a filtered inventory.
/// Inventory shows owned items for the selected slot category.
/// Supports click-to-equip and drag-and-drop from inventory to slot.
/// </summary>
public class LoadoutScript : MonoBehaviour
{
    [Header("Character Preview")]
    public Image characterPreviewImage;

    [Header("Equipment Slots")]
    public LoadoutSlotUI slotItem;
    public LoadoutSlotUI slotSkin;
    public LoadoutSlotUI slotTrail;
    public LoadoutSlotUI slotPet;
    public LoadoutSlotUI slotBiome;

    [Header("Inventory")]
    public Transform  inventoryContent;      // ScrollView > Viewport > Content
    public GameObject inventoryItemPrefab;   // Prefab with LoadoutInventoryItemUI

    [Header("All Loadout Options (configure in Inspector)")]
    public List<LoadoutItemData> allItems;

    [Header("Visuals")]
    public Sprite emptySlotIcon; // shown in slots + inventory cards when no icon is assigned

    private LoadoutSlotUI _selectedSlot;
    private readonly List<LoadoutSlotUI> _allSlots = new();

    // ── Unity lifecycle ───────────────────────────────────────────────────

    void Awake()
    {
        _allSlots.AddRange(new[] { slotItem, slotSkin, slotTrail, slotPet, slotBiome });
        foreach (var s in _allSlots)
            s.loadout = this;
    }

    void OnEnable()
    {
        AutoPopulateFromHandlers();
        RefreshSlots();
        UpdateCharacterPreview();
        // Default: open Item category so inventory is immediately useful
        SelectSlot(slotItem);
    }

    void AutoPopulateFromHandlers()
    {
        // Keep alwaysOwned sentinel entries (e.g. "No Pet", "Mountain"); strip stale data entries
        allItems.RemoveAll(d => (d.category == "Pet" || d.category == "Biome") && !d.alwaysOwned);

#pragma warning disable CS0618
        var petHandler = FindObjectOfType<PetShopHandler>(true);
#pragma warning restore CS0618
        if (petHandler != null)
        {
            foreach (var pet in petHandler.pets)
                allItems.Add(new LoadoutItemData
                {
                    displayName    = pet.petName,
                    icon           = pet.icon,
                    category       = "Pet",
                    activeValue    = pet.activeValue,
                    activePrefsKey = "ActivePet",
                    ownedKey       = pet.ownedKey
                });
        }

#pragma warning disable CS0618
        var biomeHandler = FindObjectOfType<BiomeShopHandler>(true);
#pragma warning restore CS0618
        if (biomeHandler != null)
        {
            foreach (var biome in biomeHandler.biomes)
            {
                if (biome.alwaysOwned) continue; // Mountain is already the alwaysOwned sentinel in allItems
                allItems.Add(new LoadoutItemData
                {
                    displayName    = biome.itemName,
                    icon           = biome.icon,
                    category       = "Biome",
                    activeValue    = biome.activeValue,
                    activePrefsKey = "ActiveBiome",
                    ownedKey       = biome.ownedKey
                });
            }
        }
    }

    // ── Public API (called by LoadoutSlotUI + LoadoutInventoryItemUI) ─────

    public void SelectSlot(LoadoutSlotUI slot)
    {
        _selectedSlot = slot;
        foreach (var s in _allSlots)
            s.SetSelected(s == slot);
        PopulateInventory(slot.category);
    }

    public void EquipItem(LoadoutItemData data)
    {
        CloudSaveManager.Instance.SaveString(data.activePrefsKey, data.activeValue);
        RefreshSlots();
        RefreshInventory();
        if (data.category == "Skin") UpdateCharacterPreview();
        ToastManager.Show($"{data.displayName} equipped!", ToastType.Info);
    }

    public void RefreshInventory()
    {
        if (_selectedSlot != null)
            PopulateInventory(_selectedSlot.category);
    }

    // ── Private helpers ───────────────────────────────────────────────────

    void PopulateInventory(string category)
    {
        foreach (Transform child in inventoryContent)
            Destroy(child.gameObject);

        foreach (var d in allItems)
        {
            if (d.category != category) continue;
            if (!IsOwned(d)) continue;

            var go   = Instantiate(inventoryItemPrefab, inventoryContent);
            var card = go.GetComponent<LoadoutInventoryItemUI>();
            if (card != null) card.Setup(d, this, emptySlotIcon);
        }
    }

    void RefreshSlots()
    {
        foreach (var slot in _allSlots)
        {
            string active = PlayerPrefs.GetString(slot.activePrefsKey, "");

            // Find matching data; fall back to first always-owned entry for the category
            var data = allItems.Find(d => d.category == slot.category && d.activeValue == active)
                    ?? allItems.Find(d => d.category == slot.category && d.alwaysOwned);
            if (data == null) continue;

            if (slot.iconImage != null) slot.iconImage.sprite = data.icon != null ? data.icon : emptySlotIcon;
            if (slot.nameText  != null) slot.nameText.text    = data.displayName;

            if (slot.stackCountText != null)
            {
                bool show = data.isStackable;
                slot.stackCountText.gameObject.SetActive(show);
                if (show)
                    slot.stackCountText.text = $"×{PlayerPrefs.GetInt(data.countKey, 0)}";
            }
        }
    }

    void UpdateCharacterPreview()
    {
        if (characterPreviewImage == null) return;
        string skinName = PlayerPrefs.GetString("ActiveSkin", "benjo-bird");
        var sprite = Resources.Load<Sprite>($"Skins/{skinName}");
        characterPreviewImage.sprite = sprite != null ? sprite : emptySlotIcon;
    }

    bool IsOwned(LoadoutItemData d)
    {
        if (d.alwaysOwned) return true;
        if (d.isStackable) return PlayerPrefs.GetInt(d.countKey, 0) > 0;
        return PlayerPrefs.GetInt(d.ownedKey, 0) == 1;
    }
}
