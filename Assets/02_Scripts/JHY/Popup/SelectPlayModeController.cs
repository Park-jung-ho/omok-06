using UnityEngine;
using static Constants;
using UnityEngine.UI;


public class SelectPlayModeController : PanelController
{
    [SerializeField] private Button singleButton;
    [SerializeField] private Button dualButton;
    [SerializeField] private Button multiButton;

    public void OnClickCloseButton()
    {
        Hide();
    }
    public void OnClickSinglePlayButton()
    {
        GameManager.Instance.OpenSelectPlayerOrderDifficultyPanel(1);
    }

    public void OnClickMultiPlayButton()
    {
        MatchingPopupController.OpenPopup(); // 팝업 열기
        MatchingManager.Instance.OnClickMultiPlay(); // 매칭 시작
    }

    public void OnClickDualPlayButton()
    {
        GameManager.Instance.OpenSelectPlayerOrderPanel(2);
    }
}
