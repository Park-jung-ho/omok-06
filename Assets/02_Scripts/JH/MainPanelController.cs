using UnityEngine;

public class MainPanelController : MonoBehaviour
{
    [SerializeField] private GameObject selectPlayModePanel;

    public void OnClickPlayButton()
    {
        selectPlayModePanel.SetActive(true);
        // GameManager.Instance.OpenPlayModePanel();
    }

    public void OnClickSinglePlayButton()
    {
        GameManager.Instance.ChangeToGameScene(Constants.GameType.SinglePlay);
    }
    
    public void OnClickMultiPlayButton()
    {       
        MatchingManager.Instance.OnClickMultiPlay();
    }

    public void OnClickDualPlayButton()
    {
        GameManager.Instance.ChangeToGameScene(Constants.GameType.DualPlay);
    }

}
