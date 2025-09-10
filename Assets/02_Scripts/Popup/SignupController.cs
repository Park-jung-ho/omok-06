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

public class SignupPanelController : PanelController
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
            failure: (result) =>
            {
                if (result == 0) // INVALID_EMAIL (중복 이메일)
                {
                    GameManager.Instance.OpenConfirmPanel("중복 이메일 입니다.", () =>
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