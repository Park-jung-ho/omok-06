using Newtonsoft.Json.Linq;
using SocketIOClient;
using System;
using UnityEngine;
using static Constants;

public class MultiplayController : IDisposable
{
    private SocketIO socket;
    public string RoomId { get; private set; }

    public Action OnMatchSuccess;
    public Action OnMatchCanceled;
    public Action OnStartAI;
    public Action<int, string> OnOpponentMove;

    public MultiplayController(string email)
    {
        socket = NetworkManager.Instance.Socket;
        if (socket == null || !socket.Connected)
        {
            return;
        }

        RegisterEvents();
    }

    private void RegisterEvents()
    {
        // 매칭 대기
        socket.On("waiting", (response) =>
        {
        });

        // 카운트다운
        socket.On("matchTimer", (response) =>
        {
            try
            {
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
            catch (Exception)
            {
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

                    string black = data["black"]?.ToString();
                    string white = data["white"]?.ToString();

                    string myEmail = UserData.Instance.Email;
                    string opponentEmail = (myEmail == black) ? white : black;
                    UserData.Instance.OpponentEmail = opponentEmail;

                    // 서버가 지정한 흑/백 정보 반영
                    UserData.Instance.IsBlack = (myEmail == black);

                    MatchingManager.Instance.EnqueueOnMainThread(() =>
                    {
                        MatchingManager.Instance.IsMatched = true;
                        MatchingManager.Instance.CurrentRoomId = RoomId;

                        MatchingPopupController.ClosePopup();
                        GameManager.Instance.ChangeToGameScene(Constants.GameType.MultiPlay);
                    });

                    OnMatchSuccess?.Invoke();
                }
            }
            catch (Exception)
            {
            }
        });

        // AI 매칭
        socket.On("startGameWithAI", (response) =>
        {
            try
            {
                var json = response.ToString();
                var array = JArray.Parse(json);
                if (array.Count > 0)
                {
                    var data = array[0];
                    RoomId = data["roomId"]?.Value<string>();
                    bool ai = data["ai"]?.Value<bool>() ?? false;

                    OnStartAI?.Invoke();
                }
            }
            catch (Exception)
            {
            }
        });

        // 상대 착수
        socket.On("doOpponent", (response) =>
        {
            try
            {
                var raw = response.ToString();
                var array = JArray.Parse(raw);
                var data = array[0] as JObject;

                int blockIndex = data["blockIndex"].Value<int>();
                string opponentEmail = data["email"].ToString();

                MatchingManager.Instance.EnqueueOnMainThread(() =>
                {
                    if (blockIndex == -1)
                    {
                        if (opponentEmail != UserData.Instance.Email)
                        {
                            GameManager.Instance.EndGame(true);
                        }
                        else
                        {
                            GameManager.Instance.EndGame(false);
                        }
                        return;
                    }
                    OnOpponentMove?.Invoke(blockIndex, opponentEmail);
                });
            }
            catch (Exception)
            {
            }
        });
    }

    // 매칭 참가
    public async void JoinMatch(string email)
    {
        if (socket == null) return;
        await socket.EmitAsync("joinMatch", email);
    }

    // 매칭 취소
    public async void CancelMatch(string email)
    {
        if (socket == null) return;
        await socket.EmitAsync("cancelMatch", email);
        OnMatchCanceled?.Invoke();
    }

    // 내 착수
    public async void DoPlayerMove(int blockIndex)
    {
        if (socket == null) return;
        if (!socket.Connected) return;

        await socket.EmitAsync("doPlayer", new { roomId = RoomId, blockIndex });
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
