using TMPro;
using UnityEngine;


public struct SignupData
{
    public string email;
    public string password;
    public string nickname;
}

public struct SignupResult
{
    public int result;
}

public class SignupController : PanelController
{
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private TMP_InputField nicknameInputField;

    public void OnClickMakeButton()
    {
        var signupData = new SignupData
        {
            email = emailInputField.text,
            password = passwordInputField.text,
            nickname = nicknameInputField.text
        };

        if (string.IsNullOrEmpty(signupData.email) ||
            string.IsNullOrEmpty(signupData.password) ||
            string.IsNullOrEmpty(signupData.nickname))
        {
            Shake(); // PanelController에 있는 흔들기 효과
            return;
        }

        StartCoroutine(NetworkManager.Instance.Signup(signupData,
            success: () =>
            {
                GameManager.Instance.OpenConfirmPanel("회원가입 성공!", () =>
                {
                    Hide(); // 회원가입 패널 닫기
                    GameManager.Instance.OpenSigninPanel(); // 다시 로그인 패널 열기
                });
            },
            failure: (statusCode) =>
            {
                if (statusCode == 400) // 이메일 형식 오류
                {
                    GameManager.Instance.OpenConfirmPanel("이메일 형식이 잘못되었습니다.", () =>
                    {
                        emailInputField.text = "";
                    });
                }
                else if (statusCode == 409) // 중복 이메일
                {
                    GameManager.Instance.OpenConfirmPanel("이미 존재하는 이메일입니다.", () =>
                    {
                        emailInputField.text = "";
                    });
                }
                else
                {
                    GameManager.Instance.OpenConfirmPanel("회원가입 실패, 다시 시도해주세요.", () => { });
                }
            }));
    }
}
