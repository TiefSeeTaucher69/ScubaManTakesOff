using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;

/// <summary>
/// Wird in der FirstOpen-Szene verwendet.
/// Zeigt einen Login/Registrieren-Button, der die Unity Player Accounts
/// gehostete Seite im Browser öffnet (Google, E-Mail/Passwort etc.).
/// Nach dem ersten Login erscheint ein Username-Eingabefeld.
/// </summary>
public class AuthScript : MonoBehaviour
{
    [Header("Auth-Panel")]
    [SerializeField] private GameObject authPanel;
    [SerializeField] private Button loginButton;
    [SerializeField] private TMPro.TMP_Text feedbackText;

    [Header("Username-Panel (nur erstes Mal)")]
    [SerializeField] private GameObject usernamePanel;
    [SerializeField] private TMPro.TMP_InputField usernameInput;
    [SerializeField] private TMPro.TMP_Text usernameFeedback;

    async void Start()
    {
        authPanel.SetActive(true);
        usernamePanel.SetActive(false);

        if (UnityServices.State != ServicesInitializationState.Initialized)
            await UnityServices.InitializeAsync();

        if (AuthenticationService.Instance.IsSignedIn)
            await AfterLoginAsync();
    }

    /// <summary>Wird vom Login-Button aufgerufen.</summary>
    public async void OnLoginClick()
    {
        loginButton.interactable = false;
        feedbackText.text = "Browser wird geöffnet...";

        // Spiel muss weiterlaufen während Browser offen ist,
        // sonst verarbeitet Unity den HttpListener-Callback nicht.
        Application.runInBackground = true;

        // Events VOR StartSignInAsync subscriben:
        // StartSignInAsync() wartet nur auf den Auth-Code-Redirect (Browser-Callback).
        // Der Token-Austausch passiert danach asynchron → SignedIn feuert erst NACH
        // dem Task-Completion. Wenn wir erst danach subscriben, verpassen wir das Event.
        var tcs = new TaskCompletionSource<bool>();
        bool tokenFailed = false;
        string tokenError = "";

        void OnSignedIn() => tcs.TrySetResult(true);
        void OnFailed(RequestFailedException ex)
        {
            tokenFailed = true;
            tokenError = ex.Message;
            tcs.TrySetResult(false);
        }

        PlayerAccountService.Instance.SignedIn     += OnSignedIn;
        PlayerAccountService.Instance.SignInFailed += OnFailed;

        try
        {
            feedbackText.text = "Warte auf Browser-Login...";
            await PlayerAccountService.Instance.StartSignInAsync();
        }
        catch (Exception ex)
        {
            PlayerAccountService.Instance.SignedIn     -= OnSignedIn;
            PlayerAccountService.Instance.SignInFailed -= OnFailed;
            Debug.LogError("[Auth] StartSignIn Fehler: " + ex.Message);
            feedbackText.text = "Login fehlgeschlagen: " + ex.Message;
            loginButton.interactable = true;
            return;
        }

        // Auth-Code empfangen – warte jetzt auf Token-Austausch (SignedIn-Event)
        feedbackText.text = "Verbinde...";
        await Task.WhenAny(tcs.Task, Task.Delay(30000));

        PlayerAccountService.Instance.SignedIn     -= OnSignedIn;
        PlayerAccountService.Instance.SignInFailed -= OnFailed;

        if (tokenFailed)
        {
            feedbackText.text = "Token-Fehler: " + tokenError;
            loginButton.interactable = true;
            return;
        }

        if (!tcs.Task.IsCompleted || !PlayerAccountService.Instance.IsSignedIn)
        {
            feedbackText.text = "Login Timeout – bitte erneut versuchen.";
            loginButton.interactable = true;
            return;
        }

        // Token vorhanden → Unity Authentication einloggen
        feedbackText.text = "Login erfolgreich, verbinde...";
        try
        {
            await AuthenticationService.Instance.SignInWithUnityAsync(
                PlayerAccountService.Instance.AccessToken);

            PlayerPrefs.SetInt("PlayerAccountsLinked", 1);
            PlayerPrefs.Save();

            await AfterLoginAsync();
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError("[Auth] Authentication Fehler: " + ex.Message);
            feedbackText.text = "Verbindung fehlgeschlagen: " + ex.Message;
            loginButton.interactable = true;
        }
        catch (Exception ex)
        {
            Debug.LogError("[Auth] Unbekannter Fehler: " + ex.Message);
            feedbackText.text = "Fehler: " + ex.Message;
            loginButton.interactable = true;
        }
    }

    private async Task AfterLoginAsync()
    {
        if (CloudSaveManager.Instance != null)
            await CloudSaveManager.Instance.LoadAllAsync();

        if (RemoteConfigManager.Instance != null)
            await RemoteConfigManager.Instance.FetchAsync();

        if (!PlayerPrefs.HasKey("Username"))
        {
            authPanel.SetActive(false);
            usernamePanel.SetActive(true);
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    /// <summary>Wird vom "Speichern"-Button im Username-Panel aufgerufen.</summary>
    public async void OnSaveUsernameClick()
    {
        string name = usernameInput.text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            usernameFeedback.text = "Name darf nicht leer sein.";
            return;
        }

        usernameFeedback.text = "Wird gespeichert...";

        try
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(name);

            PlayerPrefs.SetString("Username", name);
            if (CloudSaveManager.Instance != null)
                CloudSaveManager.Instance.SaveString("Username", name);

            Debug.Log("[Auth] Username gespeichert: " + name);
            SceneManager.LoadScene("MainMenu");
        }
        catch (Exception e)
        {
            Debug.LogError("[Auth] Fehler beim Setzen des Namens: " + e.Message);
            usernameFeedback.text = "Fehler: " + e.Message;
        }
    }
}
