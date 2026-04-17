using UnityEngine;

public static class XPManager
{
    public static int   MaxLevel           = 100;
    public static int   PerLevelMultiplier = 100;
    public static int   ScalingFactor      = 50;
    public static int   XPPerLeaf          = 5;
    public static float XPPerSecond        = 0.5f;

    public static void ApplyRemoteConfig(RemoteConfigManager rc)
    {
        MaxLevel           = rc.XPMaxLevel;
        PerLevelMultiplier = rc.XPPerLevelMultiplier;
        ScalingFactor      = rc.XPLevelScalingFactor;
        XPPerLeaf          = rc.XPPerLeafCollected;
        XPPerSecond        = rc.XPPerSecondSurvived;
    }

    public static int XPForLevel(int n) => PerLevelMultiplier * n;

    public static int TotalXPForLevel(int n) => ScalingFactor * n * (n - 1);

    public static int GetLevel(int totalXP)
    {
        int level = 1;
        while (level < MaxLevel && totalXP >= TotalXPForLevel(level + 1))
            level++;
        return level;
    }

    public static int GetXPInLevel(int totalXP)
    {
        return totalXP - TotalXPForLevel(GetLevel(totalXP));
    }

    public static int GetXPRequired(int totalXP)
    {
        int level = GetLevel(totalXP);
        return XPForLevel(level >= MaxLevel ? MaxLevel : level);
    }

    public static int CalculateRunXP(int score, int leavesCollected, float timeSurvived)
    {
        return score + leavesCollected * XPPerLeaf + Mathf.FloorToInt(timeSurvived * XPPerSecond);
    }

    public static bool ShouldMigrate(int totalXP, int totalScore) => totalXP == 0 && totalScore > 0;
    public static int MigrationGrant(int totalScore) => totalScore / 2;

    public static int XPCap => TotalXPForLevel(MaxLevel) + XPForLevel(MaxLevel);
}
