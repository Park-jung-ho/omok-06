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

public class SigninController : PanelController
{
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private TMP_InputField passwordInputField;

    public void OnClickConfirmButton()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Shake(); // PanelController의 흔들기 효과
            return;
        }

        var signinData = new SigninData
        {
            email = email,
            password = password
        };

        StartCoroutine(NetworkManager.Instance.Signin(signinData,
            success: () =>
            {
                GameManager.Instance.OpenConfirmPanel("로그인 성공!", () =>
                {
                    Hide(); // 로그인 패널 닫기
                    // TODO: 로그인 성공 후 다른 패널 열기 (예: 메인 메뉴)
                });
            },
            failure: (statusCode) =>
            {
                if (statusCode == 400) // 이메일 형식 잘못됨
                {
                    GameManager.Instance.OpenConfirmPanel("이메일 형식이 잘못되었습니다.", () =>
                    {
                        emailInputField.text = "";
                        passwordInputField.text = "";
                    });
                }
                else if (statusCode == 401) // 잘못된 계정 or 비밀번호
                {
                    // 서버가 INVALID_EMAIL(0)과 INVALID_PASSWORD(1)을 구분해서 내려주니까
                    // 필요하면 응답 body 파싱해서 분리할 수도 있음
                    GameManager.Instance.OpenConfirmPanel("이메일 또는 비밀번호가 잘못되었습니다.", () =>
                    {
                        passwordInputField.text = "";
                    });
                }
                else
                {
                    GameManager.Instance.OpenConfirmPanel("로그인 실패, 다시 시도해주세요.", () => { });
                }
            }));
    }

    public void OnClickSignupButton()
    {
        GameManager.Instance.OpenSignupPanel(); // 회원가입 패널 열기
    }
}
