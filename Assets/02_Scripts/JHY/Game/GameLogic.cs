using System;
using UnityEngine;

public class GameLogic : IDisposable
{
    public BlockController blockController;

    private Constants.PlayerType[,] _board;

    public BasePlayerState firstPlayerState;
    public BasePlayerState secondPlayerState;

    public enum GameResult { None, Win, Lose, Draw }

    public BasePlayerState CurrentPlayerState { get; set; }

    public GameLogic(BlockController blockController, Constants.GameType gameType)
    {
        this.blockController = blockController;
        _board = new Constants.PlayerType[Constants.BlockColumnCount, Constants.BlockColumnCount];

        switch (gameType)
        {
            case Constants.GameType.SinglePlay:
                InitSinglePlay();
                break;

            case Constants.GameType.DualPlay:
                InitDualPlay();
                break;

            case Constants.GameType.MultiPlay:
                InitMultiPlay();
                break;
        }
    }

    // AI 대전 (싱글)
    private void InitSinglePlay()
    {
        Debug.Log("싱글플레이(=AI 대전) 시작");

        firstPlayerState = new PlayerState(true);   // 흑: 사람
        secondPlayerState = new AIState(false);     // 백: AI

        SetState(firstPlayerState);
    }

    // 로컬 2인
    private void InitDualPlay()
    {
        Debug.Log("듀얼플레이 시작");

        firstPlayerState = new PlayerState(true);   // 흑
        secondPlayerState = new PlayerState(false); // 백

        GameManager.Instance.StartTurn(Constants.PlayerType.PlayerA);
        SetState(firstPlayerState);
    }

    // 멀티플레이
    private void InitMultiPlay()
    {
        if (MatchingManager.Instance.IsMatched)
        {
            Debug.Log("멀티 매칭 성공으로 게임 시작");

            firstPlayerState = new PlayerState(true); // 흑
            secondPlayerState = new MultiplayerState(false, MatchingManager.Instance.CurrentRoomId); // 백

            GameManager.Instance.StartTurn(Constants.PlayerType.PlayerA);
            SetState(firstPlayerState);
        }
        else
        {
            Debug.Log("멀티 매칭 실패 → 싱글플레이(AI 대전)로 전환");
            GameManager.Instance.ChangeToGameScene(Constants.GameType.SinglePlay);
        }
    }

    public Constants.PlayerType GetCurrentPlayerType()
    {
        return CurrentPlayerState == firstPlayerState
            ? Constants.PlayerType.PlayerA
            : Constants.PlayerType.PlayerB;
    }

    public Constants.PlayerType[,] GetBoard() => _board;

    public void SetState(BasePlayerState state)
    {
        CurrentPlayerState?.OnExit(this);
        CurrentPlayerState = state;
        CurrentPlayerState?.OnEnter(this);
    }

    public void SelectBlock(int row, int col)
    {
        if (_board[row, col] != Constants.PlayerType.None) return;

        PlayerState playerState = CurrentPlayerState as PlayerState;
        if (playerState != null)
        {
            Block.MarkerType markerType =
                (playerState.PlayerType == Constants.PlayerType.PlayerA)
                ? Block.MarkerType.Black
                : Block.MarkerType.White;

            blockController.PlaceScope(markerType, row, col);
        }
    }

    public void ConfirmPlay()
    {
        var (row, col) = blockController.GetFocusBlockPosition();
        if (row == -1 || col == -1) return;

        if (CurrentPlayerState == firstPlayerState)
            CurrentPlayerState.HandleMove(this, Constants.PlayerType.PlayerA, row, col);
        else
            CurrentPlayerState.HandleMove(this, Constants.PlayerType.PlayerB, row, col);
    }

    public bool SetNewBoardValue(Constants.PlayerType playerType, int row, int col)
    {
        if (_board[row, col] != Constants.PlayerType.None) return false;

        _board[row, col] = playerType;
        GameManager.Instance.TimerReset(playerType);
        return true;
    }

    public void ProcessMarker()
    {
        blockController.SetMarker();
    }

    public void EndGame(GameResult gameResult)
    {
        SetState(null);
        firstPlayerState = null;
        secondPlayerState = null;

        GameManager.Instance.OpenConfirmPanel("게임오버", () =>
        {
            GameManager.Instance.ChangeToMainScene();
        });
    }

    public GameResult CheckGameResult()
    {
        // TODO: 승리 조건 체크 필요
        return GameResult.None;
    }

    public void Dispose()
    {
        // 네트워크 자원 해제 필요시 추가
    }
}
