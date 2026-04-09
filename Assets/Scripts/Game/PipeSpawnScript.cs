using UnityEngine;

public class PipeSpawnScript : MonoBehaviour
{
    public GameObject pipe;

    public float baseSpawnRate = 3f;       // urspr�ngliche Spawnrate bei Startgeschwindigkeit
    public float minSpawnRate = 0.5f;      // minimale Spawnrate, damit's nicht unspielbar wird
    public float startSpeed = 5f;          // Startgeschwindigkeit, muss mit SpeedManager �bereinstimmen
    public float heightOffset = 10f;

    private float timer = 0f;
    private System.Random rankedRng;

    void Start()
    {
        if (RankedManager.IsRanked)
            rankedRng = RankedManager.CreateFreshPipeRng();

        spawnPipe(); // Erste Pipe direkt spawnen
    }

    void Update()
    {
        float currentSpeed = SpeedManager.currentSpeed;

        // Spawnrate anpassen, aber Mindestgrenze einhalten
        float adjustedSpawnRate = Mathf.Clamp(baseSpawnRate * (startSpeed / currentSpeed), minSpawnRate, baseSpawnRate);

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

        GameObject spawnedPipe = Instantiate(pipe, new Vector3(transform.position.x, y), transform.rotation);

        if (BiomeManager.ActivePipeMaterial != null)
        {
            foreach (var sr in spawnedPipe.GetComponentsInChildren<SpriteRenderer>())
                sr.sharedMaterial = BiomeManager.ActivePipeMaterial;
        }
    }
}
