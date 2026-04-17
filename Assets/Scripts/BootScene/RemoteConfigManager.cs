using System.Threading.Tasks;
using Unity.Services.RemoteConfig;
using UnityEngine;

public class RemoteConfigManager : MonoBehaviour
{
    public static RemoteConfigManager Instance { get; private set; }
    public static bool IsFetched { get; private set; } = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async Task FetchAsync()
    {
        try
        {
            await RemoteConfigService.Instance.FetchConfigsAsync(new UserAttributes(), new AppAttributes());
            ApplyFetchedValues();
            IsFetched = true;
            Debug.Log("[RemoteConfig] Fetch successful.");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[RemoteConfig] Fetch failed, using defaults: " + e.Message);
        }
    }

    // ── Pipe Speed ────────────────────────────────────────────────────────────
    public float PipeSpeedStart        { get; private set; } = 5f;
    public float PipeSpeedMax          { get; private set; } = 15f;
    public float PipeSpeedAcceleration { get; private set; } = 0.1f;

    // ── Leaf (Cannabis) Speed ─────────────────────────────────────────────────
    public float LeafSpeedStart        { get; private set; } = 10f;
    public float LeafSpeedMax          { get; private set; } = 20f;

    // ── Pipe Spawning ─────────────────────────────────────────────────────────
    public float PipeSpawnRateBase     { get; private set; } = 3f;
    public float PipeSpawnRateMin      { get; private set; } = 0.5f;
    public float PipeSpawnHeightRange  { get; private set; } = 10f;

    // ── Leaf Spawning ─────────────────────────────────────────────────────────
    public float LeafSpawnRate         { get; private set; } = 10f;
    public float LeafSpawnHeightRange  { get; private set; } = 22f;

    // ── Item: Invincible ──────────────────────────────────────────────────────
    public float InvincibilityDuration { get; private set; } = 2f;
    public float InvincibilityCooldown { get; private set; } = 20f;

    // ── Item: Shrink ──────────────────────────────────────────────────────────
    public float ShrinkDuration        { get; private set; } = 5f;
    public float ShrinkCooldown        { get; private set; } = 15f;
    public float ShrinkScale           { get; private set; } = 0.3f;

    // ── Item: Laser ───────────────────────────────────────────────────────────
    public float LaserCooldown         { get; private set; } = 15f;
    public float LaserSpeed            { get; private set; } = 10f;
    public float LaserLifetime         { get; private set; } = 4f;

    // ── Item: Slow-Mo ─────────────────────────────────────────────────────────
    public float SlowMoDuration        { get; private set; } = 5f;
    public float SlowMoCooldown        { get; private set; } = 10f;
    public float SlowMoSpeedMultiplier { get; private set; } = 0.4f;

    // ── Item: Shield ──────────────────────────────────────────────────────────
    public float ShieldDuration        { get; private set; } = 8f;
    public float ShieldCooldown        { get; private set; } = 12f;

    // ── Event: Direction Flip ─────────────────────────────────────────────────
    public float FlipFirstTriggerMin   { get; private set; } = 25f;
    public float FlipFirstTriggerMax   { get; private set; } = 45f;
    public float FlipCooldownMin       { get; private set; } = 30f;
    public float FlipCooldownMax       { get; private set; } = 55f;
    public float FlipDuration          { get; private set; } = 10f;

    // ── Event: Gravity Inversion ──────────────────────────────────────────────
    public float GravityFirstTriggerMin  { get; private set; } = 30f;
    public float GravityFirstTriggerMax  { get; private set; } = 50f;
    public float GravityCooldownMin      { get; private set; } = 30f;
    public float GravityCooldownMax      { get; private set; } = 55f;
    public float GravityInvertDuration   { get; private set; } = 7f;
    public float GravityInvertStrength   { get; private set; } = 0.75f;
    public float GravityFlapStrength     { get; private set; } = 0.75f;

    // ── XP ────────────────────────────────────────────────────────────────────
    public int   XPMaxLevel              { get; private set; } = 100;
    public int   XPPerLevelMultiplier    { get; private set; } = 100;
    public int   XPLevelScalingFactor    { get; private set; } = 50;
    public int   XPPerLeafCollected      { get; private set; } = 5;
    public float XPPerSecondSurvived     { get; private set; } = 0.5f;

    // ── Economy ───────────────────────────────────────────────────────────────
    public int   DailyRewardCannabis     { get; private set; } = 5;
    public int   MissionRewardCannabis   { get; private set; } = 15;

