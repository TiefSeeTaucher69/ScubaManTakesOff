using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoSettingsInGameScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TMPro.TMP_Dropdown fpsDropdown; // Dropdown f�r FPS Cap
    public const string PlayerPrefsKey = "FPSCap"; // Schl�ssel f�r PlayerPrefs
    public TMPro.TMP_Dropdown resolutionDropdown;
    public Toggle vsyncToggle;
    Resolution[] resolutions;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // VSync laden und setzen
        int vsyncSetting = PlayerPrefs.GetInt("VSyncEnabled", 0); 
        QualitySettings.vSyncCount = vsyncSetting;
        vsyncToggle.isOn = vsyncSetting == 1;
        vsyncToggle.onValueChanged.AddListener(OnVSyncToggleChanged);

        // Gespeicherte Einstellung laden, -1 bedeutet kein Eintrag
        int savedIndex = PlayerPrefs.GetInt(PlayerPrefsKey, -1);

        if (savedIndex == -1)
        {
            savedIndex = 3; // default: 240 FPS (matches BootScene default)
            PlayerPrefs.SetInt(PlayerPrefsKey, savedIndex);
            PlayerPrefs.Save();
        }

        // Listener tempor�r entfernen, damit beim Setzen des Werts kein Event feuert
        fpsDropdown.onValueChanged.RemoveAllListeners();

        fpsDropdown.value = savedIndex;

        ApplySetting(savedIndex);

        // Listener wieder hinzuf�gen
        fpsDropdown.onValueChanged.AddListener(OnDropdownChanged);



        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        int currentResIndex = 0;
        var options = new List<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRateRatio.numerator / resolutions[i].refreshRateRatio.denominator + "Hz";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height &&
                resolutions[i].refreshRateRatio.numerator / resolutions[i].refreshRateRatio.denominator == Screen.currentResolution.refreshRateRatio.numerator / Screen.currentResolution.refreshRateRatio.denominator)
            {
                currentResIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex", currentResIndex);
        resolutionDropdown.RefreshShownValue();

        ApplyResolution(resolutionDropdown.value);

        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDropdownChanged(int index)
    {
        ApplySetting(index);
        PlayerPrefs.SetInt(PlayerPrefsKey, index);
        PlayerPrefs.Save();
    }

    void ApplySetting(int index)
    {
        switch (index)
        {
            case 0:
                Application.targetFrameRate = 30;
                break;
            case 1:
                Application.targetFrameRate = 60;
                break;
            case 2:
                Application.targetFrameRate = 120;
                break;
            case 3:
                Application.targetFrameRate = 240;
                break;
            case 4:
                Application.targetFrameRate = -1; // unbegrenzt
                break;
            default:
                Application.targetFrameRate = 240; // Fallback unbegrenzt
                break;
        }
    }

    public void OnResolutionChanged(int index)
    {
        ApplyResolution(index);
        PlayerPrefs.SetInt("ResolutionIndex", index);
        PlayerPrefs.Save();
    }

    public void OnVSyncToggleChanged(bool isOn)
    {
        QualitySettings.vSyncCount = isOn ? 1 : 0;
        Debug.Log("VSync " + (isOn ? "aktiviert" : "deaktiviert"));
        PlayerPrefs.SetInt("VSyncEnabled", isOn ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("VSync " + (isOn ? "aktiviert" : "deaktiviert"));
    }

    void ApplyResolution(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode, res.refreshRateRatio);
        Debug.Log("Aufl�sung gesetzt auf: " + res.width + "x" + res.height);
    }
}
