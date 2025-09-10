using TMPro;
using UnityEngine;

public struct SigninData
{
    public string email;
    public string password;
}

public struct SigninResult
{
    public int result;
}

public class SigninPanelController : PanelController
{
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private TMP_InputField passwordInputField;

    public void OnClickConfirmButton()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Shake();
            return;
        }

        var signinData = new SigninData();
        signinData.email = email;
        signinData.password = password;

        // Signin 함수로 email, Password 전달하면서 로그인 요청
        StartCoroutine(NetworkManager.Instance.Signin(signinData,
            success: () =>
            {
                Hide();
            },
            failure: (result) =>
            {
                if (result == 0)
                {
                    GameManager.Instance.OpenConfirmPanel("이메일이 유효하지 않습니다.", () =>
                    {
                        emailInputField.text = "";
                        passwordInputField.text = "";
                    });
                }
                else if (result == 1)
                {
                    GameManager.Instance.OpenConfirmPanel("패스워드가 유효하지 않습니다.", () =>
                    {
                        passwordInputField.text = "";
                    });
                }
            }));
    }

    public void OnClickSignupButton()
    {
        // GameManager를 통해 회원가입 팝업 열기
        GameManager.Instance.OpenSignupPanel();
    }
}
