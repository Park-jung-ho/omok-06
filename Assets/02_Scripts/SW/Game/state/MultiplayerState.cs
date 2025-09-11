using UnityEngine;

public class MultiplayerState : BasePlayerState
{
    private Constants.PlayerType _playerType;
    private bool _isFirstPlayer;
    private MultiplayController _multiplayController;
    private string _roomId;


    public MultiplayerState(bool isFirstPlayer, MultiplayController multiplayController, string roomId)
    {
        _isFirstPlayer = isFirstPlayer;
        _multiplayController = multiplayController;
        _roomId = roomId;
        _playerType = isFirstPlayer ? Constants.PlayerType.PlayerA : Constants.PlayerType.PlayerB;
    }

    public override void OnEnter(GameLogic gameLogic)
    {
        _multiplayController.onBlockDataChanged = blockIndex =>
        {
            var row = blockIndex / Constants.BlockColumnCount;
            var col = blockIndex % Constants.BlockColumnCount;

            UnityThread.executeInUpdate(() =>
            {
                HandleMove(gameLogic, row, col);
            });
        };
    }

    public override void OnExit(GameLogic gameLogic)
    {
        _multiplayController.onBlockDataChanged = null;
    }

    public override void HandleMove(GameLogic gameLogic, int row, int col)
    {
        ProcessMove(gameLogic, _playerType, row, col);
    }

    protected override void HandleNextTurn(GameLogic gameLogic)
    {
        if (_isFirstPlayer)
            gameLogic.SetState(gameLogic.secondPlayerState);
        else
            gameLogic.SetState(gameLogic.firstPlayerState);

    }
}
