using UnityEngine;

public class PlayerState : BasePlayerState
{
    private bool _isFirstPlayer;
    public Constants.PlayerType PlayerType { get; private set; }

    // Multi
    private MultiplayController _multiplayController;
    private bool _isMultiplay;

    public PlayerState(bool isFirstPlayer)
    {
        _isFirstPlayer = isFirstPlayer;
        PlayerType = _isFirstPlayer ? Constants.PlayerType.PlayerA : Constants.PlayerType.PlayerB;
        _isMultiplay = false;
    }

    public PlayerState(bool isFirstPlayer, MultiplayController multiplayController)
        : this(isFirstPlayer)
    {
        _multiplayController = multiplayController;
        _isMultiplay = true;
    }

    public override void OnEnter(GameLogic gameLogic)
    {
        if (_isFirstPlayer)
            GameManager.Instance.SetGameTurnPanel(GameUIController.GameTurnPanelType.ATurn);
        else
            GameManager.Instance.SetGameTurnPanel(GameUIController.GameTurnPanelType.BTurn);

        // 클릭 이벤트 → 블록 선택 처리
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
            GameManager.Instance.StartTurn(Constants.PlayerType.PlayerB);
            gameLogic.SetState(gameLogic.secondPlayerState);

            // 상대가 AI라면 바로 착수 실행
            if (gameLogic.secondPlayerState is AIState ai)
            {
                ai.HandleMove(gameLogic, Constants.PlayerType.PlayerB, -1, -1);
            }
        }
        else
        {
            GameManager.Instance.StartTurn(Constants.PlayerType.PlayerA);
            gameLogic.SetState(gameLogic.firstPlayerState);
        }
    }
}
