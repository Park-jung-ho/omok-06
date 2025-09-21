using HJ;
using UnityEngine;
using UnityEngine.Playables;
using static Constants;

public abstract class BasePlayerState
{
    public abstract void OnEnter(GameLogic gameLogic);
    public abstract void OnExit(GameLogic gameLogic);
    public abstract void HandleMove(GameLogic gameLogic, PlayerType currentPlayerType, int row, int col);
    protected abstract void HandleNextTurn(GameLogic gameLogic);

    protected void ProcessMove(GameLogic gameLogic, PlayerType playerType, int row, int col)
    {
        // 마커 표시 진행
        gameLogic.ProcessMarker();

        // 보드에 값 저장
        if (gameLogic.SetNewBoardValue(playerType, row, col))
        {
            // 승패 판정
            GameLogic.GameResult gameResult = gameLogic.CheckGameResult((row, col));

            Debug.Log(gameResult);

            if (gameResult == GameLogic.GameResult.None)
            {
                HandleNextTurn(gameLogic);
            }
            else
            {
                GameManager.Instance.thisRoundResult = gameResult;
                gameLogic.EndGame(gameResult);
            }
        }

    }
}
