using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToastManager : MonoBehaviour
{
    public static ToastManager Instance { get; private set; }

    [SerializeField] GameObject _toastPrefab;

    [Header("Banner Sprites")]
    [SerializeField] Sprite _successSprite;
    [SerializeField] Sprite _warningSprite;
    [SerializeField] Sprite _infoSprite;
    [SerializeField] Sprite _rewardSprite;

    [Header("Timer Bar Colors")]
    [SerializeField] Color _successColor = new Color(0.2f, 0.85f, 0.2f);
    [SerializeField] Color _warningColor = new Color(1f,   0.6f,  0f);
    [SerializeField] Color _infoColor    = new Color(0.2f, 0.6f,  1f);
    [SerializeField] Color _rewardColor  = new Color(0.65f, 0.2f, 1f);

    [Header("Settings")]
    [SerializeField] float _defaultDuration = 3f;
    [SerializeField] int   _maxVisible      = 3;

    const float TOAST_HEIGHT  = 80f;
    const float TIMEBAR_H     = 6f;
    const float SLOT_HEIGHT   = TOAST_HEIGHT + TIMEBAR_H + 6f; // 92 px per Slot
    const float TOP_PADDING   = 10f;
    const float LEFT_PADDING  = 10f;

    int   _activeCount;
    float _nextSlotY;
    Queue<(string, ToastType, float)> _queue = new Queue<(string, ToastType, float)>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildCanvas();
        _nextSlotY = -TOP_PADDING;
    }

    void BuildCanvas()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight  = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();
    }

    // ── Statische API ─────────────────────────────────────────────

    public static void Show(string message, ToastType type, float duration = -1f)
    {
        if (Instance == null)
        {
            Debug.LogWarning("[ToastManager] Kein Instance gefunden.");
            return;
        }
        Instance.ShowInternal(message, type, duration);
    }

    // ── Intern ───────────────────────────────────────────────────

    void ShowInternal(string message, ToastType type, float duration)
    {
        float d = duration <= 0f ? _defaultDuration : duration;

        if (_activeCount >= _maxVisible)
        {
            _queue.Enqueue((message, type, d));
            return;
        }
        SpawnToast(message, type, d);
    }

    void SpawnToast(string message, ToastType type, float duration)
    {
        if (_toastPrefab == null)
        {
            Debug.LogError("[ToastManager] Toast-Prefab nicht zugewiesen!");
            return;
        }

        _activeCount++;
        float slotY = _nextSlotY;
        _nextSlotY -= SLOT_HEIGHT;

        GameObject go = Instantiate(_toastPrefab, transform);

        if (go.TryGetComponent<ToastController>(out var ctrl))
            ctrl.Init(message, GetSprite(type), GetColor(type), duration,
                      slotY, LEFT_PADDING, OnToastComplete);
    }

    void OnToastComplete()
    {
        _activeCount--;
        if (_activeCount == 0)
            _nextSlotY = -TOP_PADDING; // Slots zurücksetzen wenn alle weg

        if (_queue.Count > 0)
        {
            var (msg, type, dur) = _queue.Dequeue();
            SpawnToast(msg, type, dur);
        }
    }

    Sprite GetSprite(ToastType type) => type switch
    {
        ToastType.Success => _successSprite,
        ToastType.Warning => _warningSprite,
        ToastType.Info    => _infoSprite,
        ToastType.Reward  => _rewardSprite,
        _                 => null
    };

    Color GetColor(ToastType type) => type switch
    {
        ToastType.Success => _successColor,
        ToastType.Warning => _warningColor,
        ToastType.Info    => _infoColor,
        ToastType.Reward  => _rewardColor,
        _                 => Color.white
    };
}
