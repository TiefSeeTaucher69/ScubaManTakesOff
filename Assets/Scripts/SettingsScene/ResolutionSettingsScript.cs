using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResolutionSettingsScript : MonoBehaviour
{
    public TMPro.TMP_Dropdown resolutionDropdown;
    public Toggle vsyncToggle;
    Resolution[] resolutions;

    void Start()
    {
        // VSync laden und setzen
        int vsyncSetting = PlayerPrefs.GetInt("VSyncEnabled", 0);
        QualitySettings.vSyncCount = vsyncSetting;
        vsyncToggle.isOn = vsyncSetting == 1;
        vsyncToggle.onValueChanged.AddListener(OnVSyncToggleChanged);

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
                resolutions[i].refreshRateRatio.numerator / resolutions[i].refreshRateRatio.denominator == Screen.currentResolution.refreshRate)
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
