using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using static Constants;

public class GameManager : Singleton<GameManager>
{
    public GameType currentGameType { get; private set; }
    public GameLogic.GameResult thisRoundResult { get; set; }
    public PlayerType thisRoundWinner { get; set; }

    public AIDifficultyType aiDifficultyType;

    public static GameType _gameType;
    public Canvas _canvas { get; private set; }
    private GameLogic _gameLogic;
    private GameUIController _gameUIController;
    private BlockController _blockController;

    private float timer;
    private Coroutine timerCoroutine;

    [SerializeField] private float turnTime;
    public float TurnTime
    {
        get { return turnTime; }
    }

    private bool isGameOver = false;

    public GameLogic GameLogic => _gameLogic;
    private GameObject _playerInfoFromDBUI;

    // 전환 여부(true면 PlayerB가 선공)
    public bool isSwitched { get; private set; }

    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private GameObject signinPanel;
    [SerializeField] private GameObject signupPanel;
    [SerializeField] private GameObject rankingPanel;

    // 카운트다운
    [SerializeField] private GameObject countdownPanel;
    private GameObject countdownPanelInst;
    private TextMeshProUGUI countdownText;
    private Coroutine countdownRoutine;

    // 흑/백 선택
    [SerializeField] private GameObject selectPlayerOrderPanel;
    [SerializeField] private GameObject selectPlayerOrderDifficultyPanel;
    private GameObject selectPlayerOrderPanelInst;
    private GameObject selectPlayerOrderDifficultyPanelInst;
    private TextMeshProUGUI playerAText;                // 선공
    private TextMeshProUGUI playerBText;                // 후공

    // 게임 결과
    [SerializeField] private GameObject gameResultPanel;
    private GameObject gameResultPanelInst;
    private TextMeshProUGUI winnerText;

    // 멀티 게임 결과
    [SerializeField] private GameObject multiGameResultPanel;
    private GameObject multiGameResultPanelInst;
    private TextMeshProUGUI winnerInfoText;
    // 멀티 Game Result 팝업

    public void TurnSwitch()
    {
        isSwitched = !isSwitched;
    }

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

    public GameType GetCurrentPlayMode()
    {
        return _gameLogic.currnetPlayMode;
    }

    public bool IsMyTurn(int myType)
    {
        Constants.PlayerType currentPlayerType = _gameLogic.GetCurrentPlayerType();
        return myType == (int)currentPlayerType;
    }

    public void GameReset()
    {
        if (_blockController == null || _gameLogic == null)
            return;

        _blockController.ResetRound();
        TurnTimerReset();
        thisRoundResult = GameLogic.GameResult.None;
        thisRoundWinner = PlayerType.None;
        _gameLogic.BoardReset();
        StopCountDown();
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
        isGameOver = false; // 게임오버를 초기화하지 않으면 db에 결과가 보고되지 않음
        SceneManager.LoadScene("Game");
    }

    public void ChangeToMainScene()
    {
        _gameLogic = null;
        SceneManager.LoadScene("Main");
    }

