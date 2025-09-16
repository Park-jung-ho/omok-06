using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using SocketIOClient;

public class MatchingManager : Singleton<MatchingManager>
{
    [SerializeField] private GameObject matchingPopupPrefab;

    private GameObject matchingPopupInstance;
    private Coroutine countdownCoroutine;
    private bool isMatched;

    private SocketIO client;

    // 메인스레드 작업 큐
    private readonly Queue<Action> mainThreadActions = new Queue<Action>();

    private void Update()
    {
        lock (mainThreadActions)
        {
            while (mainThreadActions.Count > 0)
            {
                var action = mainThreadActions.Dequeue();
                action?.Invoke();
            }
        }
    }

    private void EnqueueOnMainThread(Action action)
    {
        lock (mainThreadActions)
        {
            mainThreadActions.Enqueue(action);
        }
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        CloseMatchingPopup();
        isMatched = false;
    }

    // 멀티플레이 버튼 눌렀을 때
    public async void OnClickMultiPlay()
    {
        Debug.Log("매칭 매니저: 멀티 버튼 눌림");

        OpenMatchingPopup();

        if (UserData.Instance == null) return;
        string email = UserData.Instance.Email;
        if (string.IsNullOrEmpty(email)) return;

        // 소켓 초기화
        if (client == null)
        {
            client = new SocketIO(Constants.SocketServerURL, new SocketIOOptions
            {
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
                Query = new Dictionary<string, string> { { "email", email } }
            });
            RegisterSocketEvents();

            await client.ConnectAsync();
            Debug.Log("소켓 연결 완료");
        }

        // 매칭 참가 요청
        if (client.Connected)
        {
            await client.EmitAsync("joinMatch", email);
            Debug.Log("joinMatch 이벤트 서버로 전송 완료");
        }
    }

    private void RegisterSocketEvents()
    {
        if (client == null) return;

        client.On("waiting", response =>
        {
            Debug.Log("서버 이벤트: waiting");
            EnqueueOnMainThread(OpenMatchingPopup);
        });

        client.On("matchTimer", response =>
        {
            var data = response.GetValue<Dictionary<string, object>>();
            if (data != null && data.ContainsKey("timeLeft"))
            {
                int timeLeft = 0;
                int.TryParse(data["timeLeft"].ToString(), out timeLeft);

                // UI 갱신을 반드시 메인 스레드에서 실행
                EnqueueOnMainThread(() =>
                {
                    if (matchingPopupInstance != null)
                    {
                        TMP_Text[] texts = matchingPopupInstance.GetComponentsInChildren<TMP_Text>(true);
                        foreach (var t in texts)
                        {
                            if (t.name == "CountdownText")
                            {
                                t.text = timeLeft.ToString();
                                break;
                            }
                        }
                    }
                });
            }
        });

        client.On("startGame", response =>
        {
            Debug.Log("서버 이벤트: startGame");
            EnqueueOnMainThread(OnMatchedWithPlayer);
        });

        client.On("startGameWithAI", response =>
        {
            Debug.Log("서버 이벤트: startGameWithAI");
            EnqueueOnMainThread(OnMatchedWithAI);
        });
    }

    // 매칭 팝업 열기
    private void OpenMatchingPopup()
    {
        if (matchingPopupInstance != null) return;

        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas 없음");
            return;
        }

        matchingPopupInstance = Instantiate(matchingPopupPrefab, canvas.transform);

        var countdownObj = matchingPopupInstance.transform.Find("CountdownText");
        if (countdownObj != null)
        {
            TMP_Text countdownText = countdownObj.GetComponent<TMP_Text>();
            countdownText.text = "10";
            isMatched = false;
        }
        else
        {
            Debug.LogError("CountdownText 오브젝트 없음");
        }
    }

    // 매칭 팝업 닫기
    private void CloseMatchingPopup()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        if (matchingPopupInstance != null)
        {
            Destroy(matchingPopupInstance);
            matchingPopupInstance = null;
        }
    }

    // 매칭 취소
    public async void CancelMatching()
    {
        Debug.Log("매칭 취소 버튼 눌림");

        CloseMatchingPopup();

        if (client != null && client.Connected)
        {
            await client.EmitAsync("cancelMatch", UserData.Instance.Email);
            Debug.Log("cancelMatch 이벤트 서버로 전송 완료");
        }
    }

    // 플레이어 매칭 성공
    private void OnMatchedWithPlayer()
    {
        isMatched = true;
        CloseMatchingPopup();
        GameManager.Instance.ChangeToGameScene(Constants.GameType.MultiPlay);
    }

    // AI 매칭
    private void OnMatchedWithAI()
    {
        isMatched = true;
        CloseMatchingPopup();
        GameManager.Instance.ChangeToGameScene(Constants.GameType.DualPlay);
    }
}
