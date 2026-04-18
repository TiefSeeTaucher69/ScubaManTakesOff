using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Diagnostics;
using System.IO;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Michsky.LSS;

public class BootUpdateManager : MonoBehaviour
{
    [Header("Version Settings")]
    private string currentVersion;
    public string apiUrl = "https://api.github.com/repos/TiefSeeTaucher69/ScubaManTakesOff/releases/latest";

    [Header("UI")]
    public GameObject updatePanel;       // Panel mit Buttons und Info
    public TMPro.TMP_Text updateText;
    public Button updateButton;
    public Button skipButton;
    public TMPro.TMP_Text releaseNotesText; // Im Inspector zuweisen

    [Header("Loading")]
    public GameObject loadingPanel;
    public TMPro.TMP_Text loadingStatusText;

    private string installerUrl = "";
    private string installerFilePath = "";
    private static readonly WaitForSeconds waitHalfSecond = new(0.5f);

    async void Start()
    {
        UnityEngine.Debug.Log("Current build: " + Application.version);
        SetQualitySettings();

        currentVersion = "v" + Application.version;
        LSS_LoadingScreen.presetName = "Standard";
        updatePanel.SetActive(false);
        if (loadingPanel != null) loadingPanel.SetActive(true);
        SetLoadingStatus("Connecting...");

        await InitializeUnityServicesAsync();

        StartCoroutine(CheckForUpdate());
    }

