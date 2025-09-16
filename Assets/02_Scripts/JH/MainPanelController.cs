using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static Constants;


public class MainPanelController : MonoBehaviour
{
    [SerializeField] private GameObject selectPlayModePanel;

    public void OnClickPlayButton()
    {
        selectPlayModePanel.SetActive(true);
        // GameManager.Instance.OpenPlayModePanel();
    }
}
