using UnityEngine;

public class MultiplayerState : BasePlayerState
{
    private string _roomId;
    private bool _isMyTurn;

    public MultiplayerState(bool isMyTurn, string roomId)
    {
        _isMyTurn = isMyTurn;
        _roomId = roomId;
    }

    public override void OnEnter(GameLogic gameLogic)
    {
        Debug.Log("멀티플레이 상태 진입");

        var controller = MatchingManager.Instance?.GetMultiplayController();
        if (controller != null)
        {
            controller.OnOpponentMove = (blockIndex) =>
            {
                int row = blockIndex / Constants.BlockColumnCount;
                int col = blockIndex % Constants.BlockColumnCount;

                Debug.Log($"상대 착수 수신: row={row}, col={col}");

                if (SetOpponentMove(gameLogic, row, col))
                {
                    // 상대가 두었으니 내 턴으로 전환
                    _isMyTurn = true;
                    gameLogic.SetState(gameLogic.firstPlayerState);
                }
            };
        }
    }

    public override void OnExit(GameLogic gameLogic)
    {
        Debug.Log("멀티플레이 상태 종료");
    }

    public override void HandleMove(GameLogic gameLogic, Constants.PlayerType playerType, int row, int col)
    {
        if (!_isMyTurn)
        {
            Debug.Log("내 턴이 아님 → 입력 무시");
            return;
        }

        if (!gameLogic.SetNewBoardValue(playerType, row, col))
            return;

        gameLogic.ProcessMarker();

        var controller = MatchingManager.Instance?.GetMultiplayController();
        if (controller != null)
        {
            int blockIndex = row * Constants.BlockColumnCount + col;
            controller.DoPlayerMove(blockIndex);
        }

        _isMyTurn = false; // 착수 후 턴 종료
        HandleNextTurn(gameLogic);
    }

    protected override void HandleNextTurn(GameLogic gameLogic)
    {
        if (gameLogic.GetCurrentPlayerType() == Constants.PlayerType.PlayerA)
            gameLogic.SetState(gameLogic.secondPlayerState);
        else
            gameLogic.SetState(gameLogic.firstPlayerState);

        Debug.Log("멀티플레이어 상태에서 턴 전환");
    }

    private bool SetOpponentMove(GameLogic gameLogic, int row, int col)
    {
        if (!gameLogic.SetNewBoardValue(Constants.PlayerType.PlayerB, row, col))
            return false;

        gameLogic.ProcessMarker();
        return true;
    }
}
