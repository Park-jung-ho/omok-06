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

    // 닉네임 정보 (내 닉네임, 상대 닉네임)
    public string MyNickname { get; set; }
    public string OpponentNickname { get; set; }

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

    // 멀티 버튼 눌렀을 때
    public void OnClickMultiPlay()
    {
        if (UserData.Instance == null || string.IsNullOrEmpty(UserData.Instance.Email))
        {
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

                MatchingPopupController.ClosePopup();
                GameManager.Instance.OpenSelectPlayerOrderDifficultyPanel((int)Constants.GameType.SinglePlay);
            });
        };

        multiplayController.OnMatchCanceled = () =>
        {
            EnqueueOnMainThread(() =>
            {
                IsMatched = false;
                CurrentRoomId = null;

                MatchingPopupController.ClosePopup();
            });
        };

        // 이벤트 등록이 끝난 뒤에 joinMatch 호출
        multiplayController.JoinMatch(email);
    }

    public void CancelMatching()
    {
        multiplayController?.CancelMatch(UserData.Instance.Email);
        IsMatched = false;
        CurrentRoomId = null;
        MatchingPopupController.ClosePopup();
    }

    public MultiplayController GetMultiplayController() => multiplayController;
}
