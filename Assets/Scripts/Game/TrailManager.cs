using UnityEngine;

public class TrailManager : MonoBehaviour
{
    public GameObject RedTrailPrefab;
    public GameObject PurpleTrailPrefab;
    public GameObject BlueTrailPrefab;

    private GameObject trailInstance;
    private ParticleSystem trailParticles;

    void Start()
    {
        string activeTrail = PlayerPrefs.GetString("ActiveTrail", "");

        // Offset nach links (z.B. -0.5f auf der X-Achse)
        Vector3 leftOffset = new Vector3(-0.5f, -0.5f, 0);

        Color trailColor = Color.white;

        if (activeTrail == "TrailRed")
        {
            trailInstance = Instantiate(RedTrailPrefab, transform.position + leftOffset, Quaternion.identity, transform);
            trailColor = new Color(1f, 0.2f, 0.2f, 1f); // Rot
        }
        else if (activeTrail == "TrailPurple")
        {
            trailInstance = Instantiate(PurpleTrailPrefab, transform.position + leftOffset, Quaternion.identity, transform);
            trailColor = new Color(1f, 0f, 1f, 1f); // Lila
        }
        else if (activeTrail == "TrailBlue")
        {
            trailInstance = Instantiate(BlueTrailPrefab, transform.position + leftOffset, Quaternion.identity, transform);
            trailColor = new Color(0.2f, 0.5f, 1f, 1f); // Blau
        }

        if (trailInstance != null)
        {
            trailParticles = trailInstance.GetComponent<ParticleSystem>();
            if (trailParticles != null)
            {
                var main = trailParticles.main;
                main.startColor = trailColor;
            }
        }
    }

    void Update()
    {
        // Trail-Position spiegeln wenn Richtung gewechselt wird
        if (trailInstance != null)
        {
            float offsetX = DirectionFlipManager.IsFlipped ? 0.5f : -0.5f;
            trailInstance.transform.localPosition = new Vector3(offsetX, -0.5f, 0f);
        }

        if (trailParticles != null)
        {
            var main = trailParticles.main;
            main.startSpeed = SpeedManager.currentSpeed;
        }
    }
}
