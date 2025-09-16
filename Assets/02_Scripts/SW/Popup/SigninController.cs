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
    public string nickname;
    public int rank;
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

        // NetworkManager.Signin 수정: success 콜백에 SigninResult 전달하도록
        StartCoroutine(NetworkManager.Instance.Signin(signinData,
            success: (result) =>
            {
                // UserData 싱글톤에 값 저장
                UserData.Instance.Email = email;
                UserData.Instance.Nickname = result.nickname;
                UserData.Instance.Rank = result.rank;

                GameManager.Instance.OpenConfirmPanel("로그인 성공!", () =>
                {
                    Hide(); // 로그인 패널 닫기
                    // TODO: 로그인 성공 후 메인 메뉴 패널 열기
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