    private void ApplyFetchedValues()
    {
        var rc = RemoteConfigService.Instance.appConfig;

        PipeSpeedStart        = rc.GetFloat("pipe_speed_start",               PipeSpeedStart);
        PipeSpeedMax          = rc.GetFloat("pipe_speed_max",                 PipeSpeedMax);
        PipeSpeedAcceleration = rc.GetFloat("pipe_speed_acceleration",        PipeSpeedAcceleration);
        LeafSpeedStart        = rc.GetFloat("leaf_speed_start",               LeafSpeedStart);
        LeafSpeedMax          = rc.GetFloat("leaf_speed_max",                 LeafSpeedMax);

        PipeSpawnRateBase     = rc.GetFloat("pipe_spawn_interval_base_sec",    PipeSpawnRateBase);
        PipeSpawnRateMin      = rc.GetFloat("pipe_spawn_interval_min_sec",     PipeSpawnRateMin);
        PipeSpawnHeightRange  = rc.GetFloat("pipe_spawn_height_range",         PipeSpawnHeightRange);
        LeafSpawnRate         = rc.GetFloat("leaf_spawn_interval_sec",         LeafSpawnRate);
        LeafSpawnHeightRange  = rc.GetFloat("leaf_spawn_height_range",         LeafSpawnHeightRange);

        InvincibilityDuration = rc.GetFloat("item_invincible_duration_sec",    InvincibilityDuration);
        InvincibilityCooldown = rc.GetFloat("item_invincible_cooldown_sec",    InvincibilityCooldown);
        ShrinkDuration        = rc.GetFloat("item_shrink_duration_sec",        ShrinkDuration);
        ShrinkCooldown        = rc.GetFloat("item_shrink_cooldown_sec",        ShrinkCooldown);
        ShrinkScale           = rc.GetFloat("item_shrink_scale",               ShrinkScale);
        LaserCooldown         = rc.GetFloat("item_laser_cooldown_sec",         LaserCooldown);
        LaserSpeed            = rc.GetFloat("item_laser_speed",                LaserSpeed);
        LaserLifetime         = rc.GetFloat("item_laser_lifetime_sec",         LaserLifetime);
        SlowMoDuration        = rc.GetFloat("item_slowmo_duration_sec",        SlowMoDuration);
        SlowMoCooldown        = rc.GetFloat("item_slowmo_cooldown_sec",        SlowMoCooldown);
        SlowMoSpeedMultiplier = rc.GetFloat("item_slowmo_speed_multiplier",    SlowMoSpeedMultiplier);
        ShieldDuration        = rc.GetFloat("item_shield_duration_sec",        ShieldDuration);
        ShieldCooldown        = rc.GetFloat("item_shield_cooldown_sec",        ShieldCooldown);

        FlipFirstTriggerMin   = rc.GetFloat("event_flip_first_trigger_min_sec",   FlipFirstTriggerMin);
        FlipFirstTriggerMax   = rc.GetFloat("event_flip_first_trigger_max_sec",   FlipFirstTriggerMax);
        FlipCooldownMin       = rc.GetFloat("event_flip_cooldown_min_sec",        FlipCooldownMin);
        FlipCooldownMax       = rc.GetFloat("event_flip_cooldown_max_sec",        FlipCooldownMax);
        FlipDuration          = rc.GetFloat("event_flip_duration_sec",            FlipDuration);

        GravityFirstTriggerMin  = rc.GetFloat("event_gravity_first_trigger_min_sec", GravityFirstTriggerMin);
        GravityFirstTriggerMax  = rc.GetFloat("event_gravity_first_trigger_max_sec", GravityFirstTriggerMax);
        GravityCooldownMin      = rc.GetFloat("event_gravity_cooldown_min_sec",      GravityCooldownMin);
        GravityCooldownMax      = rc.GetFloat("event_gravity_cooldown_max_sec",      GravityCooldownMax);
        GravityInvertDuration   = rc.GetFloat("event_gravity_duration_sec",          GravityInvertDuration);
        GravityInvertStrength   = rc.GetFloat("event_gravity_invert_strength",       GravityInvertStrength);
        GravityFlapStrength     = rc.GetFloat("event_gravity_flap_strength",         GravityFlapStrength);

        XPMaxLevel            = rc.GetInt  ("xp_max_level",                  XPMaxLevel);
        XPPerLevelMultiplier  = rc.GetInt  ("xp_per_level_multiplier",       XPPerLevelMultiplier);
        XPLevelScalingFactor  = rc.GetInt  ("xp_level_scaling_factor",       XPLevelScalingFactor);
        XPPerLeafCollected    = rc.GetInt  ("xp_per_leaf_collected",         XPPerLeafCollected);
        XPPerSecondSurvived   = rc.GetFloat("xp_per_second_survived",        XPPerSecondSurvived);

        DailyRewardCannabis   = rc.GetInt  ("economy_daily_reward_cannabis",  DailyRewardCannabis);
        MissionRewardCannabis = rc.GetInt  ("economy_mission_reward_cannabis",MissionRewardCannabis);

        XPManager.ApplyRemoteConfig(this);
    }

    private struct UserAttributes {}
    private struct AppAttributes  {}
}
