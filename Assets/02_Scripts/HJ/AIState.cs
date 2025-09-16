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

    public async override void HandleMove(GameLogic gameLogic, Constants.PlayerType currentPlayerType, int row, int col)
    {
        await Task.Run(() =>
        {
            Debug.Log("AI 계산중...");
            DoAIBehaviour(gameLogic, currentPlayerType);
        });
        Debug.Log("AI 계산완료!");

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

    private void DoAIBehaviour(GameLogic gameLogic, Constants.PlayerType currentPlayerType)
    {
        aiMovePos = (-1, -1);

        if (currentPlayerType != Constants.PlayerType.None)
        {

            aiMovePos = OmokAI.GetPosition(gameLogic.GetBoard(), currentPlayerType);

        }

        if (aiMovePos.row == -1)
        {
            Debug.Log("인자 값 오류");
        }
    }
}
