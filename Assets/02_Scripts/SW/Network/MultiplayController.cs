using Newtonsoft.Json.Linq;
using SocketIOClient;
using System;
using UnityEngine;

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

                    if (players != null && players.Length == 2)
                    {
                        string myEmail = UserData.Instance.Email;
                        string opponentEmail = (players[0] == myEmail) ? players[1] : players[0];
                        UserData.Instance.OpponentEmail = opponentEmail;

                        Debug.Log($"[startGame] myEmail={myEmail}, players={string.Join(",", players)}");
                        Debug.Log($"[startGame] opponentEmail={opponentEmail}");

                        MatchingManager.Instance.EnqueueOnMainThread(() =>
                        {
                            MatchingManager.Instance.StartCoroutine(
                                UserData.Instance.RefreshOpponentData(() =>
                                {
                                    if (UserData.Instance.Rank > UserData.Instance.OpponentRank)
                                    {
                                        UserData.Instance.IsBlack = true;
                                        Debug.Log("내 급수 숫자가 더 높음 → 내가 흑돌");
                                    }
                                    else
                                    {
                                        UserData.Instance.IsBlack = false;
                                        Debug.Log("상대 급수 숫자가 더 높음 → 내가 백돌");
                                    }
                                })
                            );
                        });
                    }

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
            Debug.Log("### [CLIENT] doOpponent 이벤트 발생 ### Raw=" + response);

            try
            {
                var raw = response.ToString();
                var array = JArray.Parse(raw);
                var data = array[0] as JObject;

                int blockIndex = data["blockIndex"].Value<int>();
                string opponentEmail = data["email"].ToString();

                Debug.Log($"[CLIENT] 파싱 성공: blockIndex={blockIndex}, opponentEmail={opponentEmail}");

                // Unity 관련 동작은 메인스레드 큐로 전달
                MatchingManager.Instance.EnqueueOnMainThread(() =>
                {
                    OnOpponentMove?.Invoke(blockIndex, opponentEmail);
                    Debug.Log("[CLIENT] OnOpponentMove Invoke 호출 완료 (메인스레드)");
                });
            }
            catch (Exception ex)
            {
                Debug.LogError("[CLIENT] doOpponent 파싱 실패: " + ex.Message + "\nRaw=" + response.ToString());
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
        Debug.Log($"[CLIENT] DoPlayerMove 호출됨 blockIndex={blockIndex}, RoomId={RoomId}, Connected={socket?.Connected}");

        if (socket == null)
        {
            Debug.LogError("[CLIENT] socket == null");
            return;
        }
        if (!socket.Connected)
        {
            Debug.LogError("[CLIENT] socket 연결 안됨");
            return;
        }

        await socket.EmitAsync("doPlayer", new { roomId = RoomId, blockIndex });
        Debug.Log("[CLIENT] doPlayer emit 완료");
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
