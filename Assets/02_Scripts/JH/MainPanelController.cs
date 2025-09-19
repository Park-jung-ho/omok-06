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

    public void OnClickPlayButton()
    {
        //selectPlayModePanel.SetActive(true);
        OpenPlayModePanel();
    }

    public void OpenPlayModePanel()
    {
        Canvas canvas = GameManager.Instance._canvas;

        if (canvas != null && selectPlayModePanel != null)
        {
            if (!playModePanelInst)
                playModePanelInst = Instantiate(selectPlayModePanel, canvas.transform);

            playModePanelInst.GetComponent<SelectPlayModeController>().Show();
        }
    }
}
