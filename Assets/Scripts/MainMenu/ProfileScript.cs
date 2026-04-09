using System;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
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
            PlayerPrefs.Save();
            RefreshProfile();
            renamePanel.SetActive(false);
            usernameDisplay.SetActive(true);
            if (txtFeedback != null) txtFeedback.text = "";
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to rename: " + e.Message);
            if (txtFeedback != null) txtFeedback.text = "Error: " + e.Message;
        }
    }
}
