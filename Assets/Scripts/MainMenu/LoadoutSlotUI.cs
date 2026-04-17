using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Component on each equipment slot in the Loadout panel.
/// Accepts drag-and-drop from LoadoutInventoryItemUI and click-to-select.
/// </summary>
public class LoadoutSlotUI : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [Header("Slot Config (set in Inspector)")]
    public string category;       // "Item", "Skin", "Trail", "Pet", "Biome"
    public string activePrefsKey; // "ActiveItem", "ActiveSkin", etc.

    [Header("UI References")]
    public Image    iconImage;
    public TMP_Text nameText;
    public TMP_Text stackCountText; // optional, only relevant for Item slot
    public Image    slotBackground; // tinted green when selected

    [HideInInspector] public LoadoutScript loadout;

    private static readonly Color SelectedColor   = new Color(0.13f, 0.77f, 0.37f, 0.35f);
    private static readonly Color UnselectedColor = new Color(1f, 1f, 1f, 0.12f);

    void Awake()
    {
        SetSelected(false);
    }

    public void OnPointerClick(PointerEventData e)
    {
        loadout.SelectSlot(this);
    }

    public void OnDrop(PointerEventData e)
    {
        var item = e.pointerDrag?.GetComponent<LoadoutInventoryItemUI>();
        if (item == null || item.data.category != category) return;
        loadout.EquipItem(item.data);
    }

    public void SetSelected(bool selected)
    {
        if (slotBackground != null)
            slotBackground.color = selected ? SelectedColor : UnselectedColor;
    }
}
