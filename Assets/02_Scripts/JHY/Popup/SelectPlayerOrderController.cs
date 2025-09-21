using TMPro;
using UnityEngine;
using static Constants;

public class SelectPlayerOrderController : PanelController
{
    [SerializeField] private TextMeshProUGUI playerAText;
    [SerializeField] private TextMeshProUGUI playerBText;

    private bool turn;
    private void OnEnable()
    {
        if (GameManager.Instance.currentGameType == GameType.SinglePlay)
        {
            playerAText.text = UserData.Instance.Nickname;
            playerBText.text = "AI";
        }
        else if (GameManager.Instance.currentGameType == GameType.DualPlay)
        {
            playerAText.text = "User1";
            playerBText.text = "User2";
        }
        else // 멀티 플레이
        {
        }
    }
    public void SetUserName(string playerA, string playerB)
    {
        playerAText.text = playerA;
        playerBText.text = playerB;
    }

    public void OnClickSwitchButton()
    {
        GameManager.Instance.TurnSwitch();

        string temp = playerAText.text;
        playerAText.text = playerBText.text;
        playerBText.text = temp;
    }

    public void OnClickConfirmButton()
    {
        GameManager.Instance.ChangeToGameScene(GameManager.Instance.currentGameType);
    }

    public void OnClickCloseButton()
    {
        Hide();
    }
}
