using UnityEngine;

public class CannabisSpawnerScript : MonoBehaviour
{
    public GameObject cannabisPrefab;
    public float spawnRate = 1;
    private float timer = 0f;
    public float heightOffset = 11;

    void Start()
    {
        SpawnCannabis();
    }

    void Update()
    {
        if (timer < spawnRate)
        {
            timer += Time.deltaTime;
        }
        else
        {
            SpawnCannabis();
            timer = 0;
        }

    }

    void SpawnCannabis()
    {
        float lowestPoint = transform.position.y;
        float highestPoint = transform.position.y + heightOffset;
        float baseX = Mathf.Abs(transform.position.x) + 10f;
        float spawnX = DirectionFlipManager.IsFlipped ? -baseX : baseX;
        Instantiate(cannabisPrefab, new Vector3(spawnX, Random.Range(lowestPoint, highestPoint)), transform.rotation);
    }

}