    public void OpenConfirmPanel(string message, ConfirmController.OnConfirmButtonClickd onConfirmButtonClicked, ConfirmController.OnCloseButtonClicked onCloseButtonClicked = null, ConfirmController.OnCloseButtonClickedBool onCloseButtonClickedBool = null)
    {
        if (_canvas != null)
        {
            var confirmPanelObject = Instantiate(confirmPanel, _canvas.transform);
            confirmPanelObject.GetComponent<ConfirmController>().Show(message, onConfirmButtonClicked, onCloseButtonClicked, onCloseButtonClickedBool);
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
        else if (scene.name == "Main")
        {
            GameReset();
            isSwitched = false;
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

        // 턴 UI 갱신
        FindFirstObjectByType<TurnUIController>()?.UpdateTurnUI(turn);
    }

    public void TurnTimerReset()
    {
        StopCoroutine(timerCoroutine);
        timer = turnTime;
        _gameUIController.UpdateTimerUI(timer);
    }

    private IEnumerator TurnTimer(PlayerType playerType)
    {
        timer = turnTime;

        while (timer > 0f)
        {
            timer -= Time.deltaTime;

            // 씬 전환되면 바로 멈추도록 체크 후 브레이크
            if (_gameUIController == null || !_gameUIController.isActiveAndEnabled)
                yield break;

            _gameUIController.UpdateTimerUI(timer);
            yield return null;
        }

        if (_gameUIController == null) yield break;

        thisRoundResult = GameLogic.GameResult.TimeOver;
        _gameLogic.EndGame(thisRoundResult);

        ToggleGame(false);

        TurnTimerReset();
    }


    public void ToggleGame(bool active)
    {
        _blockController.gameObject.SetActive(active);
    }

    public void OpenCountdownPanel()
    {
        ToggleGame(false);

        timer = turnTime;
        _gameUIController.UpdateTimerUI(timer);

        _playerInfoFromDBUI = GameObject.FindGameObjectWithTag("PlayerDB");
        _playerInfoFromDBUI.GetComponent<PlayerInfoFromDBUI>().GameStart();

        if (_canvas != null && countdownPanel != null)
        {
            if (!countdownPanelInst)
                countdownPanelInst = Instantiate(countdownPanel, _canvas.transform);

            countdownText = countdownPanelInst.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            countdownPanelInst.GetComponent<ConfirmController>().Show();
            countdownRoutine = StartCoroutine(UpdateCountdown(currentGameType));
        }
    }


    // 멀티 게임 종료 처리 (서버 보고 + 결과창)
    public void EndGame(bool isWin)
    {
        if (isGameOver) return;
        isGameOver = true;

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        if (_gameType == Constants.GameType.MultiPlay)
        {
            int prevPoint = UserData.Instance.Points; // 최신화 전 값 저장

            if (UserData.Instance.IsBlack)
            {
                string myEmail = UserData.Instance.Email;
                string opponentEmail = UserData.Instance.OpponentEmail;

                StartCoroutine(ReportGameResult(myEmail, opponentEmail, isWin, () =>
                {
                    UserData.Instance.ClearOpponent();

                    // 최신화 후 결과창
                    StartCoroutine(UserData.Instance.RefreshMyData(() =>
                    {
                        ShowMultiResultUI(isWin, prevPoint, UserData.Instance.Points);
                    }));
                }));
            }
            else
            {
                UserData.Instance.ClearOpponent();

                StartCoroutine(UserData.Instance.RefreshMyData(() =>
                {
                    ShowMultiResultUI(isWin, prevPoint, UserData.Instance.Points);
                }));
            }
        }
    }

    private void ShowMultiResultUI(bool isWin, int prevPoint, int currentPoint)
    {
        int pointDelta = isWin ? 1 : -1;
        var result = isWin ? GameLogic.GameResult.Win : GameLogic.GameResult.Lose;

        if (_canvas != null && multiGameResultPanel != null)
        {
            var panel = Instantiate(multiGameResultPanel, _canvas.transform);
            var controller = panel.GetComponent<MultiGameResultController>();
            if (controller != null)
            {
                controller.ShowPanel(result, pointDelta, currentPoint, prevPoint);
            }
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

    public void OpenSelectPlayerOrderDifficultyPanel(int playMode)
    {
        currentGameType = (GameType)playMode;

        if (_canvas != null && selectPlayerOrderPanel != null)
        {
            if (!selectPlayerOrderDifficultyPanelInst)
                selectPlayerOrderDifficultyPanelInst = Instantiate(selectPlayerOrderDifficultyPanel, _canvas.transform);

            selectPlayerOrderDifficultyPanelInst.GetComponent<SelectPlayerOrderDifficultyController>().Show();
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

        StopCountDown(true);
    }

    public void StopCountDown(bool restart = false)
    {
        if (countdownRoutine != null)
            StopCoroutine(countdownRoutine);

        if (restart)
        {
            countdownPanelInst.GetComponent<ConfirmController>().Hide();

            StartTurn(PlayerType.PlayerA);
            _gameLogic.StartSetState();

            ToggleGame(true);
        }
    }

    public void OpenGameResultPanel()
    {
        if (_canvas != null && gameResultPanel != null)
        {
            if (!gameResultPanelInst)
                gameResultPanelInst = Instantiate(gameResultPanel, _canvas.transform);

            winnerText = gameResultPanelInst.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

            switch (currentGameType)
            {
                case GameType.SinglePlay:
                    switch (thisRoundResult)
                    {
                        case GameLogic.GameResult.Win:
                            winnerText.text = "축하드립니다\n당신이 승리했습니다";
                            break;
                        case GameLogic.GameResult.Lose:
                            winnerText.text = "아쉽게도 AI가 승리했습니다";
                            break;
                        case GameLogic.GameResult.Draw:
                            winnerText.text = "결과는 무승부입니다.\n더 이상 돌을 놓을 수 없습니다.";
                            break;
                        case GameLogic.GameResult.Abstain:
                            winnerText.text = "기권했습니다.\nAI의 승리입니다.";
                            break;
                        case GameLogic.GameResult.TimeOver:
                            winnerText.text = "타임 오버입니다.\n아쉽게도 AI가 승리했습니다";
                            break;
                    }
                    break;
                case GameType.DualPlay:
                    string winnerName = "";
                    switch (thisRoundResult)
                    {
                        case GameLogic.GameResult.Win:
                            if (!isSwitched)
                                winnerText.text = $"축하드립니다\nUser1님이 승리했습니다";
                            else
                                winnerText.text = $"축하드립니다\nUser2님이 승리했습니다";
                            break;
                        case GameLogic.GameResult.Draw:
                            winnerText.text = "결과는 무승부입니다.\n더 이상 돌을 놓을 수 없습니다.";
                            break;
                        case GameLogic.GameResult.Abstain:
                            if (GetOppositePlayerType() == PlayerType.PlayerA)
                                winnerName = isSwitched ? "User2" : "User1";
                            else
                                winnerName = isSwitched ? "User1" : "User2";

                            winnerText.text = $"기권했습니다.\n{winnerName}님의 승리입니다";
                            break;

                        case GameLogic.GameResult.TimeOver:
                            if (GetOppositePlayerType() == PlayerType.PlayerA)
                                winnerName = isSwitched ? "User2" : "User1";
                            else
                                winnerName = isSwitched ? "User1" : "User2";

                            winnerText.text = $"타임 오버\n{winnerName}님의 승리입니다";
                            break;
                    }
                    break;
            }

            gameResultPanelInst.GetComponent<ConfirmController>().Show("", null, ChangeToMainScene);
        }
    }
    public void SetPlayButtonActive(bool value)
    {
        _gameUIController.SetPlayButtonActive(value);
    }

    public void OpenSurrenderConfirm()
    {
        OpenConfirmPanel("정말 기권하시겠습니까?",
            () =>  // YES 버튼 눌렀을 때 실행할 콜백
            {
                // 기권도 착수처럼 -1 전송
                NetworkManager.Instance.Socket.EmitAsync("doPlayer", new { roomId = MatchingManager.Instance.CurrentRoomId, blockIndex = -1 });
            },
            () =>  // 닫기 버튼 눌렀을 때 (선택 사항)
            {
                Debug.Log("기권 취소");
            });
    }
    public void OpenMultiGameResultPanel()
    {
        if (_canvas != null && multiGameResultPanel != null)
        {
            if (!multiGameResultPanelInst)
                multiGameResultPanelInst = Instantiate(multiGameResultPanel, _canvas.transform);

            winnerInfoText = multiGameResultPanelInst.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

            switch (thisRoundResult)
            {
                case GameLogic.GameResult.None:
                    break;
                case GameLogic.GameResult.Win:
                    break;
                case GameLogic.GameResult.Lose:
                    break;
                case GameLogic.GameResult.Draw:
                    break;
                case GameLogic.GameResult.Abstain:
                    break;
            }
        }
    }
}
