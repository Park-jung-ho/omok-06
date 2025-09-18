using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static Constants;


public class MainPanelController : MonoBehaviour
{
    [SerializeField] private GameObject selectPlayModePanel;
    private GameObject playModePanelInst;

    [SerializeField] private Button singleButton;
    [SerializeField] private Button dualButton;
    [SerializeField] private Button multiButton;

    private void Start()
    {
        singleButton.onClick.AddListener(OnClickSinglePlayButton);
        dualButton.onClick.AddListener(OnClickDualPlayButton);
        multiButton.onClick.AddListener(OnClickMultiPlayButton);
    }

    public void OnClickPlayButton()
    {
        //selectPlayModePanel.SetActive(true);
        // GameManager.Instance.OpenPlayModePanel();
    }

    public void OnClickSinglePlayButton()
    {
        GameManager.Instance.ChangeToGameScene(Constants.GameType.SinglePlay);
    }
    
    public void OnClickMultiPlayButton()
    {
        MatchingPopupController.OpenPopup(); // 팝업 열기
        MatchingManager.Instance.OnClickMultiPlay(); // 매칭 시작
    }

    public void OnClickDualPlayButton()
    {
        GameManager.Instance.ChangeToGameScene(Constants.GameType.DualPlay);
    }

    public void OpenPlayModePanel()
    {
        Canvas canvas = GameManager.Instance._canvas;

        if (canvas != null && selectPlayModePanel != null)
        {
            if (!playModePanelInst)
                playModePanelInst = Instantiate(selectPlayModePanel, canvas.transform);

            playModePanelInst.GetComponent<ConfirmController>().Show();
        }
    }
}
