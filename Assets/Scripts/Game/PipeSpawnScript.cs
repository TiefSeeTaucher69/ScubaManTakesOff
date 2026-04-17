using UnityEngine;

public class PipeSpawnScript : MonoBehaviour
{
    public GameObject pipe;

    public float baseSpawnRate = 3f;
    public float minSpawnRate = 0.5f;
    public float startSpeed = 5f;
    public float heightOffset = 10f;

    private float timer = 0f;
    private System.Random rankedRng;

    void Start()
    {
        if (RemoteConfigManager.Instance != null)
        {
            baseSpawnRate = RemoteConfigManager.Instance.PipeSpawnRateBase;
            minSpawnRate  = RemoteConfigManager.Instance.PipeSpawnRateMin;
            startSpeed    = RemoteConfigManager.Instance.PipeSpeedStart;
            heightOffset  = RemoteConfigManager.Instance.PipeSpawnHeightRange;
        }

        if (RankedManager.IsRanked)
            rankedRng = RankedManager.CreateFreshPipeRng();

        spawnPipe(); // Erste Pipe direkt spawnen
    }

    void Update()
    {
        float currentSpeed = SpeedManager.currentSpeed * SpeedManager.SlowMoMultiplier;

        float adjustedSpawnRate = Mathf.Max(minSpawnRate, baseSpawnRate * (startSpeed / currentSpeed));

        if (timer < adjustedSpawnRate)
        {
            timer += Time.deltaTime;
        }
        else
        {
            spawnPipe();
            timer = 0f;
        }
    }

    // Sofortiger Spawn + Timer-Reset — wird beim Richtungswechsel aufgerufen
    public void ForceSpawnNow()
    {
        spawnPipe();
        timer = 0f;
    }

    void spawnPipe()
    {
        float y;
        if (RankedManager.IsRanked && rankedRng != null)
        {
            y = (float)(rankedRng.NextDouble() * heightOffset * 2 - heightOffset) + transform.position.y;
        }
        else
        {
            y = Random.Range(transform.position.y - heightOffset, transform.position.y + heightOffset);
        }

        float spawnX = DirectionFlipManager.IsFlipped
            ? -Mathf.Abs(transform.position.x)
            :  Mathf.Abs(transform.position.x);
        GameObject spawnedPipe = Instantiate(pipe, new Vector3(spawnX, y), transform.rotation);

        if (BiomeManager.ActivePipeMaterial != null)
        {
            foreach (var sr in spawnedPipe.GetComponentsInChildren<SpriteRenderer>())
                sr.sharedMaterial = BiomeManager.ActivePipeMaterial;
        }
    }
}
