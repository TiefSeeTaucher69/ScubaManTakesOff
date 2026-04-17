using System;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProfileScript : MonoBehaviour
{
    [Header("Profile")]
    [SerializeField] private TMP_Text txtUsername;
    [SerializeField] private TMP_Text txtAvatar;
    [SerializeField] private GameObject usernameDisplay;
    [SerializeField] private GameObject renamePanel;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text txtFeedback;

    [Header("Statistics")]
    [SerializeField] private TMP_Text txtQuickplay;
    [SerializeField] private TMP_Text txtRanked;
    [SerializeField] private TMP_Text txtTotalScore;
    [SerializeField] private TMP_Text txtTotalRuns;
    [SerializeField] private TMP_Text txtAvgScore;

    [Header("XP / Level")]
    [SerializeField] private TMP_Text Txt_ProfileLevel;
    [SerializeField] private Image    Img_ProfileXPBar;
    [SerializeField] private TMP_Text Txt_ProfileXPProgress;

    void OnEnable()
    {
        RefreshProfile();
        RefreshStats();
        renamePanel.SetActive(false);
        usernameDisplay.SetActive(true);
        if (txtFeedback != null) txtFeedback.text = "";
    }

    private void RefreshProfile()
    {
        string name = PlayerPrefs.GetString("Username", "Guest");
        txtUsername.text = name;
        txtAvatar.text = name.Length > 0 ? name[0].ToString().ToUpper() : "?";
    }

    private void RefreshStats()
    {
        int runs = PlayerPrefs.GetInt("TotalRuns", 0);
        int total = PlayerPrefs.GetInt("TotalScore", 0);
        txtQuickplay.text = PlayerPrefs.GetInt("Highscore", 0).ToString();
        txtRanked.text = PlayerPrefs.GetInt("RankedHighscore", 0).ToString();
        txtTotalScore.text = total.ToString("N0");
        txtTotalRuns.text = runs.ToString();
        txtAvgScore.text = runs > 0 ? (total / runs).ToString() : "\u2014";

        int totalXP = PlayerPrefs.GetInt("TotalXP", 0);
        int level   = XPManager.GetLevel(totalXP);

        if (Txt_ProfileLevel != null)
            Txt_ProfileLevel.text = $"Lv. {level}";

        if (Img_ProfileXPBar != null)
            Img_ProfileXPBar.fillAmount = level >= XPManager.MaxLevel ? 1f
                : (XPManager.GetXPRequired(totalXP) > 0
                    ? Mathf.Clamp01((float)XPManager.GetXPInLevel(totalXP) / XPManager.GetXPRequired(totalXP))
                    : 0f);

        if (Txt_ProfileXPProgress != null)
            Txt_ProfileXPProgress.text = level >= XPManager.MaxLevel ? "MAX LEVEL"
                : $"{XPManager.GetXPInLevel(totalXP):N0} / {XPManager.GetXPRequired(totalXP):N0} XP";
    }

    public void OnRenameClick()
    {
        inputField.text = PlayerPrefs.GetString("Username", "");
        usernameDisplay.SetActive(false);
        renamePanel.SetActive(true);
        inputField.Select();
        if (txtFeedback != null) txtFeedback.text = "";
    }

    public void OnCancelClick()
    {
        renamePanel.SetActive(false);
        usernameDisplay.SetActive(true);
        if (txtFeedback != null) txtFeedback.text = "";
    }

    public async void OnSaveClick()
    {
        string newName = inputField.text.Trim();
        if (string.IsNullOrWhiteSpace(newName))
        {
            if (txtFeedback != null) txtFeedback.text = "Name cannot be empty.";
            return;
        }

        if (txtFeedback != null) txtFeedback.text = "Saving...";

        try
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(newName);
            PlayerPrefs.SetString("Username", newName);
            CloudSaveManager.Instance.SaveString("Username", newName);
            RefreshProfile();
            renamePanel.SetActive(false);
            usernameDisplay.SetActive(true);
            if (txtFeedback != null) txtFeedback.text = "";
            ToastManager.Show($"Username set to \"{newName}\"", ToastType.Success);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to rename: " + e.Message);
            if (txtFeedback != null) txtFeedback.text = "Error: " + e.Message;
            ToastManager.Show("Failed to update username.", ToastType.Warning);
        }
    }

    public void OnLogoutClick()
    {
        AuthenticationService.Instance.SignOut();
        try { PlayerAccountService.Instance.SignOut(); } catch { }
        PlayerPrefs.DeleteKey("Username");
        PlayerPrefs.DeleteKey("PlayerAccountsLinked");
        PlayerPrefs.Save();
        SceneManager.LoadScene("FirstOpen");
    }
}
