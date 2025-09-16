using Newtonsoft.Json.Linq;
using SocketIOClient;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayController : IDisposable
{
    private SocketIO socket;
    public string RoomId { get; private set; }

    public Action OnMatchSuccess;
    public Action OnMatchCanceled;
    public Action OnStartAI;
    public Action<int> OnOpponentMove;

    public MultiplayController(string email)
    {
        socket = NetworkManager.Instance.Socket;
        if (socket == null || !socket.Connected)
        {
            Debug.LogError("소켓이 연결되지 않았습니다. NetworkManager.ConnectSocket 먼저 호출해야 합니다.");
            return;
        }

        RegisterEvents();
    }

    private void RegisterEvents()
    {
        // 매칭 대기
        socket.On("waiting", (response) =>
        {
            Debug.Log("서버 이벤트: waiting → Raw=" + response.ToString());
        });

        // 카운트다운
        socket.On("matchTimer", (response) =>
        {
            try
            {
                Debug.Log("### matchTimer 수신 ### Raw=" + response.ToString());

                var json = response.ToString();
                var array = JArray.Parse(json);
                if (array.Count > 0)
                {
                    var data = array[0];
                    int timeLeft = data["timeLeft"]?.Value<int>() ?? -1;

                    if (timeLeft >= 0)
                    {
                        MatchingManager.Instance.EnqueueOnMainThread(() =>
                        {
                            MatchingPopupController.UpdateCountdown(timeLeft);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[matchTimer error] " + ex.Message + "\nRaw=" + response.ToString());
            }
        });

        // 멀티 매칭 성공
        socket.On("startGame", (response) =>
        {
            try
            {
                var json = response.ToString();
                var array = JArray.Parse(json);
                if (array.Count > 0)
                {
                    var data = array[0];
                    RoomId = data["roomId"]?.Value<string>();
                    var players = data["players"]?.ToObject<string[]>();

                    Debug.Log($"서버 이벤트: startGame, roomId={RoomId}, players={string.Join(",", players)}");

                    MatchingManager.Instance.EnqueueOnMainThread(() =>
                    {
                        
                        // 여기서 멀티 매칭 성공 처리
                        MatchingManager.Instance.IsMatched = true;
                        MatchingManager.Instance.CurrentRoomId = RoomId;

                        MatchingPopupController.ClosePopup();

                        GameManager.Instance.ChangeToGameScene(Constants.GameType.MultiPlay);
                    });

                    OnMatchSuccess?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[startGame error] " + ex.Message + "\nRaw: " + response.ToString());
            }
        });


        // AI 매칭
        socket.On("startGameWithAI", (response) =>
        {
            try
            {
                Debug.Log("### startGameWithAI 수신 ### Raw=" + response.ToString());

                var json = response.ToString();
                var array = JArray.Parse(json);
                if (array.Count > 0)
                {
                    var data = array[0];
                    RoomId = data["roomId"]?.Value<string>();
                    bool ai = data["ai"]?.Value<bool>() ?? false;

                    Debug.Log($"서버 이벤트: startGameWithAI, roomId={RoomId}, ai={ai}");

                    MatchingManager.Instance.EnqueueOnMainThread(() =>
                    {
                        MatchingPopupController.ClosePopup();
                        GameManager.Instance.ChangeToGameScene(Constants.GameType.SinglePlay);
                    });

                    OnStartAI?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[startGameWithAI error] " + ex.Message + "\nRaw=" + response.ToString());
            }
        });

        // 상대 착수
        socket.On("doOpponent", (response) =>
        {
            try
            {
                Debug.Log("### doOpponent 수신 ### Raw=" + response.ToString());

                var data = response.GetValue<JObject>(0);
                int blockIndex = data["blockIndex"].Value<int>();
                Debug.Log("상대 착수 blockIndex=" + blockIndex);
                OnOpponentMove?.Invoke(blockIndex);
            }
            catch (Exception ex)
            {
                Debug.LogError("[doOpponent error] " + ex.Message + "\nRaw=" + response.ToString());
            }
        });
    }

    // 매칭 참가
    public async void JoinMatch(string email)
    {
        if (socket == null) return;

        await socket.EmitAsync("joinMatch", email);
        Debug.Log("joinMatch 전송 완료");
    }

    // 매칭 취소
    public async void CancelMatch(string email)
    {
        if (socket == null) return;
        await socket.EmitAsync("cancelMatch", email);
        Debug.Log("cancelMatch 전송 완료");
        OnMatchCanceled?.Invoke();
    }

    // 내 착수
    public async void DoPlayerMove(int blockIndex)
    {
        if (socket == null) return;
        await socket.EmitAsync("doPlayer", new { roomId = RoomId, blockIndex });
        Debug.Log("내 착수 전송 blockIndex=" + blockIndex);
    }

    public void Dispose()
    {
        if (socket != null)
        {
            socket.Off("waiting");
            socket.Off("matchTimer");
            socket.Off("startGame");
            socket.Off("startGameWithAI");
            socket.Off("doOpponent");
        }
    }
}
