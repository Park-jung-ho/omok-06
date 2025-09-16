using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using SocketIOClient;

public class MatchingManager : Singleton<MatchingManager>
{
    [SerializeField] private GameObject matchingPopupPrefab; // [Panel] Matching 프리팹

    private GameObject matchingPopupInstance; // 매칭 팝업 인스턴스
    private Coroutine countdownCoroutine;     // 카운트다운 코루틴
    private bool isMatched;                   // 매칭 성공 여부

    private SocketIO client;

    // 싱글톤 기본 Awake()는 base에서 처리됨
    // 씬 로드 시 실행되는 로직만 이쪽에서 오버라이드
    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        // 매칭 팝업 씬 이동 시 자동 닫기
        CloseMatchingPopup();

        // 상태 초기화
        isMatched = false;
    }

    // 멀티플레이 버튼 눌렀을 때 호출
    public async void OnClickMultiPlay()
    {
        Debug.Log("매칭 매니저: 멀티 버튼 눌림");

        OpenMatchingPopup();

        if (UserData.Instance == null) return;
        string email = UserData.Instance.Email;
        if (string.IsNullOrEmpty(email)) return;

        // 소켓 클라이언트 초기화
        if (client == null && !string.IsNullOrEmpty(Constants.SocketServerURL))
        {
            client = new SocketIO(Constants.SocketServerURL, new SocketIOOptions
            {
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
            });
            RegisterSocketEvents();
        }

        client.Options.Query = new Dictionary<string, string> { { "email", email } };

        if (client.Connected)
            await client.DisconnectAsync();

        await client.ConnectAsync();
    }

    // 소켓 이벤트 등록
    private void RegisterSocketEvents()
    {
        if (client == null) return;

        client.On("waiting", response =>
        {
            Debug.Log("서버 이벤트: waiting");
            OpenMatchingPopup();
        });

        client.On("startGame", response =>
        {
            Debug.Log("서버 이벤트: startGame");
            OnMatchedWithPlayer();
        });

        client.On("startGameWithAI", response =>
        {
            Debug.Log("서버 이벤트: startGameWithAI");
            OnMatchedWithAI();
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
            countdownCoroutine = StartCoroutine(RunCountdown(countdownText, 10));
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

    // 카운트다운
    private IEnumerator RunCountdown(TMP_Text text, int start)
    {
        int time = start;
        while (time > 0)
        {
            text.text = time.ToString();
            yield return new WaitForSeconds(1f);
            time--;
        }

        if (!isMatched)
        {
            Debug.Log("카운트다운 종료 -> AI 매칭");
            OnMatchedWithAI();
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
        GameManager.Instance.ChangeToGameScene(Constants.GameType.SinglePlay);
    }
}
