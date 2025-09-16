using HJ;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    [SerializeField] private GameObject playerATurnPanel;
    [SerializeField] private GameObject playerBTurnPanel;

    [SerializeField] private TextMeshProUGUI playerATimer;
    [SerializeField] private TextMeshProUGUI playerBTimer;

    public enum GameTurnPanelType { None, ATurn, BTurn }


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

    public void UpdateTimerUI(float time, Constants.PlayerType playerType)
    {
        if(time < 0)
            time = 0f;

        int seconds = Mathf.FloorToInt(time);
        float milliSeconds = Mathf.FloorToInt((time % 1f) * 100);

        if (playerType == Constants.PlayerType.PlayerA)
        {
            playerATimer.text = string.Format("{0:00}:{1:00}", seconds, milliSeconds);
        }
        else
        {
            playerBTimer.text = string.Format("{0:00}:{1:00}", seconds, milliSeconds);
        }
    }

    public void OnPlayButton()
    {
        // 버튼 연결
        GameManager.Instance.GameLogic?.ConfirmPlay();
    }
}