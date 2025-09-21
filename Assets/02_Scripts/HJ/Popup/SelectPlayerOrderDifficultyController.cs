using TMPro;
using UnityEngine;
using static Constants;

public class SelectPlayerOrderDifficultyController : PanelController
{
    [SerializeField] private TextMeshProUGUI playerAText;
    [SerializeField] private TextMeshProUGUI playerBText;
    [SerializeField] private TextMeshProUGUI difficultyText;
    [SerializeField, ReadOnly] private AIDifficultyType difficultyType;
    private int currentIndex;

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
        GameManager.Instance.aiDifficultyType = difficultyType;
        GameManager.Instance.ChangeToGameScene(GameManager.Instance.currentGameType);
    }

    public void OnClickCloseButton()
    {
        Hide();
    }

    public void OnClickLeftButton()
    {
        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = 2;
        }

        difficultyText.text = IndexToDifficulty(currentIndex).ToString();
    }

    public void OnClickRightButton()
    {
        currentIndex++;
        if (currentIndex > 2)
        {
            currentIndex = 0;
        }

        difficultyText.text = IndexToDifficulty(currentIndex).ToString();
    }

    private AIDifficultyType IndexToDifficulty(int index)
    {
        switch (index)
        {
            case 0:
                difficultyType = AIDifficultyType.Easy;
                return difficultyType;
            case 1:
                difficultyType = AIDifficultyType.Normal;
                return difficultyType;
            case 2:
                difficultyType = AIDifficultyType.Hard;
                return difficultyType;
            default:
                return difficultyType;
        }
    }
}
