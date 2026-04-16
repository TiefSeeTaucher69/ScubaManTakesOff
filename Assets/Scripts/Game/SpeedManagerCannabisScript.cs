using UnityEngine;

public class SpeedManagerCannabisScript : MonoBehaviour
{
    public static float currentSpeed = 10f;
    public static float acceleration = 0.1f;
    public static float maxSpeed = 20f;
    public static float SlowMoMultiplier = 1f;

    void Awake()
    {
        // Sicherstellen, dass es nur eine Instanz gibt
        if (FindObjectsOfType<SpeedManagerCannabisScript>().Length > 1)
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
        currentSpeed = 10f;
        SlowMoMultiplier = 1f;
    }
}

