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

    // Multi
    //private MultiplayController _multiplayController;   
    //private string _roomId;                         

    public GameLogic(BlockController blockController, Constants.GameType gameType)
    {
        this.blockController = blockController;

        _board = new Constants.PlayerType[Constants.BlockColumnCount, Constants.BlockColumnCount];

        switch (gameType)
        {
            case Constants.GameType.SinglePlay:
                firstPlayerState = new PlayerState(true);
                //secondPlayerState = new AIState();
                SetState(firstPlayerState);
                break;
            case Constants.GameType.DualPlay:
                firstPlayerState = new PlayerState(true);
                secondPlayerState = new PlayerState(false);
                SetState(firstPlayerState);
                break;
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

    public Constants.PlayerType GetCurrentPlayerType()
    {
        if (CurrentPlayerState == firstPlayerState)
            return Constants.PlayerType.PlayerA;
        else
            return Constants.PlayerType.PlayerB;
    }

    public Constants.PlayerType[,] GetBoard()
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
        if (_board[row, col] != Constants.PlayerType.None) 
            return;

        PlayerState playerState = (PlayerState)CurrentPlayerState;

        if (playerState != null)
        {
            Block.MarkerType markerType = (playerState.PlayerType == Constants.PlayerType.PlayerA) ? Block.MarkerType.Black : Block.MarkerType.White;
            blockController.PlaceScope(markerType, row, col);
        }
    }

    // 선택된 블록이 있는지 체크하고 있다면 마커 표시와 보드에 표시
    public void ConfirmPlay()
    {
        if (!blockController.IsScopeBlock())
        {
            Debug.Log("선택된 블록이 없는 상태에서 착수 버튼 클릭");
            return;
        }

        var (row, col) = blockController.GetFocusBlockPosition();

        if (row != -1 && col != -1)
        {
            if(CurrentPlayerState == firstPlayerState)
                CurrentPlayerState.HandleMove(this, Constants.PlayerType.PlayerA, row, col);
            else
                CurrentPlayerState.HandleMove(this, Constants.PlayerType.PlayerB, row, col);
        }
    }

    public bool SetNewBoardValue(Constants.PlayerType playerType, int row, int col)
    {
        // 이미 보드에 돌을 놓은 플레이어가 존재한다면
        if (_board[row, col] != Constants.PlayerType.None) 
            return false;

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
        //if (OmokAI.CheckGameWin(Constants.PlayerType.PlayerA, _board)) { return GameResult.Win; }
        //if (OmokAI.CheckGameWin(Constants.PlayerType.PlayerB, _board)) { return GameResult.Lose; }
        //if (OmokAI.CheckGameDraw(_board)) { return GameResult.Draw; }
        return GameResult.None;
    }

    public void Dispose()
    {
        //_multiplayController?.LeaveRoom(_roomId);
        //_multiplayController?.Dispose();
    }
}