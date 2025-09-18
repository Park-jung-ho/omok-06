
using static Constants;
using System;
using UnityEngine;

public class GameLogic : IDisposable
{
    public BlockController blockController;         

    private PlayerType[,] _board;         

    public BasePlayerState firstPlayerState;        
    public BasePlayerState secondPlayerState;       
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

        switch (gameType)
        {
            case GameType.SinglePlay:
                if(turnSwitch)
                {
                    PlayerType = PlayerType.PlayerB;

                    firstPlayerState = new AIState(true);
                    secondPlayerState = new PlayerState(false);
                    SetState(firstPlayerState);
                }
                else
                {
                    PlayerType = PlayerType.PlayerA;

                    firstPlayerState = new PlayerState(true);   // 선공(흑돌)
                    secondPlayerState = new AIState(false);     // 후공(백돌)
                    SetState(firstPlayerState);
                }
                break;
            case GameType.DualPlay:
                if (turnSwitch) 
                {
                    firstPlayerState = new PlayerState(false);
                    secondPlayerState = new PlayerState(true);

                    User1PlayerType = PlayerType.PlayerB;
                    User2PlayerType = PlayerType.PlayerA;

                    SetState(secondPlayerState);
                }
                else
                {
                    firstPlayerState = new PlayerState(true); 
                    secondPlayerState = new PlayerState(false);

                    User1PlayerType = PlayerType.PlayerA;
                    User2PlayerType = PlayerType.PlayerB;

                    SetState(firstPlayerState);
                }

                break;
                // 멀티 플레이의 경우 호스트가 돌 색을 결정해서 서버에 저장
                // 클라에서는 서버에서 결정된 색을 받아서 설정
            //case Constants.GameType.MultiPlay:
            //    _multiplayController = new MultiplayController((state, roomId) =>
            //    {
            //        _roomId = roomId;
            //        switch (state)
            //        {
            //            case Constants.MultiplayControllerState.CreateRoom:
            //                Debug.Log("## Create Room ##");
            //                // TODO: 대기 화면 UI 표시
            //                break;
            //            case Constants.MultiplayControllerState.JoinRoom:
            //                Debug.Log("## Join Room ##");
            //                firstPlayerState = new MultiplayerState(true, _multiplayController);
            //                secondPlayerState = new PlayerState(false, _multiplayController, _roomId);
            //                SetState(firstPlayerState);
            //                break;
            //            case Constants.MultiplayControllerState.StartGame:
            //                Debug.Log("## Start Game ##");
            //                firstPlayerState = new PlayerState(true, _multiplayController, _roomId);
            //                secondPlayerState = new MultiplayerState(false, _multiplayController);
            //                SetState(firstPlayerState);
            //                break;
            //            case Constants.MultiplayControllerState.ExitRoom:
            //                Debug.Log("## Exit Room ##");
            //                // TODO: 팝업 띄우고 메인화면으로 이동
            //                break;
            //            case Constants.MultiplayControllerState.EndGame:
            //                Debug.Log("## End Game ##");
            //                // TODO: 팝업 띄우고 메인화면으로 이동
            //                break;
            //        }
            //    });
            //    break;
        }
    }

    public PlayerType GetCurrentPlayerType()
    {
        if (CurrentPlayerState == firstPlayerState)
            return PlayerType.PlayerA;
        else
            return PlayerType.PlayerB;
    }

    public PlayerType[,] GetBoard()
    {
        return _board;
    }

    public void SetState(BasePlayerState state)
    {
        CurrentPlayerState?.OnExit(this);
        CurrentPlayerState = state;
        CurrentPlayerState?.OnEnter(this);
    }

    // 마우스 클릭 시 스코프만 표시
    public void SelectBlock(int row, int col)
    {
        // 이미 놓여진 경우
        if (_board[row, col] != PlayerType.None) 
            return;

        PlayerState playerState = CurrentPlayerState as PlayerState;

        if (playerState != null)
        {
            Block.MarkerType markerType = (playerState.PlayerType == PlayerType.PlayerA) ? Block.MarkerType.Black : Block.MarkerType.White;
            blockController.PlaceScope(markerType, row, col);
        }
    }

    // 선택된 블록이 있는지 체크하고 있다면 마커 표시와 보드에 표시
    public void ConfirmPlay()
    {
        //if (!blockController.IsScopeBlock())
        //{
        //    Debug.Log("선택된 블록이 없는 상태에서 착수 버튼 클릭");
        //    return;
        //}

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
        LastBlockPosition = (row, col);
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

    public void Dispose()
    {
        //_multiplayController?.LeaveRoom(_roomId);
        //_multiplayController?.Dispose();
    }
}
