using TMPro;
using UnityEngine;

public class ConfirmController : PanelController
{
    [SerializeField] private TMP_Text messageText;


    // Confirm 버튼 클릭시 호출될 델리게이트
    public delegate void OnConfirmButtonClickd();
    private OnConfirmButtonClickd _onConfirmButtonClickd;

    public void Show(string message, OnConfirmButtonClickd onConfirmButtonClickd)
    {
        messageText.text = message;
        _onConfirmButtonClickd = onConfirmButtonClickd;
        base.Show(); // 둘 다 가지고 있기 때문에 base.을 붙여서 부모의 Show()를 호출한다.
    }


    /// <summary>
    /// 확인 버튼 클릭시 호출되는 메서드
    /// </summary>
    public void OnClickConfirmButton()
    {
        Hide(() =>
        {
            _onConfirmButtonClickd?.Invoke(); // 델리게이트가 null이 아닐 때만 호출
        });
    }


    /// <summary>
    /// X 버튼 클릭시 호출되는 메서드
    /// </summary>
    public void OnClickCloseButton()
    {
        Hide();
    }
}
