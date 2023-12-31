using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MADD;

public class GameController : MonoBehaviour
{
    #region member variables

    public int _score = 0;
    public GameObject _configPanel, _gamePanel, _leaderboardsContainer, _leaderboardEntryPrefab, _connectEmailBtn;
    public TextMeshProUGUI _scoreCounter, _playerUsername, _leaderboardName;
    public TMP_InputField _usernameInput, _emailInput;

    #endregion

    #region event hooks

    private void OnEnable()
    {
        BackendManager.Instance.OnLoginSuccessful += InitLeaderboards;
        BackendManager.Instance.OnLeaderboardUpdated += UpdateLeaderboardsUI;

        BackendManager.Instance.OnProfileUpdated += UpdateUserUI;
        BackendManager.Instance.OnEmailConnected += EmailConnected;

        BackendManager.Instance.OnLoginSuccessful += SetupGameUI;
        BackendManager.Instance.OnProfileUpdated += SetupGameUI;
    }

    private void OnDisable()
    {
        if (!BackendManager.Quitting)
        {
            BackendManager.Instance.OnLoginSuccessful -= InitLeaderboards;
            BackendManager.Instance.OnLeaderboardUpdated -= UpdateLeaderboardsUI;

            BackendManager.Instance.OnProfileUpdated -= UpdateUserUI;
            BackendManager.Instance.OnEmailConnected -= EmailConnected;

            BackendManager.Instance.OnProfileUpdated -= SetupGameUI;
            BackendManager.Instance.OnLoginSuccessful -= SetupGameUI;
        }
    }

    #endregion

    #region panels management

    private void SetupGameUI()
    {
        if (BackendManager.Instance._profile.username == "")
        {
            _configPanel.SetActive(true);
            _gamePanel.SetActive(false);
        }
        else
        {
            UpdateUserUI();
            _configPanel.SetActive(false);
            _gamePanel.SetActive(true);
        }
    }

    #endregion

    #region player management

    public void UpdateUsername()
    {
        string newUsername = _usernameInput.text;
        BackendManager.Instance.UpdateUsername(newUsername);
    }

    private void UpdateUserUI()
    {
        _playerUsername.text = BackendManager.Instance._profile.username;
        bool emailIsPresent = BackendManager.Instance._profile.email.Length > 0;
        _connectEmailBtn.SetActive(!emailIsPresent);
    }

    public void UpdateEmail()
    {
        string newEmail = _emailInput.text;
        BackendManager.Instance.ConnectAccount(newEmail);
    }

    private void EmailConnected()
    {
        Debug.Log("Email has been connected, give the user a prize!");
        BackendManager.Instance.TrackMilestone("email_connected");
    }

    #endregion

    #region leaderboards management

    private void InitLeaderboards()
    {
        BackendManager.Instance.GetLeaderboards();

    }

    private void UpdateLeaderboardsUI()
    {
        Leaderboard talksBoard = BackendManager.Instance.GetLeaderboard("backend talk");
        // update name
        _leaderboardName.text = talksBoard.name;
        // clear children
        TextMeshProUGUI[] children = _leaderboardsContainer.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (TextMeshProUGUI child in children) Destroy(child.gameObject);
        // sort by score
        talksBoard.scores.Sort( (score1, score2) => score1.value - score2.value);
        // create new entries
        talksBoard.scores.ForEach(score =>
        {
            GameObject entryGo = Instantiate(_leaderboardEntryPrefab, _leaderboardsContainer.transform);
            entryGo.GetComponent<TextMeshProUGUI>().text = score.value.ToString() + " - " + score.username;
        });
    }

    public void IncreaseScore()
    {
        _score++;
        _scoreCounter.text = _score.ToString();

        // track a milestone
        if (_score >= 50) BackendManager.Instance.TrackMilestone("over_50");
        // NOTE: this is not a good example of milestones as the score can keep increasing a lot and each time it gets posted to the backend
        // A better example would be with the email, like it's done above
    }

    public void SubmitScore()
    {
        BackendManager.Instance.SubmitScore("backend talk", _score);
    }

    #endregion
}
