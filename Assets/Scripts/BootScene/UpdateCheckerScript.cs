using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using System.IO;
using Unity.Services.Core;
using Unity.Services.Authentication;

public class BootUpdateManager : MonoBehaviour
{
    [Header("Versionseinstellungen")]
    private string currentVersion;
    public string apiUrl = "https://api.github.com/repos/TiefSeeTaucher69/FlappySteff/releases/latest";

    [Header("UI")]
    public GameObject updatePanel;       // Panel mit Buttons und Info
    public TMPro.TMP_Text updateText;
    public Button updateButton;
    public Button skipButton;
    public TMPro.TMP_Text releaseNotesText; // Im Inspector zuweisen

    private string installerUrl = "";
    private string installerFilePath = "";

    async void Start()
    {
        UnityEngine.Debug.Log("Aktueller Build: " + Application.version);
        SetQualitySettings();

        currentVersion = "v" + Application.version;
        updatePanel.SetActive(false);

        await InitializeUnityServicesAsync();

        StartCoroutine(CheckForUpdate());
    }

    private async System.Threading.Tasks.Task InitializeUnityServicesAsync()
    {
        try
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            UnityEngine.Debug.Log("Unity Services bereit. Player ID: " + AuthenticationService.Instance.PlayerId);
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("Unity Services Initialisierung fehlgeschlagen: " + e.Message);
        }
    }

    private void SetQualitySettings()
    {
        // VSync aus PlayerPrefs laden, Standard = 0 (aus)
        int vsyncSetting = PlayerPrefs.GetInt("VSyncEnabled", 0);
        QualitySettings.vSyncCount = vsyncSetting;

        int resIndex = PlayerPrefs.GetInt("ResolutionIndex", -1);
        if (resIndex != -1)
        {
            Resolution[] resolutions = Screen.resolutions;
            if (resIndex < resolutions.Length)
            {
                Resolution res = resolutions[resIndex];
                Screen.SetResolution(res.width, res.height, FullScreenMode.FullScreenWindow, res.refreshRate);
                UnityEngine.Debug.Log("Aufl�sung geladen aus PlayerPrefs: " + res.width + "x" + res.height);
            }
        }
        else
        {
            Resolution nativeRes = Screen.currentResolution;
            Screen.SetResolution(nativeRes.width, nativeRes.height, true);
            UnityEngine.Debug.Log("Native Aufl�sung gesetzt: " + nativeRes.width + "x" + nativeRes.height);
        }

        int fpsIndex = PlayerPrefs.GetInt("FPSCap", 3);
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
        UnityEngine.Debug.Log("FPS Cap aus PlayerPrefs gesetzt auf: " + targetFPS + " FPS");
    }

    IEnumerator CheckForUpdate()
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        request.SetRequestHeader("User-Agent", "UnityUpdateChecker");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var json = request.downloadHandler.text;
            UnityEngine.Debug.Log("GitHub API Antwort erhalten: " + json);

            GitHubRelease latest = JsonUtility.FromJson<GitHubRelease>(json);

            UnityEngine.Debug.Log($"Neueste Version: {latest.tag_name}, Aktuelle Version: {currentVersion}");

            if (IsNewerVersion(latest.tag_name, currentVersion) && latest.assets.Length > 0)
            {
                installerUrl = latest.assets[0].browser_download_url;
                updateText.text = $"Ein neues Update ({latest.tag_name}) ist verf�gbar!";
                releaseNotesText.text = latest.body; // Release Notes anzeigen
                updatePanel.SetActive(true);

                updateButton.onClick.RemoveAllListeners();
                updateButton.onClick.AddListener(() => StartCoroutine(DownloadAndInstall()));

                skipButton.onClick.RemoveAllListeners();
                skipButton.onClick.AddListener(() =>
                {
                    updatePanel.SetActive(false);
                    LoadNextScene();
                });

                UnityEngine.Debug.Log("Update-Panel angezeigt");
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
        updateText.text = "L�dt neue Version, Spiel NICHT manuell schlie�en...";
        string tempPath = Path.Combine(Application.persistentDataPath, "SMTO_UpdateInstaller.exe");
        installerFilePath = tempPath;

        UnityWebRequest request = UnityWebRequest.Get(installerUrl);
        request.downloadHandler = new DownloadHandlerFile(tempPath);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            UnityEngine.Debug.Log("Installer heruntergeladen, starte Installation...");

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = installerFilePath,
                    UseShellExecute = true
                });
                Application.Quit();
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError("Installer konnte nicht gestartet werden: " + e.Message);
                updateText.text = "Fehler beim Starten des Installers:\n" + e.Message;
                skipButton.gameObject.SetActive(true);
            }
        }
        else
        {
            UnityEngine.Debug.LogError("Download fehlgeschlagen: " + request.error);
            updateText.text = "Download fehlgeschlagen. Bitte Verbindung prüfen.";
            skipButton.gameObject.SetActive(true);
        }

        request.Dispose();
    }

    private void LoadNextScene()
    {
        if (PlayerPrefs.HasKey("Username"))
        {
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            SceneManager.LoadScene("FirstOpen");
        }
    }

    [System.Serializable]
    public class GitHubRelease
    {
        public string tag_name;
        public string body; // Release-Beschreibung
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

        UnityEngine.Debug.LogWarning("Versionsvergleich fehlgeschlagen!");
        return false;
    }

}
