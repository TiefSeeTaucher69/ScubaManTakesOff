using Assets.FantasyMonsters.Scripts;
using UnityEngine;

public class PetShopPreviewScript : MonoBehaviour
{
    [SerializeField] private Transform previewAnchor; // Weltposition des Preview-Pets

    private GameObject _currentPreview;

    public void ShowPreview(GameObject prefab)
    {
        if (_currentPreview != null) Destroy(_currentPreview);
        if (prefab == null) return;

        _currentPreview = Instantiate(prefab, previewAnchor.position, Quaternion.identity);
        _currentPreview.transform.localScale = Vector3.one;

        // Layer auf PetPreview setzen damit nur die Preview-Camera es rendert
        SetLayerRecursive(_currentPreview, LayerMask.NameToLayer("PetPreview"));

        foreach (var col in _currentPreview.GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        _currentPreview.GetComponent<Monster>()?.SetState(MonsterState.Ready);
    }

    public void HidePreview()
    {
        if (_currentPreview != null) Destroy(_currentPreview);
        _currentPreview = null;
    }

    private static void SetLayerRecursive(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursive(child.gameObject, layer);
    }
}
