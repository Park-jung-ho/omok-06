using System;
using System.Collections;
using HJ;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Constants;

public class GameUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentPlayModeText;

    [SerializeField] private GameObject playerATurnPanel;
    [SerializeField] private GameObject playerBTurnPanel;

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image sandImage;

    [SerializeField] private float TurnTime;

    [SerializeField] private Button playButton;

    public enum GameTurnPanelType { None, ATurn, BTurn }

    private void Start()
    {
        GameManager.Instance.OpenCountdownPanel();

        GameType gameType = GameManager.Instance.GetCurrentPlayMode();

        if (gameType == GameType.SinglePlay)
            currentPlayModeText.text = "싱글 플레이 모드";
        else if(gameType == GameType.DualPlay)
            currentPlayModeText.text = "듀얼 플레이 모드";
        else
            currentPlayModeText.text = "멀티 플레이 모드";

        TurnTime = GameManager.Instance.TurnTime;
    }

    public void OnClickBackButton()
    {
        GameManager.Instance.ToggleGame(false);

        GameManager.Instance.OpenConfirmPanel("게임을 종료하시겠습니까?",
            () =>
            {
                GameManager.Instance.ChangeToMainScene();
            }, null, (bool value) => { GameManager.Instance.ToggleGame(true); }
            );
    }

    public void SetGameTurnPanel(GameTurnPanelType gameTurnPanelType)
    {
        switch (gameTurnPanelType)
        {
            case GameTurnPanelType.None:
                playerATurnPanel.SetActive(false);
                playerBTurnPanel.SetActive(false);
                break;
            case GameTurnPanelType.ATurn:
                playerATurnPanel.SetActive(true);
                playerBTurnPanel.SetActive(false);
                break;
            case GameTurnPanelType.BTurn:
                playerATurnPanel.SetActive(false);
                playerBTurnPanel.SetActive(true);
                break;
        }
    }

    public void UpdateTimerUI(float time)
    {
        if (time < 0)
            time = 0f;

        int seconds = Mathf.FloorToInt(time);
        float milliSeconds = Mathf.FloorToInt((time % 1f) * 100);

        float elapsedTime = 0;
        elapsedTime += time;

        timerText.text = string.Format("{0:00}:{1:00}", seconds, milliSeconds);
        sandImage.fillAmount = (elapsedTime / TurnTime);
    }

    public void OnPlayButton()
    {
        // 버튼 연결
        if (GameManager._gameType == GameType.MultiPlay)
        {
            var logic = GameManager.Instance.GameLogic;
            if (logic?.CurrentPlayerState is MultiplayerState multi)
            {
                if (!multi.IsMyTurn)
                    return;
            }
        }

        GameManager.Instance.GameLogic?.ConfirmPlay();
    }

    public void OnAbstainButton()
    {
        GameManager.Instance.ToggleGame(false);
        // 멀티플레이일 때
        if (GameManager._gameType == Constants.GameType.MultiPlay)
        {
            // 내 턴 여부 확인
            var logic = GameManager.Instance.GameLogic;
            if (logic?.CurrentPlayerState is MultiplayerState multi)
            {
                if (!multi.IsMyTurn)
                    return;
            }

            // 여기서 바로 EndGame(false) 하지 말고 서버로 -1 전송하는 함수 호출
            GameManager.Instance.OpenSurrenderConfirm();
        }
        else
        {
           GameManager.Instance.thisRoundResult = GameLogic.GameResult.Abstain;

           GameManager.Instance.OpenConfirmPanel("기권하시겠습니까?", GameManager.Instance.OpenGameResultPanel, () =>
            {
                GameManager.Instance.ToggleGame(true);
            });
        }
    }
    public void SetPlayButtonActive(bool value)
    {
        playButton.interactable = value;
    }
}
