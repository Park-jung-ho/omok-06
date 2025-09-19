using System;
using System.Collections;
using HJ;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using static Constants;

public class GameManager : Singleton<GameManager>
{
    public GameType currentGameType { get; private set; }
    public GameLogic.GameResult thisRoundResult { get; set; }
    public PlayerType thisRoundWinner { get; set; }

    public static GameType _gameType;
    public Canvas _canvas { get; private set; }
    private GameLogic _gameLogic;
    private GameUIController _gameUIController;
    private BlockController _blockController;

    private float timer;
    private Coroutine timerCoroutine;
    [SerializeField] private float turnTime = 30f;

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
    private GameObject selectPlayerOrderPanelInst;
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
    }

    public void SetGameTurnPanel(GameUIController.GameTurnPanelType gameTurnPanelType)
    {
        _gameUIController.SetGameTurnPanel(gameTurnPanelType);
    }

    public void StartTurn(PlayerType turn)
    {
        var ui = FindFirstObjectByType<GameUIController>();
        if (ui == null) return;

        if (_gameType == Constants.GameType.MultiPlay)
        {
            bool iAmBlack = UserData.Instance.IsBlack;

            if (turn == Constants.PlayerType.PlayerA)
            {
                if (iAmBlack)
                    ui.SetGameTurnPanel(GameUIController.GameTurnPanelType.ATurn); // 내 턴
                else
                    ui.SetGameTurnPanel(GameUIController.GameTurnPanelType.BTurn); // 상대 턴
            }
            else // PlayerB 턴
            {
                if (iAmBlack)
                    ui.SetGameTurnPanel(GameUIController.GameTurnPanelType.BTurn); // 상대 턴
                else
                    ui.SetGameTurnPanel(GameUIController.GameTurnPanelType.ATurn); // 내 턴
            }
        }
        else
        {
            // 싱글/듀얼 기존 코드
        }

        // 코루틴 실행 전에 반드시 멈춤
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        // 여기가 핵심: 실제 턴 주인의 타입 그대로 넘김
        timerCoroutine = StartCoroutine(TurnTimer(turn));
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

        thisRoundResult = GameLogic.GameResult.Lose;
        _gameLogic.EndGame(thisRoundResult);

        OpenConfirmPanel("타임 오버", () =>
        {
            OpenGameResultPanel();
        }, OpenGameResultPanel);

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
                            winnerText.text = "무승부입니다.\n모든 저라에 돌이 놓였습니다.";
                            break;
                        case GameLogic.GameResult.Abstain:
                            winnerText.text = "기권했습니다.\nAI의 승리입니다.";
                            break;
                    }
                    break;
                case GameType.DualPlay:
                    string winnerName = "";
                    switch (thisRoundResult)
                    {
                        case GameLogic.GameResult.Win:
                            if(!isSwitched)
                                winnerText.text = $"축하드립니다\nUser1님이 승리했습니다";
                            else
                                winnerText.text = $"축하드립니다\nUser2님이 승리했습니다";
                            break;
                        case GameLogic.GameResult.Draw:
                            winnerText.text = "게임의 결과는 무승부입니다.\n모든 자리에 돌이 놓였습니다.";
                            break;
                        case GameLogic.GameResult.Abstain:
                            if (GetOppositePlayerType() == PlayerType.PlayerA)
                                winnerName = isSwitched ? "User2" : "User1";
                            else
                                winnerName = isSwitched ? "User1" : "User2";

                            winnerText.text = $"기권했습니다.\n{winnerName}님의 승리입니다";
                            break;
                    }
                    break;
            }

            gameResultPanelInst.GetComponent<ConfirmController>().Show("", null, ChangeToMainScene);

            GameReset();
        }
    }
    public void SetPlayButtonActive(bool value)
    {
        _gameUIController.SetPlayButtonActive(value);
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
