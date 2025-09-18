using static Constants;
using System;
using UnityEngine;

public class GameLogic : IDisposable
{
    public BlockController blockController;

    public BasePlayerState firstPlayerState;
    public BasePlayerState secondPlayerState;
    private PlayerType[,] _board;

    public enum GameResult { None, Win, Lose, Draw }
    public GameType currnetPlayMode { get; private set; }

    public BasePlayerState CurrentPlayerState { get; set; }

    public GameLogic(BlockController blockController, GameType gameType)
    {
        this.blockController = blockController;

        _board = new PlayerType[BlockColumnCount, BlockColumnCount];
        currnetPlayMode = gameType;

        // 블록 클릭 → 커서만 표시
        blockController.OnBlockClickedDelegate = (row, col) =>
        {
            Debug.Log($"[GAMELOGIC] 블록 클릭 row={row}, col={col}");
            SelectBlock(row, col);
        };

        blockController.InitBlocks();

        switch (gameType)
        {
            case GameType.SinglePlay:
                firstPlayerState = new PlayerState(true);
                secondPlayerState = new AIState(false);
                SetState(firstPlayerState);
                break;
            case GameType.DualPlay:
                firstPlayerState = new PlayerState(true);
                secondPlayerState = new PlayerState(false);
                SetState(firstPlayerState);
                break;
            case GameType.MultiPlay:
                InitMultiPlay();
                break;
        }
    }

    private void InitMultiPlay()
    {
        if (!MatchingManager.Instance.IsMatched)
        {
            Debug.Log("멀티 매칭 실패 → 싱글로 전환");
            GameManager.Instance.ChangeToGameScene(Constants.GameType.SinglePlay);
            return;
        }

        Debug.Log("멀티 매칭 성공 → 게임 시작");

        if (UserData.Instance.IsBlack)
        {
            firstPlayerState = new MultiplayerState(true, MatchingManager.Instance.CurrentRoomId);
            secondPlayerState = new MultiplayerState(false, MatchingManager.Instance.CurrentRoomId);
            SetState(firstPlayerState);

            (firstPlayerState as MultiplayerState)?.SetTurn(true);
            GameManager.Instance.StartTurn(Constants.PlayerType.PlayerA);
        }
        else
        {
            firstPlayerState = new MultiplayerState(false, MatchingManager.Instance.CurrentRoomId);
            secondPlayerState = new MultiplayerState(true, MatchingManager.Instance.CurrentRoomId);
            SetState(firstPlayerState);

            GameManager.Instance.StartTurn(Constants.PlayerType.PlayerA);
        }

        var ui = UnityEngine.Object.FindFirstObjectByType<GameUIController>();
        if (ui != null)
            ui.SetStoneIcons(UserData.Instance.IsBlack);
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
        if (_board[row, col] != PlayerType.None) return;

        Block.MarkerType markerType = Block.MarkerType.None;

        if (CurrentPlayerState is PlayerState playerState)
        {
            markerType = (playerState.PlayerType == PlayerType.PlayerA)
                ? Block.MarkerType.Black : Block.MarkerType.White;
        }
        else if (CurrentPlayerState is MultiplayerState)
        {
            var currentTurn = GetCurrentPlayerType();
            markerType = (currentTurn == Constants.PlayerType.PlayerA)
                ? Block.MarkerType.Black : Block.MarkerType.White;
        }
        else
        {
            Debug.LogError("CurrentPlayerState가 PlayerState/MultiplayerState가 아님");
            return;
        }

        blockController?.PlaceScope(markerType, row, col);
    }

    public void ConfirmPlay()
    {
        var (row, col) = blockController.GetFocusBlockPosition();

        if (row != -1 && col != -1)
        {
            if (CurrentPlayerState == firstPlayerState)
                CurrentPlayerState.HandleMove(this, PlayerType.PlayerA, row, col);
            else
                CurrentPlayerState.HandleMove(this, PlayerType.PlayerB, row, col);
        }
    }

    public bool SetNewBoardValue(PlayerType playerType, int row, int col)
    {
        if (_board[row, col] != PlayerType.None)
            return false;

        _board[row, col] = playerType;

        if (GameManager._gameType != Constants.GameType.MultiPlay)
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

    // 승리/무승부 판정
    public GameResult CheckGameResult((int row, int col) lastMove)
    {
        var winner = GameResultChecker.CheckBoardState(_board, lastMove);

        if (winner == PlayerType.PlayerA)
        {
            return UserData.Instance.IsBlack ? GameResult.Win : GameResult.Lose;
        }
        else if (winner == PlayerType.PlayerB)
        {
            return UserData.Instance.IsBlack ? GameResult.Lose : GameResult.Win;
        }
        else if (GameResultChecker.CheckGameDraw(_board))
        {
            return GameResult.Draw;
        }
        else
        {
            return GameResult.None;
        }
    }

    // 기존 코드가 부르는 매개변수 없는 버전 → 내부에서 FocusBlock 좌표 쓰기
    public GameResult CheckGameResult()
    {
        var (row, col) = blockController.GetFocusBlockPosition();
        if (row == -1 || col == -1) return GameResult.None;

        return CheckGameResult((row, col));
    }

    public void Dispose() { }
}
