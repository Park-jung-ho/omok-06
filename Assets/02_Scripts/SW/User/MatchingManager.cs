using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchingManager : Singleton<MatchingManager>
{
    private MultiplayController multiplayController;

    // 매칭 성공 여부와 방 ID
    public bool IsMatched { get; set; } = false;
    public string CurrentRoomId { get; set; }

    // 메인 스레드 큐
    private readonly Queue<Action> mainThreadActions = new Queue<Action>();

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Main")
        {
            IsMatched = false;
            CurrentRoomId = null;
        }
    }

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

    public void EnqueueOnMainThread(Action action)
    {
        lock (mainThreadActions)
        {
            mainThreadActions.Enqueue(action);
        }
    }

    // ---------------------------------------------------------
    // 멀티 버튼 눌렀을 때
    // ---------------------------------------------------------
    public void OnClickMultiPlay()
    {
        if (UserData.Instance == null || string.IsNullOrEmpty(UserData.Instance.Email))
        {
            Debug.LogWarning("UserData 없음 → 매칭 불가");
            return;
        }

        string email = UserData.Instance.Email;

        MatchingPopupController.OpenPopup();
        StartMatch(email);
    }

    private void StartMatch(string email)
    {
        multiplayController = new MultiplayController(email);

        // 이벤트 등록을 먼저
        multiplayController.OnMatchSuccess = () =>
        {
            EnqueueOnMainThread(() =>
            {
                IsMatched = true;
                CurrentRoomId = multiplayController.RoomId;
                Debug.Log("상대와 매칭 성공 → 멀티 모드로 씬 전환");

                MatchingPopupController.ClosePopup();
                GameManager.Instance.ChangeToGameScene(Constants.GameType.MultiPlay);
            });
        };

        multiplayController.OnStartAI = () =>
        {
            EnqueueOnMainThread(() =>
            {
                IsMatched = true;
                CurrentRoomId = null;
                Debug.Log("AI 매칭 시작 → 싱글 모드로 씬 전환");

                MatchingPopupController.ClosePopup();
                GameManager.Instance.ChangeToGameScene(Constants.GameType.SinglePlay);
            });
        };

        multiplayController.OnMatchCanceled = () =>
        {
            EnqueueOnMainThread(() =>
            {
                IsMatched = false;
                CurrentRoomId = null;
                Debug.Log("매칭 취소됨");

                MatchingPopupController.ClosePopup();
            });
        };

        // 이벤트 등록이 끝난 뒤에 joinMatch 호출
        multiplayController.JoinMatch(email);
    }

    public void CancelMatching()
    {
        Debug.Log("매칭 취소 요청");
        multiplayController?.CancelMatch(UserData.Instance.Email);
        IsMatched = false;
        CurrentRoomId = null;
        MatchingPopupController.ClosePopup();
    }

    public MultiplayController GetMultiplayController() => multiplayController;
}
