using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class NetworkManager : Singleton<NetworkManager>
{
    private string baseUrl = "http://localhost:3000/users";

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

                // 서버 응답 파싱
                var result = JsonUtility.FromJson<SigninResult>(resultString);

                if (result.result == (int)Constants.ResponseType.SUCCESS)
                {
                    // success 콜백에 전체 result 넘김
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
                Debug.LogError($"로그인 요청 실패: {www.error}, 상태코드: {statusCode}");
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

            if (result.result == 2) // SUCCESS
            {
                success?.Invoke();
            }
            else
            {
                failure?.Invoke(result.result); // 0 = INVALID_EMAIL, 기타 실패
            }
        }
        else
        {
            int statusCode = (int)request.responseCode;
            Debug.LogError($"회원가입 요청 실패: {request.error}, 상태코드: {statusCode}");
            failure?.Invoke(statusCode);
        }
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        // 필요시 씬 로드 시 초기화
    }
}
