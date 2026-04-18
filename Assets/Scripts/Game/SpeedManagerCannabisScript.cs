using UnityEngine;

public class SpeedManagerCannabisScript : MonoBehaviour
{
    public static float startSpeed = 10f;
    public static float currentSpeed = 10f;
    public static float acceleration = 0.1f;
    public static float maxSpeed = 20f;
    public static float SlowMoMultiplier = 1f;

    void Awake()
    {
        if (FindObjectsByType<SpeedManagerCannabisScript>(FindObjectsSortMode.None).Length > 1)
            Debug.LogWarning("Mehrere SpeedManager vorhanden - das sollte nicht passieren!");

        SlowMoMultiplier = 1f;

        if (RemoteConfigManager.Instance != null)
        {
            startSpeed   = RemoteConfigManager.Instance.LeafSpeedStart;
            maxSpeed     = RemoteConfigManager.Instance.LeafSpeedMax;
            acceleration = RemoteConfigManager.Instance.LeafSpeedAcceleration;
            currentSpeed = startSpeed;
        }
    }

    void Update()
    {
        currentSpeed += acceleration * Time.deltaTime;
        currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
    }

    public static void ResetSpeed()
    {
        currentSpeed = RemoteConfigManager.Instance != null ? RemoteConfigManager.Instance.LeafSpeedStart : startSpeed;
        SlowMoMultiplier = 1f;
    }
}