    private async System.Threading.Tasks.Task InitializeUnityServicesAsync()
    {
        try
        {
            await UnityServices.InitializeAsync();

            bool linked = PlayerPrefs.GetInt("PlayerAccountsLinked", 0) == 1;

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                if (AuthenticationService.Instance.SessionTokenExists && linked)
                {
                    SetLoadingStatus("Signing in...");
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    UnityEngine.Debug.Log("Auto-login via session token. Player ID: " + AuthenticationService.Instance.PlayerId);

                    SetLoadingStatus("Loading save data...");
                    if (CloudSaveManager.Instance != null)
                        await CloudSaveManager.Instance.LoadAllAsync();
                }
            }
            else
            {
                UnityEngine.Debug.Log("Already signed in. Player ID: " + AuthenticationService.Instance.PlayerId);
            }

            SetLoadingStatus("Fetching config...");
            if (RemoteConfigManager.Instance != null)
                await RemoteConfigManager.Instance.FetchAsync();
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogWarning("Unity Services initialization failed (session expired?): " + e.Message);
            try { AuthenticationService.Instance.SignOut(); } catch { }
        }
    }

    private void SetLoadingStatus(string status)
    {
        if (loadingStatusText != null) loadingStatusText.text = status;
    }

    private void SetQualitySettings()
    {
        // Load VSync from PlayerPrefs, default = 0 (off)
        int vsyncSetting = PlayerPrefs.GetInt("VSyncEnabled", 0);
        QualitySettings.vSyncCount = vsyncSetting;

        int resIndex = PlayerPrefs.GetInt("ResolutionIndex", -1);
        if (resIndex != -1)
        {
            Resolution[] resolutions = Screen.resolutions;
            if (resIndex < resolutions.Length)
            {
                Resolution res = resolutions[resIndex];
                Screen.SetResolution(res.width, res.height, FullScreenMode.FullScreenWindow, res.refreshRateRatio);
                UnityEngine.Debug.Log("Resolution loaded from PlayerPrefs: " + res.width + "x" + res.height);
            }
        }
        else
        {
            Resolution nativeRes = Screen.currentResolution;
            Screen.SetResolution(nativeRes.width, nativeRes.height, true);
            UnityEngine.Debug.Log("Native resolution set: " + nativeRes.width + "x" + nativeRes.height);
        }

        int fpsIndex = PlayerPrefs.GetInt("FPSCap", -1);
        if (fpsIndex == -1)
        {
            fpsIndex = 3; // default: 240 FPS
            PlayerPrefs.SetInt("FPSCap", fpsIndex);
            PlayerPrefs.Save();
        }
        int targetFPS = fpsIndex switch
        {
            0 => 30,
            1 => 60,
            2 => 120,
            3 => 240,
            4 => -1,
            _ => 240
        };
        Application.targetFrameRate = targetFPS;
        UnityEngine.Debug.Log("FPS cap set from PlayerPrefs: " + targetFPS + " FPS");
    }

    IEnumerator CheckForUpdate()
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        request.SetRequestHeader("User-Agent", "UnityUpdateChecker");
        yield return request.SendWebRequest();

        if (loadingPanel != null) loadingPanel.SetActive(false);

        if (request.result == UnityWebRequest.Result.Success)
        {
            var json = request.downloadHandler.text;
            UnityEngine.Debug.Log("GitHub API response received: " + json);

            GitHubRelease latest = JsonUtility.FromJson<GitHubRelease>(json);

            UnityEngine.Debug.Log($"Latest version: {latest.tag_name}, Current version: {currentVersion}");

            if (IsNewerVersion(latest.tag_name, currentVersion) && latest.assets != null && latest.assets.Length > 0)
            {
                installerUrl = latest.assets[0].browser_download_url;
                updateText.text = $"A new update ({latest.tag_name}) is available!";
                releaseNotesText.text = latest.body;
                updatePanel.SetActive(true);

                updateButton.onClick.RemoveAllListeners();
                updateButton.onClick.AddListener(() => StartCoroutine(DownloadAndInstall()));

                skipButton.onClick.RemoveAllListeners();
                skipButton.onClick.AddListener(() =>
                {
                    updatePanel.SetActive(false);
                    LoadNextScene();
                });

                UnityEngine.Debug.Log("Update panel shown");
            }
            else
            {
                LoadNextScene();
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("Update-Check failed: " + request.error);
            LoadNextScene();
        }

        request.Dispose();
    }

    IEnumerator DownloadAndInstall()
    {
        updateText.text = "Downloading new version, do NOT close the game manually...";
        string tempPath = Path.Combine(Path.GetTempPath(), "SMTO_UpdateInstaller.exe");
        installerFilePath = tempPath;

        UnityWebRequest request = UnityWebRequest.Get(installerUrl);
        request.downloadHandler = new DownloadHandlerFile(tempPath);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            UnityEngine.Debug.Log("Installer downloaded, starting installation...");
            updateText.text = "Starting installer...";

            // Exit fullscreen so the UAC elevation dialog is visible
            if (Screen.fullScreen)
            {
                Screen.fullScreen = false;
                yield return waitHalfSecond;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = installerFilePath,
                    UseShellExecute = true,
                    Verb = "runas"
                });
                Application.Quit();
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError("Failed to start installer: " + e.Message);
                updateText.text = "Error starting installer:\n" + e.Message;
                Screen.fullScreen = true;
                skipButton.gameObject.SetActive(true);
            }
        }
        else
        {
            UnityEngine.Debug.LogError("Download failed: " + request.error);
            updateText.text = "Download failed. Please check your connection.";
            skipButton.gameObject.SetActive(true);
        }

        request.Dispose();
    }

    private void LoadNextScene()
    {
        bool linked = PlayerPrefs.GetInt("PlayerAccountsLinked", 0) == 1;
        if (AuthenticationService.Instance.IsSignedIn && PlayerPrefs.HasKey("Username") && linked)
        {
            LSS_LoadingScreen.LoadScene("MainMenu");
        }
        else
        {
            LSS_LoadingScreen.LoadScene("FirstOpen");
        }
    }

    [System.Serializable]
    public class GitHubRelease
    {
        public string tag_name;
        public string body;
        public Asset[] assets;
    }

    [System.Serializable]
    public class Asset
    {
        public string browser_download_url;
    }

    private bool IsNewerVersion(string latest, string current)
    {
        latest = latest.TrimStart('v');
        current = current.TrimStart('v');

        System.Version latestVersion, currentVersion;

        if (System.Version.TryParse(latest, out latestVersion) && System.Version.TryParse(current, out currentVersion))
        {
            return latestVersion > currentVersion;
        }

        UnityEngine.Debug.LogWarning("Version comparison failed!");
        return false;
    }

}
