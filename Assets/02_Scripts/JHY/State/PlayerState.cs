using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Playables;
using static Constants;

public class PlayerState : BasePlayerState
{
    private bool _isFirstPlayer;
    public PlayerType PlayerType { get; set; }

    // Multi
    private MultiplayController _multiplayController;
    private bool _isMultiplay;

    public PlayerState(bool isFirstPlayer)
    {
        _isFirstPlayer = isFirstPlayer;
        PlayerType = _isFirstPlayer ? PlayerType.PlayerA : PlayerType.PlayerB;
        _isMultiplay = false;
    }

    public PlayerState(bool isFirstPlayer, MultiplayController multiplayController)
        : this(isFirstPlayer)
    {
        _multiplayController = multiplayController;
        _isMultiplay = true;
    }

    #region 필수 메소드
    public override void OnEnter(GameLogic gameLogic)
    {
        if (_isFirstPlayer)
            GameManager.Instance.SetGameTurnPanel(GameUIController.GameTurnPanelType.ATurn);
        else
            GameManager.Instance.SetGameTurnPanel(GameUIController.GameTurnPanelType.BTurn);

        GameManager.Instance.SetPlayButtonActive(true);

        // 클릭 이벤트 발생 시 -> Scope On
        gameLogic.blockController.OnBlockClickedDelegate = (row, col) =>
        {
            gameLogic.SelectBlock(row, col);
        };
    }

    public override void OnExit(GameLogic gameLogic)
    {
        gameLogic.blockController.OnBlockClickedDelegate = null;
        GameManager.Instance.SetPlayButtonActive(false);
    }

    public override void HandleMove(GameLogic gameLogic, PlayerType currentPlayerType, int row, int col)
    {
        // 실제 착수 처리
        ProcessMove(gameLogic, currentPlayerType, row, col);

        // 멀티 모드라면 서버에 전송
        if (_isMultiplay && _multiplayController != null)
        {
            int blockIndex = row * Constants.BlockColumnCount + col;
            Debug.Log($"[멀티] 내 착수 서버 전송: {blockIndex}");
            _multiplayController.DoPlayerMove(blockIndex);
        }
    }

    protected override void HandleNextTurn(GameLogic gameLogic)
    {
        if (_isFirstPlayer)
        {
            GameManager.Instance.StartTurn(PlayerType.PlayerB);
            gameLogic.SetState(gameLogic.secondPlayerState);

            // 상대가 AI라면 바로 착수 실행
            if (gameLogic.secondPlayerState is AIState ai)
            {
                ai.HandleMove(gameLogic, Constants.PlayerType.PlayerB, -1, -1);
            }
        }
        else
        {
            GameManager.Instance.StartTurn(PlayerType.PlayerA);
            gameLogic.SetState(gameLogic.firstPlayerState);
        }
    }
    #endregion
}
