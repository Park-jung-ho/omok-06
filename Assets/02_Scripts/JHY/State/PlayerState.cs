using System.Diagnostics;
using UnityEngine.Playables;

public class PlayerState : BasePlayerState
{
    private bool _isFirstPlayer;
    public Constants.PlayerType PlayerType { get; set; }

    // Multi
    //private string _roomId;
    //private bool _isMultiplay;

    public PlayerState(bool isFirstPlayer)
    {
        _isFirstPlayer = isFirstPlayer;
        PlayerType = _isFirstPlayer ? Constants.PlayerType.PlayerA : Constants.PlayerType.PlayerB;
        //_isMultiplay = false;
    }

    //public PlayerState(bool isFirstPlayer, MultiplayController multiplayController, string roomId)
    //    : this(isFirstPlayer)
    //{
    //    _multiplayController = multiplayController;
    //    _roomId = roomId;
    //    _isMultiplay = true;
    //}

    #region 필수 메소드
    public override void OnEnter(GameLogic gameLogic)
    {
        if (_isFirstPlayer)
        {
            GameManager.Instance.SetGameTurnPanel(GameUIController.GameTurnPanelType.ATurn);
        }
        else
        {
            GameManager.Instance.SetGameTurnPanel(GameUIController.GameTurnPanelType.BTurn);
        }

        // 클릭 이벤트 발생 시 -> Scope On
        gameLogic.blockController.OnBlockClickedDelegate = (row, col) =>
        {
            gameLogic.SelectBlock(row, col);
        };
    }

    public override void OnExit(GameLogic gameLogic)
    {
        gameLogic.blockController.OnBlockClickedDelegate = null;
    }

    public override void HandleMove(GameLogic gameLogic, Constants.PlayerType currentPlayerType, int row, int col)
    {
        ProcessMove(gameLogic, currentPlayerType, row, col);

        //if (_isMultiplay)
        //    _multiplayController.DoPlayer(_roomId, row * Constants.BlockColumnCount + col);
    }

    protected override void HandleNextTurn(GameLogic gameLogic)
    {
        if (_isFirstPlayer)
        {
            GameManager.Instance.StartTurn(Constants.PlayerType.PlayerB);
            gameLogic.SetState(gameLogic.secondPlayerState);
        }
        else
        {
            GameManager.Instance.StartTurn(Constants.PlayerType.PlayerA);
            gameLogic.SetState(gameLogic.firstPlayerState);
        }
    }

    #endregion
}