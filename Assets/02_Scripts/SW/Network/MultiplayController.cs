using Newtonsoft.Json;
using SocketIOClient;
using System;
using System.Net.Sockets;
using UnityEngine;

// joinRoom/ createRoom �̺�Ʈ ������ �� ���޵Ǵ� ������ Ÿ��
public class RoomData
{
    [JsonProperty("roomId")]
    public string roomId { get; set; }
}

// ������ �� ��Ŀ ��ġ
public class BlockData
{
    [JsonProperty("blockIndex")]
    public int blockIndex { get; set; }
}

public class MultiplayController : IDisposable
{
    private SocketIOUnity _socket;

    // Room ���� ��ȭ�� ���� ������ �Ҵ��ϴ� ����
    private Action<Constants.MultiplayControllerState, string> _onMultiplayStateChanged;

    // ���� ���� ��Ȳ���� Marker�� ��ġ�� ������Ʈ �ϴ� ����
    public Action<int> onBlockDataChanged;


    public MultiplayController(Action<Constants.MultiplayControllerState, string> onMultiplayStateChanged)
    {
        // �������� �̺�Ʈ�� �߻��ϸ� ó�� �� �޼��带 _onMultiplayStateChanged�� ���
        _onMultiplayStateChanged = onMultiplayStateChanged;


        // Socket.io Ŭ���̾�Ʈ �ʱ�ȭ
        var uri = new Uri(Constants.SocketServerURL);
        _socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
            Reconnection = false,        // �ڵ� ������ ��Ȱ��ȭ
            ReconnectionAttempts = 0,    // ��õ� Ƚ���� 0
            ReconnectionDelay = 0        // Ȥ�� �� �����̵� ����
        });

        _socket.OnUnityThread("createRoom", CreateRoom);
        _socket.OnUnityThread("joinRoom", JoinRoom);
        _socket.OnUnityThread("startGame", StartGame);
        _socket.OnUnityThread("exitGame", ExitRoom);
        _socket.OnUnityThread("endGame", EndGame);
        _socket.OnUnityThread("doOpponent", DoOpponent);

        _socket.Connect(); // ������ ����
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

    // Room�� ���� �� ȣ���ϴ� �޼���, Client => Server
    public void LeaveRoom(string roomId)
    {
        // ����
        _socket.Emit("leaveRoom", new { roomId });
    }

    // �÷��̾ Marker�� �θ� ȣ���ϴ� �޼���, Client => Server
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
