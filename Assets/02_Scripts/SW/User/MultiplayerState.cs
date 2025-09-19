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

                // 상대 돌 타입 결정
                var opponentType = UserData.Instance.IsBlack
                    ? Constants.PlayerType.PlayerB
                    : Constants.PlayerType.PlayerA;

                if (gameLogic.SetNewBoardValue(opponentType, row, col))
                {
                    var markerType = opponentType == Constants.PlayerType.PlayerA
                        ? Block.MarkerType.Black : Block.MarkerType.White;

                    gameLogic.blockController.PlaceStone(markerType, row, col);

                    // 승리 판정
                    var winner = GameResultChecker.CheckBoardState(gameLogic.GetBoard(), (row, col));
                    if (winner != Constants.PlayerType.None)
                    {
                        bool iAmBlack = UserData.Instance.IsBlack;
                        bool iWin = (iAmBlack && winner == Constants.PlayerType.PlayerA) ||
                                    (!iAmBlack && winner == Constants.PlayerType.PlayerB);

                        gameLogic.EndGame(iWin ? GameLogic.GameResult.Win : GameLogic.GameResult.Lose);
                        GameManager.Instance.EndGame(iWin);
                        return;
                    }

                    // 턴 전환 (내 차례로)
                    var myType = UserData.Instance.IsBlack
                        ? Constants.PlayerType.PlayerA
                        : Constants.PlayerType.PlayerB;

                    gameLogic.SetState(
                        myType == Constants.PlayerType.PlayerA
                            ? gameLogic.firstPlayerState : gameLogic.secondPlayerState);

                    if (gameLogic.CurrentPlayerState is MultiplayerState multi)
                        multi.SetTurn(true);

                    // 상대가 두었으니 이제 내 차례 → 내 타입(myType)을 넘기는 게 맞음
                    GameManager.Instance.StartTurn(myType);
                }
            };
        }

        // 블록 클릭 → 내 턴일 때만 가능
        gameLogic.blockController.OnBlockClickedDelegate = (row, col) =>
        {
            if (isMyTurn) gameLogic.SelectBlock(row, col);
        };
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

        // 승리 판정
        var winner = GameResultChecker.CheckBoardState(gameLogic.GetBoard(), (row, col));
        if (winner != Constants.PlayerType.None)
        {
            bool iAmBlack = UserData.Instance.IsBlack;
            bool iWin = (iAmBlack && winner == Constants.PlayerType.PlayerA) ||
                        (!iAmBlack && winner == Constants.PlayerType.PlayerB);

            gameLogic.EndGame(iWin ? GameLogic.GameResult.Win : GameLogic.GameResult.Lose);
            GameManager.Instance.EndGame(iWin);
            return;
        }

        // 턴 전환 (상대 차례로)
        isMyTurn = false;
        var nextTurn = (playerType == Constants.PlayerType.PlayerA)
            ? Constants.PlayerType.PlayerB : Constants.PlayerType.PlayerA;

        gameLogic.SetState(
            nextTurn == Constants.PlayerType.PlayerA
                ? gameLogic.firstPlayerState : gameLogic.secondPlayerState);

        if (gameLogic.CurrentPlayerState is MultiplayerState multi)
            multi.SetTurn(false);

        // 이번에는 내가 돌을 뒀으니, 상대 턴 시작 → nextTurn 넘김
        GameManager.Instance.StartTurn(nextTurn);
    }

    protected override void HandleNextTurn(GameLogic gameLogic) { }
}
