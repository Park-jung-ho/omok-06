using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MultiGameResultController : MonoBehaviour
{
    [SerializeField] private Image[] slotImages;
    [SerializeField] private GameObject winnerInfo;
    [SerializeField] private GameObject loserInfo;
    [SerializeField] private TMP_Text infoText;

    public void ShowPanel(GameLogic.GameResult result, int pointDelta, int currentPoint, int prevPoint)
    {
        GameManager.Instance.ToggleGame(false);

        gameObject.SetActive(true);

        // 승/패 UI
        winnerInfo?.SetActive(result == GameLogic.GameResult.Win);
        loserInfo?.SetActive(result == GameLogic.GameResult.Lose);

        // 슬롯 업데이트
        UpdateSlots(currentPoint, prevPoint, result);

        // 멘트 업데이트
        if (prevPoint == 2 && result == GameLogic.GameResult.Win)
            infoText.text = "승급 하셨습니다!";
        else if (prevPoint == -2 && result == GameLogic.GameResult.Lose)
            infoText.text = "강등 되셨습니다..";
        else
            infoText.text = result == GameLogic.GameResult.Win ? "+1점 획득!" : "-1점 감점..";
    }

    private void UpdateSlots(int currentPoint, int prevPoint, GameLogic.GameResult result)
    {
        foreach (var slot in slotImages)
            slot.gameObject.SetActive(false);

        // 승급 직후
        if (prevPoint == 2 && result == GameLogic.GameResult.Win)
        {
            for (int i = 0; i < 3; i++)
                slotImages[3 + i].gameObject.SetActive(true);
            return;
        }

        // 강등 직후
        if (prevPoint == -2 && result == GameLogic.GameResult.Lose)
        {
            for (int i = 0; i < 3; i++)
                slotImages[2 - i].gameObject.SetActive(true);
            return;
        }

        // 0 → 1 (첫 승리) 케이스
        if (prevPoint == 0 && result == GameLogic.GameResult.Win)
        {
            slotImages[3].gameObject.SetActive(true); // 오른쪽 첫 칸
            return;
        }

        // 0 → -1 (첫 패배) 케이스
        if (prevPoint == 0 && result == GameLogic.GameResult.Lose)
        {
            slotImages[2].gameObject.SetActive(true); // 왼쪽 첫 칸
            return;
        }

        // 일반 케이스
        if (currentPoint > 0)
        {
            for (int i = 0; i < currentPoint; i++)
                slotImages[3 + i].gameObject.SetActive(true);
        }
        else if (currentPoint < 0)
        {
            for (int i = 0; i < Mathf.Abs(currentPoint); i++)
                slotImages[2 - i].gameObject.SetActive(true);
        }
    }

    public void ClosePanel()
    {
        Destroy(gameObject);
        GameManager.Instance.ChangeToMainScene();
    }
}
