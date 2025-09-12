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

        StartCoroutine(NetworkManager.Instance.Signin(signinData,
            success: (result) =>
            {
                // ✅ 로그인 성공 시 서버에서 받은 닉네임, 랭크 저장
                UserData.Instance.SetUserInfo(result.nickname, result.rank);

                GameManager.Instance.OpenConfirmPanel("로그인 성공!", () =>
                {
                    Hide(); // 로그인 패널 닫기
                    // TODO: 여기서 MainPanel 열기나 test_game 씬 이동 가능
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
