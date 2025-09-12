using Newtonsoft.Json;
using SocketIOClient;
using System;
using UnityEngine;

// 서버에서 보내주는 데이터 구조
public class RoomData
{
    [JsonProperty("roomId")]
    public string roomId { get; set; }
}

public class BlockData
{
    [JsonProperty("blockIndex")]
    public int blockIndex { get; set; }
}

public class MultiplayController : IDisposable
{
    private SocketIOUnity _socket;

    // 룸 상태 변화 콜백
    private Action<Constants.MultiplayControllerState, string> _onMultiplayStateChanged;

    // 상대방 착수 이벤트
    public Action<int> onBlockDataChanged;

    public MultiplayController(Action<Constants.MultiplayControllerState, string> onMultiplayStateChanged)
    {
        _onMultiplayStateChanged = onMultiplayStateChanged;

        var uri = new Uri(Constants.SocketServerURL);
        _socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
            Reconnection = false,
            ReconnectionAttempts = 0,
            ReconnectionDelay = 0
        });

        // 서버 이벤트 매핑
        _socket.OnUnityThread("waiting", Waiting);                  // 방 생성 후 대기
        _socket.OnUnityThread("startGame", StartGame);              // 매칭 성공
        _socket.OnUnityThread("startGameWithAI", StartGameWithAI);  // AI 매칭
        _socket.OnUnityThread("doOpponent", DoOpponent);            // 상대 착수
        _socket.OnUnityThread("endGame", EndGame);                  // 게임 종료
        _socket.OnUnityThread("exitRoom", ExitRoom);                // 방 나가기

        _socket.Connect();
    }

    // === 서버 이벤트 핸들러 ===
    private void Waiting(SocketIOResponse response)
    {
        var data = response.GetValue<RoomData>();
        Debug.Log($"방 생성 완료, 대기 중... RoomId={data.roomId}");
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayControllerState.CreateRoom, data.roomId);
    }

    private void StartGame(SocketIOResponse response)
    {
        var data = response.GetValue<RoomData>();
        Debug.Log($"매칭 성공! RoomId={data.roomId}");
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayControllerState.StartGame, data.roomId);
    }

    private void StartGameWithAI(SocketIOResponse response)
    {
        var data = response.GetValue<RoomData>();
        Debug.Log($"상대 랭크 조건 불만족 → AI 매칭 시작! RoomId={data.roomId}");

        // 여기서 GameManager 게임 타입을 SinglePlay로 변경해서 AI전 시작
        GameManager._gameType = Constants.GameType.SinglePlay;
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayControllerState.StartGame, data.roomId);
    }

    private void DoOpponent(SocketIOResponse response)
    {
        var data = response.GetValue<BlockData>();
        Debug.Log($"상대 착수 BlockIndex={data.blockIndex}");
        onBlockDataChanged?.Invoke(data.blockIndex);
    }

    private void EndGame(SocketIOResponse response)
    {
        Debug.Log("게임 종료 이벤트 수신");
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayControllerState.EndGame, null);
    }

    private void ExitRoom(SocketIOResponse response)
    {
        Debug.Log("방 나가기 이벤트 수신");
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayControllerState.ExitRoom, null);
    }

    // === 클라 → 서버 송신 ===
    public void LeaveRoom(string roomId)
    {
        _socket.Emit("leaveRoom", new { roomId });
    }

    public void DoPlayer(string roomId, int blockIndex)
    {
        _socket.Emit("doPlayer", new { roomId, blockIndex });
    }

    public void Dispose()
    {
        if (_socket != null)
        {
            _socket.Disconnect();
            _socket.Dispose();
            _socket = null;
        }
    }
}
