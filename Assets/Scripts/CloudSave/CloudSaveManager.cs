using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using UnityEngine;

/// <summary>
/// Singleton: Synchronisiert wichtige PlayerPrefs-Daten mit Unity Cloud Save.
/// Schreibt sofort in PlayerPrefs, dann asynchron in die Cloud (fire-and-forget).
/// Beim Login wird die Cloud geladen und überschreibt lokale Daten.
/// </summary>
public class CloudSaveManager : MonoBehaviour
{
    private static CloudSaveManager _instance;

    public static CloudSaveManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("CloudSaveManager");
                _instance = go.AddComponent<CloudSaveManager>();
            }
            return _instance;
        }
    }

    // Int-Keys die in der Cloud gespeichert werden
    private static readonly HashSet<string> IntKeys = new HashSet<string>
    {
        "CannabisStash",
        "Highscore", "RankedHighscore", "TotalScore", "TotalRuns",
        "HasInvincibleItem", "HasLaserItem", "HasShrinkItem",
        "HasSlowMoItem", "HasShieldItem",
        "ItemCount_Invincible", "ItemCount_Shrink", "ItemCount_Laser",
        "ItemCount_SlowMo", "ItemCount_Shield",
        "HasTrailRed", "HasTrailPurple", "HasTrailBlue",
        "HasSkin_ginger-bird", "HasSkin_tom-bird", "HasSkin_bennet-bird", "HasSkin_jan-bird",
        "HasSkin_paulaner-bird",
        "HasPetBlackCat",
        "TotalXP", "PlayerLevel"
    };

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Nach Login aufrufen: lädt alle Cloud-Daten in PlayerPrefs.
    /// Cloud-Daten gewinnen gegenüber lokalen Daten.
    /// </summary>
    public async Task LoadAllAsync()
    {
        try
        {
            var data = await CloudSaveService.Instance.Data.Player.LoadAllAsync();
            foreach (var kvp in data)
            {
                string key = kvp.Key;
                if (kvp.Value?.Value == null) continue;
                try
                {
                    if (IsIntKey(key))
                        PlayerPrefs.SetInt(key, kvp.Value.Value.GetAs<int>());
                    else
                        PlayerPrefs.SetString(key, kvp.Value.Value.GetAs<string>());
                }
                catch
                {
                    try { PlayerPrefs.SetString(key, kvp.Value.Value.GetAs<string>()); }
                    catch { /* Wert überspringen */ }
                }
            }
            PlayerPrefs.Save();
            Debug.Log($"[CloudSave] {data.Count} Keys geladen.");
        }
        catch (Exception e)
        {
            Debug.LogWarning("[CloudSave] Laden fehlgeschlagen (offline?): " + e.Message);
        }
    }

    /// <summary>Int-Wert sofort in PlayerPrefs + asynchron in Cloud speichern.</summary>
    public void SaveInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
        PushAsync(new Dictionary<string, object> { { key, value } });
    }

    /// <summary>String-Wert sofort in PlayerPrefs + asynchron in Cloud speichern.</summary>
    public void SaveString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
        PlayerPrefs.Save();
        PushAsync(new Dictionary<string, object> { { key, value } });
    }

    /// <summary>Mehrere Werte auf einmal sofort in PlayerPrefs + asynchron in Cloud speichern.</summary>
    public void SaveBatch(Dictionary<string, object> batch)
    {
        foreach (var kvp in batch)
        {
            if (kvp.Value is int i)      PlayerPrefs.SetInt(kvp.Key, i);
            else if (kvp.Value is string s) PlayerPrefs.SetString(kvp.Key, s);
        }
        PlayerPrefs.Save();
        PushAsync(batch);
    }

    private async void PushAsync(Dictionary<string, object> data)
    {
        try
        {
            if (!Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn) return;
            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
            Debug.Log($"[CloudSave] Gespeichert: {string.Join(", ", data.Keys)}");
        }
        catch (Exception e)
        {
            Debug.LogWarning("[CloudSave] Speichern fehlgeschlagen (offline?): " + e.Message);
        }
    }

    private bool IsIntKey(string key) =>
        IntKeys.Contains(key)
        || key.StartsWith("MissionRewardCollected_")
        || key.StartsWith("HasBiome")
        || key.StartsWith("HasPet");
}
