
using UnityEngine;
using System.Collections.Generic;

namespace MADD
{
    [System.Serializable]
    public class QueryResult
    {
        public string payload;
        public string status;
    }

    [System.Serializable]
    public class LoginData
    {
        public string deviceID;
        public string username;
        public string email;
        public string password;
        public string jwt;
        public Profile player;
    }

    [System.Serializable]
    public class UpdateUsernameData
    {
        public string username;
        public Profile user;
    }

    [System.Serializable]
    public class MailAssociationData
    {
        public string deviceID;
        public string email;
        public string status;
        public Profile user;
    }

    [System.Serializable]
    public class Profile
    {
        [HideInInspector]
        public int id;
        public string username;
        public string email;
        public int currency;
        public List<Score> scores;
        public string deviceID;
    }

    [System.Serializable]
    public class Leaderboard
    {
        public int id;
        public string name;
        public List<Score> scores;
    }

    [System.Serializable]
    public class Score
    {
        public string username;
        public int value;
    }

    [System.Serializable]
    public class ScoreData
    {
        public int value;
        public Leaderboard leaderboard;
    }
}