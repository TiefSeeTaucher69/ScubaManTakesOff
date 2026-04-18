using UnityEngine;

public class SpeedManager : MonoBehaviour
{
    public static float startSpeed   = 5f;
    public static float currentSpeed { get; private set; } = 5f;
    public static float acceleration = 0.1f;
    public static float maxSpeed = 15f;
    public static float SlowMoMultiplier = 1f;
    private static bool _running = false;

    void Awake()
    {
        if (FindObjectsByType<SpeedManager>(FindObjectsSortMode.None).Length > 1)
            Debug.LogWarning("Mehrere SpeedManager vorhanden - das sollte nicht passieren!");

        SlowMoMultiplier = 1f;
        _running = true;

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
        if (!_running) return;
        currentSpeed += acceleration * Time.deltaTime;
        currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
    }

    public static void ResetSpeed()
    {
        _running = false;
        currentSpeed = RemoteConfigManager.Instance != null ? RemoteConfigManager.Instance.PipeSpeedStart : startSpeed;
        SlowMoMultiplier = 1f;
    }

}
