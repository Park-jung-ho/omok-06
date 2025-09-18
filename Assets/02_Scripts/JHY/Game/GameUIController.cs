using System;
using System.Collections;
using HJ;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Constants;

public class GameUIController : MonoBehaviour
{
    [SerializeField] private GameObject playerATurnPanel;
    [SerializeField] private GameObject playerBTurnPanel;

    [SerializeField] private TextMeshProUGUI timerText;

    [SerializeField] private Image sandImage;

    [SerializeField] private Image playerAStoneIcon;   // A 플레이어 돌 아이콘
    [SerializeField] private Image playerBStoneIcon;   // B 플레이어 돌 아이콘
    [SerializeField] private Sprite blackStoneSprite;  // 흑돌 스프라이트
    [SerializeField] private Sprite whiteStoneSprite;  // 백돌 스프라이트

    [SerializeField] private float TurnTime = 30f;

    public enum GameTurnPanelType { None, ATurn, BTurn }
    private void Start()
    {
        GameManager.Instance.OpenCountdownPanel();
    }

    public void OnClickBackButton()
    {
        GameManager.Instance.OpenConfirmPanel("게임을 종료하시겠습니까?",
            () =>
            {
                GameManager.Instance.ChangeToMainScene();
            });
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

    public void UpdateTimerUI(float time, PlayerType playerType)
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

    public void OnPlayButton(int playerType)
    {
        // 버튼 연결
        if (GameManager._gameType == Constants.GameType.MultiPlay)
        {
            var logic = GameManager.Instance.GameLogic;
            if (logic?.CurrentPlayerState is MultiplayerState multi)
            {
                if (!multi.IsMyTurn)
                    return;
            }
        }
        else
        {
            if (!GameManager.Instance.IsMyTurn(playerType))
                return;
        }

        GameManager.Instance.GameLogic?.ConfirmPlay();
    }

    public void OnAbstainButton(int playerType)
    {
        if (!GameManager.Instance.IsMyTurn(playerType))
            return;

        GameManager.Instance.ToggleGame(false);

        GameManager.Instance.OpenConfirmPanel("기권하시겠습니까?", null, () =>
        {
            GameManager.Instance.ToggleGame(true);
        });
    }

    // 돌 색상 초기화
    public void SetStoneIcons(bool isBlack)
    {
        if (isBlack)
        {
            // 내가 흑
            playerAStoneIcon.sprite = blackStoneSprite;
            playerBStoneIcon.sprite = whiteStoneSprite;
        }
        else
        {
            // 내가 백
            playerAStoneIcon.sprite = whiteStoneSprite;
            playerBStoneIcon.sprite = blackStoneSprite;
        }
    }
}
