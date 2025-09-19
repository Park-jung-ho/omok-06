using UnityEngine;
using TMPro;
using static Constants;

public class TurnUIController : MonoBehaviour
{
    [SerializeField] private GameObject blackTurnPanel;
    [SerializeField] private GameObject whiteTurnPanel;
    [SerializeField] private TMP_Text turnInfoText; // "내 차례" / "상대 차례" 표시용

    /// <summary>
    /// 현재 턴에 맞춰 UI 갱신
    /// </summary>
    public void UpdateTurnUI(PlayerType currentTurn)
    {
        bool isBlackTurn = (currentTurn == PlayerType.PlayerA);
        bool iAmBlack = UserData.Instance.IsBlack; // 내 색깔 확인

        // 패널 on/off
        blackTurnPanel.SetActive(isBlackTurn);
        whiteTurnPanel.SetActive(!isBlackTurn);

        // 내 차례 여부 판정
        bool isMyTurn = (isBlackTurn == iAmBlack);

        turnInfoText.text = isMyTurn ? "내 차례입니다" : "상대방 차례";
    }
}
