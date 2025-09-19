using static Constants;
using System;
using System.Collections.Generic;
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
    public (int row, int col) LastBlockPosition { get; private set; }
    public PlayerType PlayerType { get; private set; }      // 싱글 플레이에서 누가 플레이어인지 구분하는 용도
    public PlayerType User1PlayerType { get; private set; }
    public PlayerType User2PlayerType { get; private set; }

    // Multi
    //private MultiplayController _multiplayController;
    //private string _roomId;

    public GameLogic(BlockController blockController, GameType gameType, bool turnSwitch)
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
                if(turnSwitch)
                {
                    PlayerType = PlayerType.PlayerB;

                    firstPlayerState = new AIState(true);
                    secondPlayerState = new PlayerState(false);

                    UserData.Instance.SetReplayData("AI",UserData.Instance.Rank,false);
                }
                else
                {
                    PlayerType = PlayerType.PlayerA;

                    firstPlayerState = new PlayerState(true);   // 선공(흑돌)
                    secondPlayerState = new AIState(false);     // 후공(백돌)

                    UserData.Instance.SetReplayData("AI",UserData.Instance.Rank);
                }

                break;
            case GameType.DualPlay:
                firstPlayerState = new PlayerState(true);
                secondPlayerState = new PlayerState(false);
                UserData.Instance.SetReplayData("Player2",UserData.Instance.Rank);

                if (turnSwitch)
                {
                    User1PlayerType = PlayerType.PlayerB;
                    User2PlayerType = PlayerType.PlayerA;
                }
                else
                {
                    User1PlayerType = PlayerType.PlayerA;
                    User2PlayerType = PlayerType.PlayerB;
                }

                break;
            case GameType.MultiPlay:
                InitMultiPlay();
                break;
        }
    }

    public void BoardReset()
    {
        for (int row = 0; row < BlockColumnCount; row++)
        {
            for (int col = 0; col < BlockColumnCount; col++)
            {
                _board[row, col] = PlayerType.None;
            }
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

            UserData.Instance.SetReplayData(UserData.Instance.OpponentNickname,UserData.Instance.OpponentRank);

            (firstPlayerState as MultiplayerState)?.SetTurn(true);
        }
        else
        {
            firstPlayerState = new MultiplayerState(false, MatchingManager.Instance.CurrentRoomId);
            secondPlayerState = new MultiplayerState(true, MatchingManager.Instance.CurrentRoomId);

            UserData.Instance.SetReplayData(UserData.Instance.OpponentNickname,UserData.Instance.OpponentRank, false);
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
    public void StartSetState()
    {
        SetState(firstPlayerState);
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

        LastBlockPosition = (row, col);
        GameManager.Instance.TimerReset(playerType);

        SoundManager.Instance.PlaySFX("play");

        // 리플레이 저장
        UserData.Instance.replayData.replay.Add(new ReplayController.BlockData{col = col, row = row});
        Debug.Log($"리플레이 저장 [{row},{col}]");

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

        GameManager.Instance.GameReset();

        GameManager.Instance.OpenConfirmPanel("게임오버", () =>
        {
            GameManager.Instance.ChangeToMainScene();
        });
    }

    // 승리/무승부 판정
    public GameResult CheckGameResult((int row, int col) lastMove)
    {
        if (GameResultChecker.CheckGameDraw(_board)) { return GameResult.Draw; } // 무승부

        PlayerType winnerType = GameResultChecker.CheckBoardState(_board, LastBlockPosition); // 게임 결과값 출력 메서드 호출

        if (winnerType == PlayerType.None) { return GameResult.None; } // 승부가 나지 않으면 None 반환

        if (GameManager._gameType == GameType.DualPlay)    // 혼자하기
        {
            if (winnerType == User1PlayerType) { Debug.Log("User 1 승"); }
            else if (winnerType == User2PlayerType) { Debug.Log("User 2 승"); }
        }
        else if (GameManager._gameType == GameType.SinglePlay) // AI대전
        {
            if (winnerType == PlayerType)
            {
                Debug.Log("플레이어 승");
            }
            else
            {
                Debug.Log("AI 승");
            }
        }
        return GameResult.None;
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
