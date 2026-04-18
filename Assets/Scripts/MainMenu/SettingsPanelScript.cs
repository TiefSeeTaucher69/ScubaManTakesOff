using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Inline-Settings-Panel im MainMenu.
/// Selbe PlayerPrefs-Keys wie SettingsSceneHandlerScript / VolumeControlScripts.
/// </summary>
public class SettingsPanelScript : MonoBehaviour
{
    [Header("FPS")]
    public TMPro.TMP_Dropdown fpsDropdown;

    [Header("Auflösung & VSync")]
    public TMPro.TMP_Dropdown resolutionDropdown;
    public Toggle vsyncToggle;

    [Header("Lautstärke")]
    public AudioMixer audioMixer;
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    private Resolution[] resolutions;

    void Start()
    {
        LadeFPS();
        LadeAufloesung();
        LadeVSync();
        LadeLautstaerke();
    }

    // ── Auflösung ─────────────────────────────────────

    void LadeAufloesung()
    {
        if (resolutionDropdown == null) return;

        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        int currentResIndex = 0;
        var options = new List<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            int hz = (int)resolutions[i].refreshRateRatio.value;
            options.Add(resolutions[i].width + " x " + resolutions[i].height + " @ " + hz + "Hz");

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height &&
                resolutions[i].refreshRateRatio.Equals(Screen.currentResolution.refreshRateRatio))
                currentResIndex = i;
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.onValueChanged.RemoveAllListeners();
        resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex", currentResIndex);
        resolutionDropdown.RefreshShownValue();
        WendeAufloesungAn(resolutionDropdown.value);
        resolutionDropdown.onValueChanged.AddListener(OnAufloesungChanged);
    }

    void OnAufloesungChanged(int index)
    {
        WendeAufloesungAn(index);
        PlayerPrefs.SetInt("ResolutionIndex", index);
        PlayerPrefs.Save();
    }

    void WendeAufloesungAn(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode, res.refreshRateRatio);
    }

    // ── FPS ──────────────────────────────────────────

    void LadeFPS()
    {
        int saved = PlayerPrefs.GetInt("FPSCap", 3); // Default: 240 FPS
        fpsDropdown.onValueChanged.RemoveAllListeners();
        fpsDropdown.value = saved;
        WendeFPSAn(saved);
        fpsDropdown.onValueChanged.AddListener(OnFPSChanged);
    }

    void OnFPSChanged(int index)
    {
        WendeFPSAn(index);
        PlayerPrefs.SetInt("FPSCap", index);
        PlayerPrefs.Save();
    }

    void WendeFPSAn(int index)
    {
        switch (index)
        {
            case 0: Application.targetFrameRate = 30;  break;
            case 1: Application.targetFrameRate = 60;  break;
            case 2: Application.targetFrameRate = 120; break;
            case 3: Application.targetFrameRate = 240; break;
            default: Application.targetFrameRate = -1; break; // unbegrenzt
        }
    }

    // ── VSync ─────────────────────────────────────────

    void LadeVSync()
    {
        int saved = PlayerPrefs.GetInt("VSyncEnabled", 0);
        QualitySettings.vSyncCount = saved;
        vsyncToggle.isOn = saved == 1;
        vsyncToggle.onValueChanged.AddListener(OnVSyncChanged);
    }

    void OnVSyncChanged(bool isOn)
    {
        int value = isOn ? 1 : 0;
        QualitySettings.vSyncCount = value;
        PlayerPrefs.SetInt("VSyncEnabled", value);
        PlayerPrefs.Save();
    }

    // ── Lautstärke ────────────────────────────────────

    void LadeLautstaerke()
    {
        SetupSlider(masterSlider, "MasterVolume", "MasterVolume");
        SetupSlider(musicSlider,  "MusicVolume",  "MusicVolume");
        SetupSlider(sfxSlider,    "SFXVolume",    "SFXVolume");
    }

    void SetupSlider(Slider slider, string prefsKey, string mixerParam)
    {
        if (slider == null) return;

        float saved = PlayerPrefs.GetFloat(prefsKey, 1f);
        slider.value = saved;
        WendeLautstaerkeAn(mixerParam, saved);
        slider.onValueChanged.AddListener(value =>
        {
            WendeLautstaerkeAn(mixerParam, value);
            PlayerPrefs.SetFloat(prefsKey, value);
            PlayerPrefs.Save();
        });
    }

    void WendeLautstaerkeAn(string mixerParam, float value)
    {
        if (audioMixer != null)
            audioMixer.SetFloat(mixerParam, value > 0 ? Mathf.Log10(value) * 20 : -80f);
    }
}
