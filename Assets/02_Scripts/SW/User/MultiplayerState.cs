using UnityEngine;

public class MultiplayerState : BasePlayerState
{
    private string roomId;
    private bool isMyTurn;
    public bool IsMyTurn => isMyTurn;

    public MultiplayerState(bool isMyTurn, string roomId)
    {
        this.isMyTurn = isMyTurn;
        this.roomId = roomId;
    }

    public void SetTurn(bool myTurn)
    {
        isMyTurn = myTurn;
        Debug.Log("내 턴 여부 갱신: " + isMyTurn);
    }

    public override void OnEnter(GameLogic gameLogic)
    {
        var controller = MatchingManager.Instance?.GetMultiplayController();
        if (controller != null)
        {
            controller.OnOpponentMove = (blockIndex, opponentEmail) =>
            {
                if (opponentEmail == UserData.Instance.Email) return;

                int row = blockIndex / Constants.BlockColumnCount;
                int col = blockIndex % Constants.BlockColumnCount;
                var opponentType = UserData.Instance.IsBlack
                    ? Constants.PlayerType.PlayerB
                    : Constants.PlayerType.PlayerA;

                if (gameLogic.SetNewBoardValue(opponentType, row, col))
                {
                    var markerType = opponentType == Constants.PlayerType.PlayerA
                        ? Block.MarkerType.Black : Block.MarkerType.White;

                    gameLogic.blockController.PlaceStone(markerType, row, col);

                    // ✅ 상대방 수 → 승리 체크
                    var result = gameLogic.CheckGameResult((row, col));
                    if (result != GameLogic.GameResult.None)
                    {
                        bool iWin = (result == GameLogic.GameResult.Win);
                        gameLogic.EndGame(result);
                        GameManager.Instance.EndGame(iWin);
                        return;
                    }

                    // 내 턴으로 전환
                    var myType = UserData.Instance.IsBlack
                        ? Constants.PlayerType.PlayerA
                        : Constants.PlayerType.PlayerB;

                    gameLogic.SetState(
                        myType == Constants.PlayerType.PlayerA
                            ? gameLogic.firstPlayerState : gameLogic.secondPlayerState);

                    if (gameLogic.CurrentPlayerState is MultiplayerState multi)
                    {
                        bool myTurnNow =
                            (UserData.Instance.IsBlack && myType == Constants.PlayerType.PlayerA) ||
                            (!UserData.Instance.IsBlack && myType == Constants.PlayerType.PlayerB);
                        multi.SetTurn(myTurnNow);
                    }

                    GameManager.Instance.StartTurn(myType);
                }
            };
        }

        gameLogic.blockController.OnBlockClickedDelegate = (row, col) =>
        {
            if (isMyTurn) gameLogic.SelectBlock(row, col);
            else Debug.Log("내 턴이 아님 → 클릭 무시");
        };

        if (isMyTurn)
        {
            var myType = UserData.Instance.IsBlack
                ? Constants.PlayerType.PlayerA : Constants.PlayerType.PlayerB;
            GameManager.Instance.StartTurn(myType);
        }
    }

    public override void OnExit(GameLogic gameLogic)
    {
        gameLogic.blockController.OnBlockClickedDelegate = null;
    }

    public override void HandleMove(GameLogic gameLogic, Constants.PlayerType playerType, int row, int col)
    {
        if (!isMyTurn) return;
        if (!gameLogic.SetNewBoardValue(playerType, row, col)) return;

        gameLogic.ProcessMarker();

        var controller = MatchingManager.Instance?.GetMultiplayController();
        controller?.DoPlayerMove(row * Constants.BlockColumnCount + col);

        gameLogic.blockController.ClearScope();

        // 내 착수 후 승리 체크
        var result = gameLogic.CheckGameResult((row, col));
        if (result != GameLogic.GameResult.None)
        {
            bool iWin = (result == GameLogic.GameResult.Win);
            gameLogic.EndGame(result);
            GameManager.Instance.EndGame(iWin);
            return;
        }

        isMyTurn = false;

        var nextTurn = (playerType == Constants.PlayerType.PlayerA)
            ? Constants.PlayerType.PlayerB : Constants.PlayerType.PlayerA;

        gameLogic.SetState(
            nextTurn == Constants.PlayerType.PlayerA
                ? gameLogic.firstPlayerState : gameLogic.secondPlayerState);

        if (gameLogic.CurrentPlayerState is MultiplayerState multi)
        {
            bool myTurnNow =
                (UserData.Instance.IsBlack && nextTurn == Constants.PlayerType.PlayerA) ||
                (!UserData.Instance.IsBlack && nextTurn == Constants.PlayerType.PlayerB);
            multi.SetTurn(myTurnNow);
        }

        GameManager.Instance.StartTurn(nextTurn);
    }

    protected override void HandleNextTurn(GameLogic gameLogic)
    {
        Debug.Log("멀티플레이어 상태에서 턴 전환 처리");
    }
}
