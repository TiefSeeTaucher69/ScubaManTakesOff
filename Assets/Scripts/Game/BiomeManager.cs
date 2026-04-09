using System.Collections.Generic;
using UnityEngine;

public class BiomeManager : MonoBehaviour
{
    [System.Serializable]
    public class BiomeEntry
    {
        public string    biomeName;
        public Texture2D backgroundTexture;  // FullBG Texture2D — null = Fallback-Farbe
        public Color     fallbackColor = new Color(0.53f, 0.81f, 0.92f, 1f);
        public Material  pipeMaterial;       // PipeBark_XX
    }

    [Header("Biome-Einträge (im Inspector befüllen)")]
    public List<BiomeEntry> biomes = new();

    [Header("SpriteRenderer des Hintergrund-GameObjects")]
    public SpriteRenderer backgroundRenderer;

    [Header("Scrolling")]
    [Range(0f, 1f)]
    public float parallaxFactor = 1.0f; // 1.0 = gleiche Geschwindigkeit wie Pipes

    public static Material ActivePipeMaterial { get; private set; }

    private bool       _scrolling    = false;
    private float      _bgWorldWidth = 0f;
    private float      _scrollOffset = 0f;   // einzige Source of Truth — kein Float-Akkumulationsfehler
    private Transform  _tileA;
    private GameObject _tileBObject;
    private Transform  _tileB;

    void Awake()
    {
        string active = PlayerPrefs.GetString("ActiveBiome", "Mountain");
        BiomeEntry entry = biomes.Find(b => b.biomeName == active);
        if (entry == null && biomes.Count > 0)
            entry = biomes[0];

        if (entry == null) return;

        ActivePipeMaterial = entry.pipeMaterial;

        if (backgroundRenderer != null)
        {
            if (entry.backgroundTexture != null)
            {
                var tex = entry.backgroundTexture;
                float ppu = 100f;
                backgroundRenderer.sprite = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    ppu);
                backgroundRenderer.color = Color.white;

                Camera cam = Camera.main;
                if (cam != null && cam.orthographic)
                {
                    float camH = cam.orthographicSize * 2f;
                    float camW = camH * cam.aspect;
                    float spriteH = tex.height / ppu;
                    float spriteW = tex.width  / ppu;
                    backgroundRenderer.transform.localScale = new Vector3(
                        camW / spriteW,
                        camH / spriteH,
                        1f);

                    // Scrolling-Setup: zwei Tiles nebeneinander
                    _bgWorldWidth = camW;
                    _tileA = backgroundRenderer.transform;
                    _tileA.position = new Vector3(0f, 0f, 1f);

                    _tileBObject = Instantiate(backgroundRenderer.gameObject);
                    _tileBObject.name = "BiomeBackground_Clone";
                    _tileB = _tileBObject.transform;
                    _tileB.position = new Vector3(_bgWorldWidth, 0f, 1f);

                    _scrolling = true;
                }
            }
            else
            {
                backgroundRenderer.sprite = null;
                backgroundRenderer.color  = entry.fallbackColor;
                backgroundRenderer.transform.localScale = Vector3.one * 1000f;
                // Solid color: kein Scrolling nötig
            }
        }
    }

    void Update()
    {
        if (!_scrolling) return;

        _scrollOffset -= SpeedManager.currentSpeed * parallaxFactor * Time.deltaTime;

        // Modulo-Wrap: sobald ein komplettes Tile-Bild rausgescrollt ist → zurücksetzen
        if (_scrollOffset <= -_bgWorldWidth)
            _scrollOffset += _bgWorldWidth;

        // Beide Positionen immer aus dem gleichen Offset berechnet → niemals eine Lücke
        _tileA.position = new Vector3(_scrollOffset,                  0f, 1f);
        _tileB.position = new Vector3(_scrollOffset + _bgWorldWidth,  0f, 1f);
    }

    void OnDestroy()
    {
        ActivePipeMaterial = null;
        if (_tileBObject != null)
            Destroy(_tileBObject);
    }
}
