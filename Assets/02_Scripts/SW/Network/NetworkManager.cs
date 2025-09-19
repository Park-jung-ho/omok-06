using SocketIOClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class NetworkManager : Singleton<NetworkManager>
{
    private string baseUrl = $"{Constants.ServerUrl}/users";

    private SocketIO socket;
    public SocketIO Socket => socket;

    // 로그인 성공 시 호출해서 소켓 연결
    public async void ConnectSocket(string email)
    {
        if (socket != null && socket.Connected)
        {
            return;
        }

        socket = new SocketIO(Constants.SocketServerURL, new SocketIOOptions
        {
            Query = new Dictionary<string, string> { { "email", email } }
        });

        await socket.ConnectAsync();
    }

    // 로그인
    public IEnumerator Signin(SigninData signinData, Action<SigninResult> success, Action<int> failure)
    {
        string url = $"{baseUrl}/signin";
        string jsonString = JsonUtility.ToJson(signinData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonString);

        using (UnityWebRequest www = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var resultString = www.downloadHandler.text;
                var result = JsonUtility.FromJson<SigninResult>(resultString);

                if (result.result == (int)Constants.ResponseType.SUCCESS)
                {
                    success?.Invoke(result);
                }
                else
                {
                    failure?.Invoke(result.result);
                }
            }
            else
            {
                int statusCode = (int)www.responseCode;
                failure?.Invoke(statusCode);
            }
        }
    }

    // 회원가입
    public IEnumerator Signup(SignupData signupData, Action success, Action<int> failure)
    {
        string url = $"{baseUrl}/signup";
        string json = JsonUtility.ToJson(signupData);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var resultString = request.downloadHandler.text;
            var result = JsonUtility.FromJson<SignupResult>(resultString);

            if (result.result == 2)
            {
                success?.Invoke();
            }
            else
            {
                failure?.Invoke(result.result);
            }
        }
        else
        {
            int statusCode = (int)request.responseCode;
            failure?.Invoke(statusCode);
        }
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode) { }
}
