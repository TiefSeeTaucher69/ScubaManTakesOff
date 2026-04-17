using UnityEngine;

[System.Serializable]
public class LoadoutItemData
{
    public string displayName;
    public Sprite icon;
    public string category;       // "Item", "Skin", "Trail", "Pet", "Biome"
    public string activeValue;    // stored in the activePrefsKey (e.g. "Invincible", "benjo-bird")
    public string activePrefsKey; // "ActiveItem", "ActiveSkin", "ActiveTrail", "ActivePet", "ActiveBiome"
    public string ownedKey;       // "HasSkin_ginger-bird" etc. (non-stackable items)
    public bool   alwaysOwned;    // true for defaults (benjo-bird, Mountain, no-item, no-trail, no-pet)
    public bool   isStackable;    // true for consumable items (Invincible, Shrink, etc.)
    public string countKey;       // "ItemCount_Invincible" etc. (only used when isStackable)
}
