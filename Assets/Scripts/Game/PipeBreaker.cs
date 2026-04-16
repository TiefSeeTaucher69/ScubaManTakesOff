using UnityEngine;
using System.Collections;

/// <summary>
/// Singleton: Zerstört Pipes mit Crumble-Animation + Partikeleffekt.
/// Aufruf: PipeBreaker.Break(pipeGameObject)
/// Kein Prefab nötig — Partikel werden vollständig per Code erzeugt.
/// </summary>
public class PipeBreaker : MonoBehaviour
{
    public static PipeBreaker Instance { get; private set; }

    [Tooltip("Dauer der Crumble-Animation in Sekunden.")]
    public float crumbleDuration = 0.25f;

    [Tooltip("Maximale Shake-Amplitude zu Beginn.")]
    public float shakeStrength = 0.2f;

    [Tooltip("Anzahl gespawnter Partikel.")]
    public int particleCount = 50;

    void Awake()
    {
        Instance = this;
    }

    public static void Break(GameObject pipe)
    {
        if (pipe == null) return;

        // Collider sofort deaktivieren → keine weiteren Kollisionen
        foreach (var col in pipe.GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        if (Instance != null)
            Instance.StartCoroutine(Instance.BreakCoroutine(pipe));
        else
            Destroy(pipe);
    }

    IEnumerator BreakCoroutine(GameObject pipe)
    {
        if (pipe == null) yield break;

        // Partikel als Kind der Pipe spawnen → bewegen sich automatisch mit
        GameObject psObj = CreateParticles(pipe);

        float elapsed = 0f;
        while (elapsed < crumbleDuration && pipe != null)
        {
            float t = elapsed / crumbleDuration;
            float shake = shakeStrength * (1f - t);
            pipe.transform.position += (Vector3)(Random.insideUnitCircle * shake);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (pipe != null)
        {
            // Vor Destroy von Pipe trennen, damit PS eigenständig weiterläuft
            if (psObj != null)
                psObj.transform.SetParent(null);
            Destroy(pipe);
        }
    }

    GameObject CreateParticles(GameObject pipe)
    {
        // Gesamte Bounds aus allen SpriteRenderers berechnen (= Pipeform)
        Bounds bounds = new Bounds(pipe.transform.position, Vector3.zero);
        var spriteRenderers = pipe.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in spriteRenderers)
            bounds.Encapsulate(sr.bounds);

        // ParticleSystem als Kind der Pipe anlegen
        GameObject psObj = new GameObject("PipeBreakParticles");
        psObj.transform.SetParent(pipe.transform);
        psObj.transform.position = bounds.center;

        var ps = psObj.AddComponent<ParticleSystem>();
        // Sofort stoppen — Unity spielt PS automatisch ab nach AddComponent
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Material vom SpriteRenderer übernehmen → korrekte Pipe-Farbe/-Textur
        var psRenderer = ps.GetComponent<ParticleSystemRenderer>();
        var firstSr = spriteRenderers.Length > 0 ? spriteRenderers[0] : null;
        if (firstSr != null && firstSr.sharedMaterial != null)
            psRenderer.material = firstSr.sharedMaterial;

        // Main-Modul
        var main = ps.main;
        main.duration        = 0.5f;
        main.loop            = false;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.4f, 1f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(1.5f, 6f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.15f, 0.45f);
        main.startColor      = Color.white; // Farbe kommt vom Material
        main.gravityModifier = 1.5f;
        // World-Space: bereits gespawnte Partikel fliegen frei weiter
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction      = ParticleSystemStopAction.Destroy;

        // Einmaliger Burst beim Start
        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, particleCount) });

        // Box-Form passend zur Pipe-Größe → Partikel verteilt über gesamte Pipelänge
        var shape = ps.shape;
        shape.enabled   = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale     = new Vector3(
            Mathf.Max(bounds.size.x, 0.5f),
            Mathf.Max(bounds.size.y, 1f),
            0.1f);

        ps.Play();
        return psObj;
    }
}
