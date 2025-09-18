using System.Collections;
using HJ;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Constants;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private GameObject signinPanel;
    [SerializeField] private GameObject signupPanel;
    [SerializeField] private GameObject rankingPanel;
    [SerializeField] private GameObject playModePanel;  // PlayMode 팝업 프리팹

    // 카운트다운
    [SerializeField] private GameObject countdownPanel;
    private GameObject countdownPanelInst;
    private TextMeshProUGUI countdownText;
    private Coroutine countdownRoutine;

    // 흑/백 선택
    [SerializeField] private GameObject selectPlayerOrderPanel;
    private GameObject selectPlayerOrderPanelInst;
    private TextMeshProUGUI playerAText;                // 선공
    private TextMeshProUGUI playerBText;                // 후공

    // Game Result
    [SerializeField] private GameObject gameResultPanel;
    private GameObject gameResultPanelInst;
    private TextMeshProUGUI winnerText;

    public GameType currentGameType { get; private set; }

    // 전환 여부(true면 PlayerB가 선공)
    public bool isSwitched { get; private set; }
    public void TurnSwitch()
    {
        isSwitched = !isSwitched;
    }

    public static GameType _gameType;
    private Canvas _canvas;
    private GameLogic _gameLogic;
    private GameUIController _gameUIController;
    private BlockController _blockController;

    private float timer;
    private Coroutine timerCoroutine;
    [SerializeField] private float turnTime = 30f;

    public GameLogic GameLogic => _gameLogic;

    protected override void Awake()
    {
        base.Awake();

        _canvas = FindFirstObjectByType<Canvas>();
        SceneManager.sceneLoaded += OnSceneLoad;
    }
    private void Start()
    {
        //OpenSigninPanel();
        if (SceneManager.GetActiveScene().name == "Main")
        {
            OpenSigninPanel();
            return; // 게임 로직은 생성하지 않음
        }

        //_gameUIController = FindFirstObjectByType<GameUIController>();
        //_blockController = FindFirstObjectByType<BlockController>();

        //if (_blockController != null)
        //{
        //    _blockController.InitBlocks();
        //}

        //if (_gameUIController != null)
        //{
        //    _gameUIController.SetGameTurnPanel(GameUIController.GameTurnPanelType.None);
        //}

        //if (_gameLogic != null) _gameLogic.Dispose();
        //_gameLogic = new GameLogic(_blockController, ConstantsGameType.SinglePlay, isSwitched);
    }
    public bool IsMyTurn(int myType)
    {
        PlayerType currentPlayerType = _gameLogic.GetCurrentPlayerType();

        if (myType == (int)currentPlayerType)
            return true;
        else 
            return false;
    }

    public void GameReset()
    {
        _blockController.ResetRound();

        _gameLogic.BoardReset();  
    }

    public PlayerType GetOppositePlayerType()
    {
        PlayerType currentPlayerType = _gameLogic.GetCurrentPlayerType();

        if(currentPlayerType == PlayerType.PlayerA)
            return PlayerType.PlayerB;
        else
            return PlayerType.PlayerA;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoad; 
    }

    public void ChangeToGameScene(GameType gameType)
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
            _gameLogic = new GameLogic(_blockController, _gameType, isSwitched);
        }
    }

    public void SetGameTurnPanel(GameUIController.GameTurnPanelType gameTurnPanelType)
    {
        _gameUIController.SetGameTurnPanel(gameTurnPanelType);
    }

    public void StartTurn(PlayerType playerType)
    {
        if (timerCoroutine != null)
{            StopCoroutine(timerCoroutine);}

        timerCoroutine = StartCoroutine(TurnTimer(playerType));
    }

    public void TimerReset(PlayerType playerType)
    {
        timer = turnTime;

        _gameUIController.UpdateTimerUI(timer, playerType);
    }

    private IEnumerator TurnTimer(PlayerType playerType)
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
        }, ChangeToMainScene);

        ToggleGame(false);
    }
    public void ToggleGame(bool active)
    {
        _blockController.gameObject.SetActive(active);
    }

    public void OpenPlayModePanel()
    {
        if (_canvas != null && playModePanel != null)
        {
            var panel = Instantiate(playModePanel, _canvas.transform);
        }
    }

    public void OpenCountdownPanel()
    {
        ToggleGame(false);

        if (_canvas != null && countdownPanel != null)
        {
            if (!countdownPanelInst)
                countdownPanelInst = Instantiate(countdownPanel, _canvas.transform);

            countdownText = countdownPanelInst.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            countdownPanelInst.GetComponent<ConfirmController>().Show();
            countdownRoutine = StartCoroutine(UpdateCountdown(currentGameType));
        }
    }

    public void OpenSelectPlayerOrderPanel(int playMode)
    {
        currentGameType = (GameType)playMode;

        if (_canvas != null && selectPlayerOrderPanel != null)
        {
            if (!selectPlayerOrderPanelInst)
                selectPlayerOrderPanelInst = Instantiate(selectPlayerOrderPanel, _canvas.transform);

            selectPlayerOrderPanelInst.GetComponent<SelectPlayerOrderController>().Show();
        }
    }

    public IEnumerator UpdateCountdown(GameType playMode)
    {
        int count = 3;

        while (count > 0)
        {
            countdownText.text = count.ToString();
            yield return new WaitForSeconds(1f);
            count--;
        }

        countdownText.text = "게임 시작";
        yield return new WaitForSeconds(1f);

        StopCountDown();
    }

    void StopCountDown()
    {
        if (countdownRoutine != null)
            StopCoroutine(countdownRoutine);

        countdownPanelInst.GetComponent<ConfirmController>().Hide();

        StartTurn(PlayerType.PlayerA);
        _gameLogic.StartSetState();

        ToggleGame(true);
    }

}
