
using static Constants;
using System;
using UnityEngine;

public class GameLogic : IDisposable
{
    public BlockController blockController;

    private Constants.PlayerType[,] _board;

    public BasePlayerState firstPlayerState;
    public BasePlayerState secondPlayerState;
    private PlayerType[,] _board;         

    public enum GameResult { None, Win, Lose, Draw }
    public GameType currnetPlayMode { get; private set; }

    public BasePlayerState CurrentPlayerState { get; set; }

    // Multi
    //private MultiplayController _multiplayController;   
    //private string _roomId;                         

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
                // 선공(흑돌)
                firstPlayerState = new PlayerState(true);
                // 후공(백돌)
                //secondPlayerState = new AIState();
                SetState(firstPlayerState);
                break;
            case GameType.DualPlay:
                firstPlayerState = new PlayerState(true);
                secondPlayerState = new PlayerState(false);
                SetState(firstPlayerState);
                break;
            case Constants.GameType.MultiPlay:
                InitMultiPlay();
                break;
        }
    }

    private void InitSinglePlay()
    {
        Debug.Log("싱글플레이 시작");
        firstPlayerState = new PlayerState(true);
        secondPlayerState = new AIState(false);

        SetState(firstPlayerState);
        GameManager.Instance.StartTurn(Constants.PlayerType.PlayerA);
    }

    private void InitDualPlay()
    {
        Debug.Log("듀얼플레이 시작");
        firstPlayerState = new PlayerState(true);
        secondPlayerState = new PlayerState(false);

        SetState(firstPlayerState);
        GameManager.Instance.StartTurn(Constants.PlayerType.PlayerA);
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

            // 시작 시 내 턴임을 명확히 지정
            (firstPlayerState as MultiplayerState)?.SetTurn(true);

            GameManager.Instance.StartTurn(Constants.PlayerType.PlayerA);
        }
        else
        {
            // 백: 후수 플레이어
            firstPlayerState = new MultiplayerState(false, MatchingManager.Instance.CurrentRoomId);
            secondPlayerState = new MultiplayerState(true, MatchingManager.Instance.CurrentRoomId);
            SetState(firstPlayerState); // 현재는 흑 차례

            // 바로 "상대방 턴" UI와 타이머 시작
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
        // 이미 놓여진 경우
        if (_board[row, col] != PlayerType.None) 
            return;

        PlayerState playerState = CurrentPlayerState as PlayerState;

        if (CurrentPlayerState is PlayerState playerState)
        {
            Block.MarkerType markerType = (playerState.PlayerType == PlayerType.PlayerA) ? Block.MarkerType.Black : Block.MarkerType.White;
            blockController.PlaceScope(markerType, row, col);
        }
        else if (CurrentPlayerState is MultiplayerState)
        {
            // 멀티는 내 흑/백 여부(UserData) 대신
            // 지금 턴이 누구인지(GetCurrentPlayerType)로 판정
            var currentTurn = GetCurrentPlayerType();
            markerType = (currentTurn == Constants.PlayerType.PlayerA)
                ? Block.MarkerType.Black
                : Block.MarkerType.White;
        }
        else return;

        blockController.PlaceScope(markerType, row, col);
    }

    public void ConfirmPlay()
    {
        var (row, col) = blockController.GetFocusBlockPosition();
        
        if (row != -1 && col != -1)
        {
            Debug.Log("실행");
            if(CurrentPlayerState == firstPlayerState)
                CurrentPlayerState.HandleMove(this, PlayerType.PlayerA, row, col);
            else
                CurrentPlayerState.HandleMove(this, PlayerType.PlayerB, row, col);
        }
    }

    public bool SetNewBoardValue(PlayerType playerType, int row, int col)
    {
        // 이미 보드에 돌을 놓은 플레이어가 존재한다면
        if (_board[row, col] != PlayerType.None) 
            return false;

        _board[row, col] = playerType;

        // 싱글/듀얼만 타이머 리셋
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

    public GameResult CheckGameResult() => GameResult.None;

    public void Dispose() { }
}
