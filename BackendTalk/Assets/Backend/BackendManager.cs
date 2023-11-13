using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using SimpleAPI;
using System;

namespace MADD
{
    public class BackendManager : Singleton<BackendManager>
    {
        #region member variables

        [Header("Server Data and Status")]
        public string _serverUrl = "http://localhost:1337/api";
        public bool LoggedIn = false;
        [Header("Player and Leaderboards Management")]
        public Profile _profile;
        public List<Leaderboard> _leaderboards;

        public Action OnLoginSuccessful, OnLoginFailed, OnLogout, OnProfileUpdated, OnEmailConnected, OnLeaderboardUpdated;

        #endregion

        async void Start()
        {

            Logout();
            await AttemptLogin();

        }

        #region login and account management

        private async Task<bool> AttemptLogin()
        {
            var succ = await Login(); //this will always succeed unless there's a network problem
            if (!succ)
            {
                OnLoginFailed.Invoke();
                LoggedIn = false;
                return false;
            }
            else
            {
                OnLoginSuccessful?.Invoke();
                LoggedIn = true;
                return true;
            }
        }

        private async Task<bool> Login()
        {
            LoginData body = new LoginData()
            {
                deviceID = GetDeviceID()
            };

            var res = await API.Post(_serverUrl + "/login", body);
            if (res != null)
            {
                // all good, we set the auth token and cache the profile
                API.SetToken(res.jwt);
                _profile = res.player;
                Debug.Log("Logged in!");
                return true;
            }
            else
            {
                Debug.LogWarning("Could Not Login with Device ID");
                return false;

            }
        }

        public void Logout()
        {
            API.RemoveToken();
            OnLogout?.Invoke();
        }

        public async Task UpdateUsername(string username)
        {
            UpdateUsernameData body = new UpdateUsernameData()
            {
                username = username
            };
            Profile updatedUser = await API.Post<UpdateUsernameData, Profile>(_serverUrl + "/update-username", body);
            _profile = updatedUser;
            OnProfileUpdated?.Invoke();
        }

        public async Task<string> ConnectAccount(string email)
        {
            if (!IsValidEmail(email))
            {
                return "invalid_email";
            }

            MailAssociationData body = new MailAssociationData()
            {

                deviceID = GetDeviceID(),
                email = email
            };

            QueryResult res = await API.Post<MailAssociationData, QueryResult>(_serverUrl + "/update-email", body);
            if (res.status == "success")
            {
                OnProfileUpdated?.Invoke();
                OnEmailConnected?.Invoke();
                return "success";
            }
            else
            {
                Debug.LogWarning(res.status);
                return "failure";
            }
        }

        public async void TrackMilestone(string milestoneName)
        {
            QueryResult body = new QueryResult()
            {
                payload = milestoneName
            };
            QueryResult res = await API.Post<QueryResult>(_serverUrl + "/track-milestone", body);
            if (res == null || res.status != "success")
                Debug.LogWarning(res.status);
        }

        #endregion

        #region leaderboards management

        public async Task<List<Leaderboard>> GetLeaderboards()
        {
            List<Leaderboard> leaderboards = await API.GetArray<Leaderboard>(_serverUrl + "/leaderboards");
            _leaderboards = leaderboards;
            OnLeaderboardUpdated?.Invoke();
            return leaderboards;
        }

        public Leaderboard GetLeaderboard(string name)
        {
            return _leaderboards.Find(x => x.name == name);
        }

        public async Task SubmitScore(string leaderboardName, int score)
        {
            Leaderboard leaderboard = GetLeaderboard(leaderboardName);
            ScoreData body = new ScoreData()
            {
                value = score
            };
            // update score and receive the updated leaderboard
            Leaderboard newLeaderboardStatus = await API.Put<ScoreData, Leaderboard>(_serverUrl + "/leaderboards/" + leaderboard.id, body);
            print("Leaderboard " + newLeaderboardStatus.id + " Updated!");
            // update local version
            int index = _leaderboards.FindIndex(x => x.id == newLeaderboardStatus.id);
            _leaderboards[index] = newLeaderboardStatus;
            // notify subs
            OnLeaderboardUpdated?.Invoke();
        }

        #endregion

        #region utils

        private string GetDeviceID()
        {
            string deviceID = PlayerPrefs.GetString("DID", "NOPE");
            if (deviceID == "NOPE")
            {
                deviceID = SystemInfo.deviceUniqueIdentifier;
                PlayerPrefs.SetString("DID", deviceID);
            }
            return deviceID;
        }

        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}