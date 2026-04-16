using UnityEngine;
using System;
using System.Globalization;

public class RankedManager : MonoBehaviour
{
    public static RankedManager Instance { get; private set; }
    public static bool IsRanked { get; set; } = false;
    public static string WeeklyItem { get; private set; } = "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeWeeklyData();
    }

    public static int GetWeekSeed()
    {
        DateTime now = DateTime.UtcNow;
        Calendar cal = CultureInfo.InvariantCulture.Calendar;
        int week = cal.GetWeekOfYear(now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        return now.Year * 100 + week;
    }

    private static void InitializeWeeklyData()
    {
        int seed = GetWeekSeed();
        var rng = new System.Random(seed);
        string[] items = { "Invincible", "Shrink", "Laser", "SlowMo", "Shield" };
        WeeklyItem = items[rng.Next(0, 5)];
        Debug.Log($"[RankedManager] Woche {seed} – Weekly Item: {WeeklyItem}");
    }

    public static System.Random CreateFreshPipeRng()
    {
        return new System.Random(GetWeekSeed());
    }

    public static TimeSpan GetTimeUntilReset()
    {
        DateTime now = DateTime.UtcNow;
        int daysUntilMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
        if (daysUntilMonday == 0) daysUntilMonday = 7;
        DateTime nextMonday = now.Date.AddDays(daysUntilMonday);
        return nextMonday - now;
    }

    public static string GetWeeklyItemDisplayName()
    {
        return WeeklyItem switch
        {
            "Invincible" => "Invincible",
            "Shrink"     => "Shrink",
            "Laser"      => "Laser Shot",
            "SlowMo"     => "Slow-Mo",
            "Shield"     => "Shield",
            _            => WeeklyItem
        };
    }
}
