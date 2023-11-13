using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SimpleAPI
{
    #region utils

    public static class ExtensionMethods //from https://gist.github.com/mattyellen/d63f1f557d08f7254345bff77bfdc8b3
    {
        public static TaskAwaiter GetAwaiter(this AsyncOperation asyncOp)
        {
            var tcs = new TaskCompletionSource<object>();
            asyncOp.completed += obj => { tcs.SetResult(null); };
            return ((Task)tcs.Task).GetAwaiter();
        }
    }

    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.Items;
        }

        public static string ToJson<T>(T[] array)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper);
        }

        public static string ToJson<T>(T[] array, bool prettyPrint)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        public static string fixJson(string value)
        {
            value = "{\"Items\":" + value + "}";
            return value;
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }

    }

    public class RecordsList<T>
    {
        public List<T> records = new List<T>();
    }

    //public delegate void Callback<T>(T item);
    //public delegate void ListCallback<T>(List<T> item);
    //public delegate void StringCallback(string item);

    #endregion

    public enum method { GET, PUT, POST, DELETE }

    public class API : MonoBehaviour
    {
        public static UnityWebRequest Request<T>(method method, string URL, T data)
        {
            switch (method)
            {
                case method.GET:
                    var get = UnityWebRequest.Get(URL);
                    if (GetToken() != null) get.SetRequestHeader("Authorization", "Bearer " + GetToken());
                    return get;

                case method.POST:
                    var post = UnityWebRequest.Put(URL, JsonUtility.ToJson(data));

                    post.method = UnityWebRequest.kHttpVerbPOST; //from https://manuelotheo.com/uploading-raw-json-data-through-unitywebrequest/
                    post.SetRequestHeader("Content-Type", "application/json");
                    post.SetRequestHeader("Accept", "application/json");
                    if (GetToken() != null) post.SetRequestHeader("Authorization", "Bearer " + GetToken());
                    return post;

                case method.PUT:
                    byte[] putArr = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
                    var put = UnityWebRequest.Put(URL, putArr);
                    put.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");
                    if (GetToken() != null) put.SetRequestHeader("Authorization", "Bearer " + GetToken());
                    return put;

                case method.DELETE:
                    var del = UnityWebRequest.Delete(URL);
                    if (GetToken() != null) del.SetRequestHeader("Authorization", "Bearer " + GetToken());
                    return del;

                default:
                    var def = UnityWebRequest.Get(URL);
                    if (GetToken() != null) def.SetRequestHeader("Authorization", "Bearer " + GetToken());
                    return def;
            }
        }

        #region auth

        public static void SetToken(string token)
        {
            PlayerPrefs.SetString("TOKEN", token);
        }

        public static string GetToken()
        {
            if (PlayerPrefs.HasKey("TOKEN"))
                return PlayerPrefs.GetString("TOKEN");
            else
                return null;
        }

        public static void RemoveToken()
        {
            PlayerPrefs.DeleteKey("TOKEN");
        }

        #endregion


        #region awaitable

        //GET
        public static async Task<T> Get<T>(string URL)
        {
            var req = Request<T>(method.GET, URL, default(T));

            await req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning("Could not GET " + URL + " " + req.responseCode);
                return default(T);
            }
            else
            {
                var res = JsonUtility.FromJson<T>(req.downloadHandler.text);
                return res;
            }
        }
        //GET ARRAY
        public static async Task<List<T>> GetArray<T>(string URL)
        {
            var req = Request<T>(method.GET, URL, default(T));

            await req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning("Could not GET Array at " + URL + " " + req.responseCode);
                return default(List<T>);
            }
            else
            {
                var records = new RecordsList<T>();
                var fixedJson = JsonHelper.fixJson(req.downloadHandler.text);
                records.records = JsonHelper.FromJson<T>(fixedJson).ToList();
                return records.records;
            }
        }

        //POST
        public static async Task<T2> Post<T1, T2>(string URL, T1 data)
        {
            var req = Request(method.POST, URL, data);
            await req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.ProtocolError)
            {
                //Debug.LogWarning("Could not POST at " + URL + " " + req.responseCode);
                return default(T2);
            }
            else
            {
                T2 res = JsonUtility.FromJson<T2>(req.downloadHandler.text);
                return res;
            }
        }

        public static async Task<T> Post<T>(string URL, T data)
        {
            var req = Request(method.POST, URL, data);
            await req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning("Could not POST at " + URL + " " + req.responseCode);
                return default(T);
            }
            else
            {
                T res = JsonUtility.FromJson<T>(req.downloadHandler.text);
                return res;
            }
        }

        //PUT
        public static async Task<T> Put<T>(string URL, T data)
        {
            var req = Request(method.PUT, URL, data);
            await req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning("Could not PUT at " + URL + " " + req.responseCode);
                return default(T);
            }
            else
            {
                var res = JsonUtility.FromJson<T>(req.downloadHandler.text);
                return res;
            }
        }

        public static async Task<T2> Put<T1, T2>(string URL, T1 data)
        {
            var req = Request(method.PUT, URL, data);
            await req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning("Could not PUT at " + URL + " " + req.responseCode);
                return default(T2);
            }
            else
            {
                var res = JsonUtility.FromJson<T2>(req.downloadHandler.text);
                return res;
            }
        }
        //DELETE
        public static async Task<string> Delete(string URL)
        {
            var req = Request<string>(method.DELETE, URL, null);
            await req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning("Could not DELETE " + URL + " " + req.responseCode);
                return null;
            }
            else
            {
                return req.downloadHandler.text;
            }
        }

        #endregion

        #region media getters

        public static async Task<Texture2D> GetTexture(string URL)
        {
            var req = UnityWebRequestTexture.GetTexture(URL);
            if (GetToken() != null) req.SetRequestHeader("Authorization", "Bearer " + GetToken());

            await req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning("Could not GET " + URL + " " + req.responseCode);
                return null;
            }
            else
            {
                Texture2D res = ((DownloadHandlerTexture)req.downloadHandler).texture;
                return res;
            }
        }

        public static async Task<AudioClip> GetAudio(string URL, AudioType type)
        {
            var req = UnityWebRequestMultimedia.GetAudioClip(URL, type);
            if (GetToken() != null) req.SetRequestHeader("Authorization", "Bearer " + GetToken());

            await req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning("Could not GET " + URL + " " + req.responseCode);
                return null;
            }
            else
            {
                AudioClip res = ((DownloadHandlerAudioClip)req.downloadHandler).audioClip;
                if (res.length > 0)
                {
                    while (!res.LoadAudioData())
                        await Task.Delay(1);
                    return res;
                }
                return null;
            }
        }

        public static async Task<AssetBundle> GetAssetbundle(string URL)
        {
            var req = UnityWebRequestAssetBundle.GetAssetBundle(URL);
            if (GetToken() != null) req.SetRequestHeader("Authorization", "Bearer " + GetToken());

            await req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning("Could not GET " + URL + " " + req.responseCode);
                return null;
            }
            else
            {
                AssetBundle res = ((DownloadHandlerAssetBundle)req.downloadHandler).assetBundle;
                return res;
            }
        }

        #endregion
    }
}

