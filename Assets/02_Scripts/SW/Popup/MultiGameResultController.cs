using UnityEngine;
using TMPro;

public class MultiGameResultController : MonoBehaviour
{
    [Header("승패 텍스트")]
    [SerializeField] private GameObject winnerInfo;
    [SerializeField] private GameObject loserInfo;

    [Header("UI 컴포넌트")]
    [SerializeField] private TMP_Text infoText;

    public void ShowPanel(GameLogic.GameResult result, int prevPoint, int pointDelta, int newPoint)
    {
        gameObject.SetActive(true);

        // 승/패 UI
        if (winnerInfo != null) winnerInfo.SetActive(result == GameLogic.GameResult.Win);
        if (loserInfo != null) loserInfo.SetActive(result == GameLogic.GameResult.Lose);

        // 승급/강등 판단은 직전 점수 + 변화값으로 확인
        int checkPoint = prevPoint + pointDelta;

        if (checkPoint >= 3)
        {
            infoText.text = "승급 하셨습니다!";
        }
        else if (checkPoint <= -3)
        {
            infoText.text = "강등 되셨습니다..";
        }
        else
        {
            infoText.text = result == GameLogic.GameResult.Win ? "+1점 획득!" : "-1점 감점..";
        }
    }

    public void ClosePanel()
    {
        Destroy(gameObject);
        GameManager.Instance.ChangeToMainScene();
    }
}
