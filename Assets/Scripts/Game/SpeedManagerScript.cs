using UnityEngine;

public class SpeedManager : MonoBehaviour
{
    public const  float startSpeed   = 5f;
    public static float currentSpeed { get; private set; } = startSpeed;
    public static float acceleration = 0.1f;
    public static float maxSpeed = 15f;
    public static float SlowMoMultiplier = 1f;

    void Awake()
    {
        // Sicherstellen, dass es nur eine Instanz gibt
        if (FindObjectsOfType<SpeedManager>().Length > 1)
        {
            Debug.LogWarning("Mehrere SpeedManager vorhanden � das sollte nicht passieren!");
        }
    }

    void Update()
    {
        currentSpeed += acceleration * Time.deltaTime;
        currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
    }

    public static void ResetSpeed()
    {
        currentSpeed = startSpeed;
        SlowMoMultiplier = 1f;
    }

}
