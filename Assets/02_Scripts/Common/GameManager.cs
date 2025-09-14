using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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

    void Awake()
    {
        _canvas = FindFirstObjectByType<Canvas>();
        _gameUIController = FindFirstObjectByType<GameUIController>();
        _blockController = FindFirstObjectByType<BlockController>();

    }
    private void Start()
    {
        //OpenSigninPanel();

        // Test Code
        if (_blockController != null)
        {
            _blockController.InitBlocks();
        }

        _gameType = Constants.GameType.DualPlay;
        _gameLogic = new GameLogic(_blockController, _gameType);

        StartTurn(Constants.PlayerType.PlayerA);
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
        Debug.Log("Time Over");
    }

    public void ConfirmPlayButton()
    {
        _gameLogic.ConfirmPlay();
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

    public void OpenConfirmPanel(string message, ConfirmController.OnConfirmButtonClickd onConfirmButtonClicked)
    {
        if (_canvas != null)
        {
            var confirmPanelObject = Instantiate(confirmPanel, _canvas.transform);
            confirmPanelObject.GetComponent<ConfirmController>()
                .Show(message, onConfirmButtonClicked);
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
}
