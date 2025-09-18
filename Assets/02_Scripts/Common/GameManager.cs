using System.Collections;
using HJ;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using static Constants;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private GameObject signinPanel;
    [SerializeField] private GameObject signupPanel;
    [SerializeField] private GameObject rankingPanel;
    [SerializeField] private GameObject playModePanel;

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

    private bool isGameOver = false;

    public GameLogic GameLogic => _gameLogic;

    protected override void Awake()
    {
        base.Awake();

        _canvas = FindFirstObjectByType<Canvas>();
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Main")
        {
            OpenSigninPanel();
            return;
        }
    }

    public bool IsMyTurn(int myType)
    {
        Constants.PlayerType currentPlayerType = _gameLogic.GetCurrentPlayerType();
        return myType == (int)currentPlayerType;
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

    public void StartTurn(PlayerType turn)
    {
        var ui = FindFirstObjectByType<GameUIController>();
        if (ui == null) return;

        // 싱글/듀얼/멀티 모두 동일하게 돌 색 기준으로 표시
        if (turn == PlayerType.PlayerA) // 흑 차례
            ui.SetGameTurnPanel(GameUIController.GameTurnPanelType.ATurn);
        else                            // 백 차례
            ui.SetGameTurnPanel(GameUIController.GameTurnPanelType.BTurn);

        // 코루틴 실행 전에 반드시 멈춤
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        // 해당 턴 타이머 시작
        timerCoroutine = StartCoroutine(TurnTimer(turn));
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

            // 씬 전환되면 바로 멈추도록 체크 후 브레이크
            if (_gameUIController == null || !_gameUIController.isActiveAndEnabled)
                yield break;

            _gameUIController.UpdateTimerUI(timer, playerType);
            yield return null;
        }

        if (_gameUIController == null) yield break;

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


    // 멀티 게임 종료 처리 (서버에 결과 보고)
    public void EndGame(bool isWin)
    {
        if (isGameOver) return;
        isGameOver = true;

        if (_gameType == Constants.GameType.MultiPlay)
        {
            // 흑돌(방장)만 서버에 결과 보고
            if (UserData.Instance.IsBlack)
            {
                string myEmail = UserData.Instance.Email;
                string opponentEmail = UserData.Instance.OpponentEmail;

                StartCoroutine(ReportGameResult(myEmail, opponentEmail, isWin, () =>
                {
                    UserData.Instance.ClearOpponent();
                }));
            }
            else
            {
                Debug.Log("백 플레이어는 결과 보고 안 함 → UI만 닫음");
                UserData.Instance.ClearOpponent();
            }
        }
        else
        {
            Debug.Log("싱글/듀얼 모드 → 서버 보고 생략");
        }
    }


    private IEnumerator ReportGameResult(string myEmail, string opponentEmail, bool isWin, System.Action onComplete)
    {
        // 불러오는 방식이 안 먹혀서 주소를 직접쓰는 방식을 썻었는데, 101.79.11.181:3000로 포트 바뀌니까 불러오는 방식이 가능해짐. 
        string url = $"{ServerUrl}/game/result";   
        WWWForm form = new WWWForm();
        form.AddField("winner", isWin ? myEmail : opponentEmail);
        form.AddField("loser", isWin ? opponentEmail : myEmail);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("게임 결과 반영 성공 " + www.downloadHandler.text);

                // 내 최신 데이터 갱신
                StartCoroutine(UserData.Instance.RefreshMyData());
            }
            else
            {
                Debug.LogError("게임 결과 반영 실패 " + www.error);
            }
        }

        onComplete?.Invoke();
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

    public void StopCountDown()
    {
        if (countdownRoutine != null)
            StopCoroutine(countdownRoutine);

        countdownPanelInst.GetComponent<ConfirmController>().Hide();

        _gameLogic.StartSetState();               
        StartTurn(PlayerType.PlayerA);            

        ToggleGame(true);
    }

    public void OpenGameResultPanel(string winnerInfo)
    {
        if (_canvas != null && gameResultPanel != null)
        {
            if (!gameResultPanelInst)
                gameResultPanelInst = Instantiate(gameResultPanel, _canvas.transform);

            winnerText.text = winnerInfo;
            countdownText = countdownPanelInst.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            countdownPanelInst.GetComponent<ConfirmController>().Show();
            countdownRoutine = StartCoroutine(UpdateCountdown(currentGameType));
        }
    }

}
