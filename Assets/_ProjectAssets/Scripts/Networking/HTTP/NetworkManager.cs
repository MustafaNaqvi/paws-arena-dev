using Anura.ConfigurationModule.Managers;
using Cysharp.Threading.Tasks;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviour
{
    private static string url;

    private void Start()
    {
        switch (ConfigurationManager.Instance.GameConfig.env)
        {
            case GameEnvironment.DEV:
                {
                    url = ConfigurationManager.Instance.GameConfig.devUrl;
                    break;
                }
            case GameEnvironment.STAGING:
                {
                    url = ConfigurationManager.Instance.GameConfig.stagingUrl;
                    break;
                }
            case GameEnvironment.PROD:
                {
                    url = ConfigurationManager.Instance.GameConfig.prodUrl;
                    break;
                }
        }
    }
    public static async UniTask<string> GETRequestCoroutine(string relativePath, Action<long, string> OnError, bool isAuthenticated = false)
    {
        UnityWebRequest www = new UnityWebRequest(url + relativePath, "GET");
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Access-Control-Allow-Origin", "*");
        if (isAuthenticated)
        {
            www.SetRequestHeader("principalId", GameState.principalId);
            www.SetRequestHeader("Authorization", "Basic aWNraXR0aWVzXzE5MDk5MjAzOjJXWi1FM31Xb3dwaGxzIzQyMyNAMSVncnI=");

        }
        await www.SendWebRequest();

        if (www.error != null)
        {
            OnError?.Invoke(www.responseCode, www.downloadHandler.text);
            return "";
        }
        else
        {
            return www.downloadHandler.text;
        }

    }

    public static async UniTask POSTRequest(string relativePath, string json, Action<string> OnSuccess, Action<long, string> OnError, bool isAuthenticated = false)
    {
        UnityWebRequest www = new UnityWebRequest(url + relativePath, "POST");
        if (json != null && json != "")
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        }

        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Access-Control-Allow-Origin", "*");
        if (isAuthenticated)
        {
            www.SetRequestHeader("principalId", GameState.principalId);
            www.SetRequestHeader("Authorization", "Basic aWNraXR0aWVzXzE5MDk5MjAzOjJXWi1FM31Xb3dwaGxzIzQyMyNAMSVncnI=");

        }
        await www.SendWebRequest();

        if (www.error != null)
        {
            OnError?.Invoke(www.responseCode, www.downloadHandler.text);
        }
        else
        {
            OnSuccess?.Invoke(www.downloadHandler.text);
        }

    }
}
