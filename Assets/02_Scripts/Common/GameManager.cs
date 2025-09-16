using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Constants;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private GameObject signinPanel;
    [SerializeField] private GameObject signupPanel;
    [SerializeField] private GameObject rankingPanel; 

    public static Constants.GameType _gameType;
    private Canvas _canvas;
    private GameLogic _gameLogic;
    private GameUIController _gameUIController;
    private BlockController _blockController;

    private float timer;
    private Coroutine timerCoroutine;
    [SerializeField] private float turnTime = 30f;
    protected override void Awake()
    {
        base.Awake();
        _canvas = FindFirstObjectByType<Canvas>();
    }
    private void Start()
    {
        //OpenSigninPanel();
    }
    public bool IsMyTurn(int myType)
    {
        Constants.PlayerType currentPlayerType = _gameLogic.GetCurrentPlayerType();

        if (myType == (int)currentPlayerType)
            return true;
        else 
            return false;
    }

    public Constants.PlayerType GetOppositePlayerType()
    {
        Constants.PlayerType currentPlayerType = _gameLogic.GetCurrentPlayerType();

        if(currentPlayerType == PlayerType.PlayerA)
            return Constants.PlayerType.PlayerB;
        else
            return Constants.PlayerType.PlayerA;
    }

    public void ChangeToGameScene(Constants.GameType gameType)
    {
        _gameType = gameType;
        SceneManager.LoadScene("Game");
    }
    public void ChangeToMainScene()
    {
        //_gameLogic?.Dispose();
        _gameLogic = null;
        SceneManager.LoadScene("Main");
    }

    public void OpenConfirmPanel(string message, ConfirmController.OnConfirmButtonClickd onConfirmButtonClicked, ConfirmController.OnCloseButtonClicked onCloseButtonClicked = null)
    {
        if (_canvas != null)
        {
            var confirmPanelObject = Instantiate(confirmPanel, _canvas.transform);
            confirmPanelObject.GetComponent<ConfirmController>().Show(message, onConfirmButtonClicked, onCloseButtonClicked);
        }
    }

    public void OpenSigninPanel()
    {
        if (_canvas != null)
        {
            var existingSigninPanel = _canvas.GetComponentInChildren<SigninController>();
            if (existingSigninPanel != null)
                return;

            var signinPanelObject = Instantiate(signinPanel, _canvas.transform);
            signinPanelObject.GetComponent<SigninController>().Show();
        }
    }

    public void OpenSignupPanel()
    {
        if (_canvas != null)
        {
            var signupPanelObject = Instantiate(signupPanel, _canvas.transform);
            signupPanelObject.GetComponent<SignupController>().Show();
        }
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        _canvas = FindFirstObjectByType<Canvas>();

        if (scene.name == "Game")
        {
            _gameUIController = FindFirstObjectByType<GameUIController>();
            _blockController = FindFirstObjectByType<BlockController>();

            if (_blockController != null)
            {
                _blockController.InitBlocks();
            }

            if (_gameUIController != null)
            {
                _gameUIController.SetGameTurnPanel(GameUIController.GameTurnPanelType.None);
            }

            if (_gameLogic != null) _gameLogic.Dispose();
            _gameLogic = new GameLogic(_blockController, _gameType);
        }
    }

    public void SetGameTurnPanel(GameUIController.GameTurnPanelType gameTurnPanelType)
    {
        _gameUIController.SetGameTurnPanel(gameTurnPanelType);
    }

    public void StartTurn(Constants.PlayerType playerType)
    {
        if (timerCoroutine != null)
{            StopCoroutine(timerCoroutine);}

        timerCoroutine = StartCoroutine(TurnTimer(playerType));
    }

    public void TimerReset(Constants.PlayerType playerType)
    {
        timer = turnTime;
        _gameUIController.UpdateTimerUI(timer, playerType);
    }

    private IEnumerator TurnTimer(Constants.PlayerType playerType)
    {
        TimerReset(playerType);

        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            _gameUIController.UpdateTimerUI(timer, playerType);

            yield return null;
        }

        // 타임 오버
        OpenConfirmPanel("타임 오버", () =>
        {
            ChangeToMainScene();
        });

        ToggleGame(false);
    }

    public void ConfirmPlayButton()
    {
        _gameLogic.ConfirmPlay();
    }

    public void ToggleGame(bool active)
    {
        // TODO : 나중에 다시 활성화시켜줘야 함
        _blockController.gameObject.SetActive(active);
    }
}
