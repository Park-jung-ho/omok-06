using TMPro;
using UnityEngine;

public class SelectPlayerOrderController : PanelController
{
    [SerializeField] private TextMeshProUGUI playerAText;
    [SerializeField] private TextMeshProUGUI playerBText;

    private bool turn;

    public void OnClickSwitchButton()
    {
        GameManager.Instance.TurnSwitch();

        turn = !turn;

        if (!turn)
        {
            playerAText.text = "Username1";
            playerBText.text = "Username2";
        }
        else
        {
            playerAText.text = "Username2";
            playerBText.text = "Username1";
        }
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
