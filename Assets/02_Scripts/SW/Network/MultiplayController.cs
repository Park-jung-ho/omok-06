using Newtonsoft.Json;
using SocketIOClient;
using System;
using System.Net.Sockets;
using UnityEngine;

// joinRoom/ createRoom 이벤트 전달할 때 전달되는 정보의 타입
public class RoomData
{
    [JsonProperty("roomId")]
    public string roomId { get; set; }
}

// 상대방이 둔 마커 위치
public class BlockData
{
    [JsonProperty("blockIndex")]
    public int blockIndex { get; set; }
}

public class MultiplayController : IDisposable
{
    private SocketIOUnity _socket;

    // Room 상태 변화에 따른 동작을 할당하는 변수
    private Action<Constants.MultiplayControllerState, string> _onMultiplayStateChanged;

    // 게임 진행 상황에서 Marker의 위치를 업데이트 하는 변수
    public Action<int> onBlockDataChanged;


    public MultiplayController(Action<Constants.MultiplayControllerState, string> onMultiplayStateChanged)
    {
        // 서버에서 이벤트가 발생하면 처리 할 메서드를 _onMultiplayStateChanged에 등록
        _onMultiplayStateChanged = onMultiplayStateChanged;


        // Socket.io 클라이언트 초기화
        var uri = new Uri(Constants.SocketServerURL);
        _socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
            Reconnection = false,        // 자동 재접속 비활성화
            ReconnectionAttempts = 0,    // 재시도 횟수도 0
            ReconnectionDelay = 0        // 혹시 모를 딜레이도 제거
        });

        _socket.OnUnityThread("createRoom", CreateRoom);
        _socket.OnUnityThread("joinRoom", JoinRoom);
        _socket.OnUnityThread("startGame", StartGame);
        _socket.OnUnityThread("exitGame", ExitRoom);
        _socket.OnUnityThread("endGame", EndGame);
        _socket.OnUnityThread("doOpponent", DoOpponent);

        _socket.Connect(); // 서버에 접속
    }
    private void CreateRoom(SocketIOResponse response)
    {
        var data = response.GetValue<RoomData>();
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayControllerState.CreateRoom, data.roomId);
    }

    private void JoinRoom(SocketIOResponse response)
    {
        var data = response.GetValue<RoomData>();
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayControllerState.JoinRoom, data.roomId);
    }

    private void StartGame(SocketIOResponse response)
    {
        var data = response.GetValue<RoomData>();
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayControllerState.StartGame, data.roomId);
    }

    private void EndGame(SocketIOResponse response)
    {
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayControllerState.EndGame, null);
    }

    private void ExitRoom(SocketIOResponse response)
    {
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayControllerState.ExitRoom, null);
    }


    private void DoOpponent(SocketIOResponse response)
    {
        var data = response.GetValue<BlockData>();
        onBlockDataChanged?.Invoke(data.blockIndex);
    }

    #region Client => Server

    // Room을 나올 때 호출하는 메서드, Client => Server
    public void LeaveRoom(string roomId)
    {
        // 수정
        _socket.Emit("leaveRoom", new { roomId });
    }

    // 플레이어가 Marker를 두면 호출하는 메서드, Client => Server
    public void DoPlayer(string roomId, int blockIndex)
    {
        _socket.Emit("doPlayer", new { roomId, blockIndex });
    }
    #endregion

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
