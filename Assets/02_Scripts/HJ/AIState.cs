using System.Threading.Tasks;
using UnityEngine;
using static Constants;

public class AIState : BasePlayerState
{
    private bool isFirstPlayer;
    private (int row, int col) aiMovePos;

    public Constants.PlayerType aiType;

    public AIState(bool isFirstPlayer)
    {
        this.isFirstPlayer = isFirstPlayer;
        aiType = isFirstPlayer ? Constants.PlayerType.PlayerA : Constants.PlayerType.PlayerB;
    }

    public override void HandleMove(GameLogic gameLogic, Constants.PlayerType currentPlayerType, int row, int col)
    {
        Debug.Log("AI 계산중...");
        DoAIBehaviour(gameLogic, currentPlayerType);
    }

    public override void OnEnter(GameLogic gameLogic)
    {
        if (isFirstPlayer)
        {
            GameManager.Instance.SetGameTurnPanel(GameUIController.GameTurnPanelType.ATurn);
        }
        else
        {
            GameManager.Instance.SetGameTurnPanel(GameUIController.GameTurnPanelType.BTurn);
        }
    }

    public override void OnExit(GameLogic gameLogic)
    {

    }

    protected override void HandleNextTurn(GameLogic gameLogic)
    {
        if (isFirstPlayer)
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

    private async void DoAIBehaviour(GameLogic gameLogic, Constants.PlayerType currentPlayerType)
    {
        aiMovePos = (-1, -1);

        if (currentPlayerType != Constants.PlayerType.None)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            await Task.Run(() =>
            {
                sw.Start();
                aiMovePos = OmokAI.GetPosition(gameLogic.GetBoard(), currentPlayerType);
            });
            sw.Stop();
            Debug.Log("AI 연산 완료!");
            Debug.Log($"연산 시간: {sw.ElapsedMilliseconds}ms");
        }

        if (aiMovePos.row == -1)
        {
            Debug.Log("인자 값 오류");
            return;
        }

        Block.MarkerType markerType = Block.MarkerType.None;
        if (currentPlayerType == Constants.PlayerType.PlayerA)
        {
            markerType = Block.MarkerType.Black;
        }
        else if (currentPlayerType == Constants.PlayerType.PlayerB)
        {
            markerType = Block.MarkerType.White;
        }

        gameLogic.blockController.GetBlocks()[aiMovePos.row * 15 + aiMovePos.col].CurrentMarkerType = markerType;
        gameLogic.blockController.GetBlocks()[aiMovePos.row * 15 + aiMovePos.col].SetMarker();

        if (gameLogic.SetNewBoardValue(currentPlayerType, aiMovePos.row, aiMovePos.col))
        {
            var gameResult = gameLogic.CheckGameResult();

            if (gameResult == GameLogic.GameResult.None)
            {
                HandleNextTurn(gameLogic);
            }
            else
            {
                Debug.Log("결과 : {gameResult}");
                gameLogic.EndGame(gameResult);
            }
        }
    }
}
