using UnityEngine;

public class SpeedManager : MonoBehaviour
{
    public static float startSpeed   = 5f;
    public static float currentSpeed { get; private set; } = 5f;
    public static float acceleration = 0.1f;
    public static float maxSpeed = 15f;
    public static float SlowMoMultiplier = 1f;

    void Awake()
    {
        if (FindObjectsOfType<SpeedManager>().Length > 1)
            Debug.LogWarning("Mehrere SpeedManager vorhanden - das sollte nicht passieren!");

        if (RemoteConfigManager.Instance != null)
        {
            startSpeed   = RemoteConfigManager.Instance.PipeSpeedStart;
            maxSpeed     = RemoteConfigManager.Instance.PipeSpeedMax;
            acceleration = RemoteConfigManager.Instance.PipeSpeedAcceleration;
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
        currentSpeed = RemoteConfigManager.Instance != null ? RemoteConfigManager.Instance.PipeSpeedStart : startSpeed;
        SlowMoMultiplier = 1f;
    }

}
