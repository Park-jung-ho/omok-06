public class PlayerState : BasePlayerState
{
    private bool _isFirstPlayer;
    private Constants.PlayerType _playerType;

    // Multi
    //private string _roomId;
    //private bool _isMultiplay;

    public PlayerState(bool isFirstPlayer)
    {
        _isFirstPlayer = isFirstPlayer;
        _playerType = _isFirstPlayer ? Constants.PlayerType.PlayerA : Constants.PlayerType.PlayerB;
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
            //GameManager.Instance.SetGameTurnPanel(GameUIController.GameTurnPanelType.ATurn);
        }
        else
        {
            //GameManager.Instance.SetGameTurnPanel(GameUIController.GameTurnPanelType.BTurn);
        }

        gameLogic.blockController.OnBlockClickedDelegate = (row, col) =>
        {
            HandleMove(gameLogic, row, col);
        };
    }

    public override void OnExit(GameLogic gameLogic)
    {
        gameLogic.blockController.OnBlockClickedDelegate = null;
    }

    public override void HandleMove(GameLogic gameLogic, int row, int col)
    {
        ProcessMove(gameLogic, _playerType, row, col);

        //if (_isMultiplay)
        //    _multiplayController.DoPlayer(_roomId, row * Constants.BlockColumnCount + col);
    }

    protected override void HandleNextTurn(GameLogic gameLogic)
    {
        if (_isFirstPlayer)
        {
            gameLogic.SetState(gameLogic.secondPlayerState);
        }
        else
        {
            gameLogic.SetState(gameLogic.firstPlayerState);
        }
    }
    #endregion
}