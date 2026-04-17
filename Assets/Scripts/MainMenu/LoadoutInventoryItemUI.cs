using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Component on each inventory card in the Loadout panel.
/// Supports click-to-equip and drag-and-drop onto LoadoutSlotUI.
/// </summary>
public class LoadoutInventoryItemUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Image    iconImage;
    public TMP_Text nameText;
    public TMP_Text stackCountText;

    [HideInInspector] public LoadoutItemData data;
    [HideInInspector] public LoadoutScript   loadout;

    private CanvasGroup _cg;
    private Transform   _originalParent;
    private Vector3     _originalPosition;
    private Canvas      _rootCanvas;

    public void Setup(LoadoutItemData d, LoadoutScript l, Sprite fallbackIcon = null)
    {
        data    = d;
        loadout = l;
        _cg         = GetComponent<CanvasGroup>();
        _rootCanvas = GetComponentInParent<Canvas>();

        if (iconImage != null) iconImage.sprite = d.icon != null ? d.icon : fallbackIcon;
        if (nameText  != null) nameText.text    = d.displayName;
        RefreshCount();
    }

    public void RefreshCount()
    {
        if (stackCountText == null) return;
        if (data.isStackable)
        {
            int c = PlayerPrefs.GetInt(data.countKey, 0);
            stackCountText.gameObject.SetActive(true);
            stackCountText.text = $"×{c}";
        }
        else
        {
            stackCountText.gameObject.SetActive(false);
        }
    }

    // ── Click: equip directly ─────────────────────────────────────────────
    public void OnPointerClick(PointerEventData e)
    {
        if (e.dragging) return;
        loadout.EquipItem(data);
    }

    // ── Drag ──────────────────────────────────────────────────────────────
    public void OnBeginDrag(PointerEventData e)
    {
        _originalParent   = transform.parent;
        _originalPosition = transform.localPosition;

        // Reparent to root canvas so it renders on top of everything
        transform.SetParent(_rootCanvas.transform, true);
        transform.SetAsLastSibling();

        if (_cg != null) { _cg.alpha = 0.75f; _cg.blocksRaycasts = false; }
    }

    public void OnDrag(PointerEventData e)
    {
        transform.position = e.position;
    }

    public void OnEndDrag(PointerEventData e)
    {
        // Restore to original parent whether or not a drop was accepted
        transform.SetParent(_originalParent, true);
        transform.localPosition = _originalPosition;
        if (_cg != null) { _cg.alpha = 1f; _cg.blocksRaycasts = true; }
        loadout.RefreshInventory();
    }
}
