using System.Collections;
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

    public static Constants.GameType _gameType;
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

    public Constants.PlayerType GetOppositePlayerType()
    {
        Constants.PlayerType currentPlayerType = _gameLogic.GetCurrentPlayerType();

        if (currentPlayerType == PlayerType.PlayerA)
            return Constants.PlayerType.PlayerB;
        else
            return Constants.PlayerType.PlayerA;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }

    public void ChangeToGameScene(Constants.GameType gameType)
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
            _gameLogic = new GameLogic(_blockController, _gameType);
        }
    }

    public void SetGameTurnPanel(GameUIController.GameTurnPanelType gameTurnPanelType)
    {
        _gameUIController.SetGameTurnPanel(gameTurnPanelType);
    }

    public void StartTurn(Constants.PlayerType turn)
    {
        var ui = UnityEngine.Object.FindFirstObjectByType<GameUIController>();
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

        // ✅ 코루틴 실행 전에 반드시 멈춤
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        // 🔑 여기가 핵심: 실제 턴 주인의 타입 그대로 넘김
        timerCoroutine = StartCoroutine(TurnTimer(turn));
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

        OpenConfirmPanel("타임 오버", () =>
        {
            ChangeToMainScene();
        });

        ToggleGame(false);
    }

    public void OpenPlayModePanel()
    {
        if (_canvas != null && playModePanel != null)
        {
            var panel = Instantiate(playModePanel, _canvas.transform);
        }
    }

    public void ToggleGame(bool active)
    {
        if (_blockController != null)
            _blockController.gameObject.SetActive(active);
    }

    // 멀티 게임 종료 처리 (서버에 결과 보고)
    public void EndGame(bool isWin)
    {
        if (isGameOver) return;
        isGameOver = true;

        // 멀티 모드일 때만 서버에 결과 보고
        if (_gameType == Constants.GameType.MultiPlay)
        {
            string myEmail = UserData.Instance.Email;
            string opponentEmail = UserData.Instance.OpponentEmail;

            StartCoroutine(ReportGameResult(myEmail, opponentEmail, isWin, () =>
            {
                UserData.Instance.ClearOpponent(); // 게임이 끝나면 상대 데이터 초기화
            }));
        }
        else
        {
            Debug.Log("싱글/듀얼 모드 → 서버 보고 생략");
        }
    }


    // 게임 결과 서버 반영
    private IEnumerator ReportGameResult(string myEmail, string opponentEmail, bool isWin, System.Action onComplete)
    {
        string url = "http://localhost:3000/game/result";
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
}
