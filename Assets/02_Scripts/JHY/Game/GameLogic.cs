using System;
using UnityEngine;

public class GameLogic : IDisposable
{
    public BlockController blockController;         

    private Constants.PlayerType[,] _board;         

    public BasePlayerState firstPlayerState;        
    public BasePlayerState secondPlayerState;       

    public enum GameResult { None, Win, Lose, Draw }

    private BasePlayerState _currentPlayerState;
    
    // Multi
    private MultiplayController _multiplayController;   
    private string _roomId;                         

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
            case Constants.GameType.MultiPlay:
                _multiplayController = new MultiplayController((state, roomId) =>
                {
                    _roomId = roomId;
                    switch (state)
                    {
                        case Constants.MultiplayControllerState.CreateRoom:
                            Debug.Log("## Create Room ##");
                            break;

                        case Constants.MultiplayControllerState.JoinRoom:
                            Debug.Log("## Join Room ##");
                            // A는 PlayerState, B는 MultiplayerState
                            firstPlayerState = new PlayerState(true, _multiplayController, _roomId);
                            secondPlayerState = new MultiplayerState(false, _multiplayController, _roomId);
                            SetState(firstPlayerState);
                            break;

                        case Constants.MultiplayControllerState.StartGame:
                            Debug.Log("## Start Game ##");
                            // B의 입장 기준으로 반대로 설정
                            firstPlayerState = new MultiplayerState(true, _multiplayController, _roomId);
                            secondPlayerState = new PlayerState(false, _multiplayController, _roomId);
                            SetState(firstPlayerState);
                            break;

                        case Constants.MultiplayControllerState.ExitRoom:
                            Debug.Log("## Exit Room ##");
                            break;

                        case Constants.MultiplayControllerState.EndGame:
                            Debug.Log("## End Game ##");
                            break;
                    }
                });
                break;
        }
    }

    public Constants.PlayerType[,] GetBoard()
    {
        return _board;
    }

    public void SetState(BasePlayerState state)
    {
        _currentPlayerState?.OnExit(this);
        _currentPlayerState = state;
        _currentPlayerState?.OnEnter(this);
    }

    public bool SetNewBoardValue(Constants.PlayerType playerType, int row, int col)
    {
        if (_board[row, col] != Constants.PlayerType.None) return false;

        if (playerType == Constants.PlayerType.PlayerA)
        {
            _board[row, col] = playerType;
            blockController.PlaceMaker(Block.MarkerType.Black, row, col);
            return true;
        }
        else if (playerType == Constants.PlayerType.PlayerB)
        {
            _board[row, col] = playerType;
            blockController.PlaceMaker(Block.MarkerType.White, row, col);
            return true;
        }
        return false;
    }

    public void EndGame(GameResult gameResult)
    {
        SetState(null);
        firstPlayerState = null;
        secondPlayerState = null;

        //GameManager.Instance.OpenConfirmPanel("게임오버", () =>
        //{
        //    GameManager.Instance.ChangeToMainScene();
        //});
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