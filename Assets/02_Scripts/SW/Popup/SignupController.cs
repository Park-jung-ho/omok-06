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
    [SerializeField] private TMP_InputField passwordCheckInputField; // 비밀번호 확인 필드 추가
    [SerializeField] private TMP_InputField nicknameInputField;

    public void OnClickMakeButton()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;
        string passwordCheck = passwordCheckInputField.text;
        string nickname = nicknameInputField.text;

        // 입력값 비었는지 검사
        if (string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(password) ||
            string.IsNullOrEmpty(passwordCheck) ||
            string.IsNullOrEmpty(nickname))
        {
            Shake();
            return;
        }

        // 비밀번호 불일치 검사
        if (password != passwordCheck)
        {
            GameManager.Instance.OpenConfirmPanel("비밀번호가 일치하지 않습니다.", () =>
            {
                passwordInputField.text = "";
                passwordCheckInputField.text = "";
            });
            return;
        }

        // 서버에 전달할 비밀번호는 1개만
        var signupData = new SignupData
        {
            email = email,
            password = password,   // 확인된 비밀번호만 보냄
            nickname = nickname
        };

        StartCoroutine(NetworkManager.Instance.Signup(signupData,
            success: () =>
            {
                GameManager.Instance.OpenConfirmPanel("회원가입 성공!", () =>
                {
                    Hide(); // 회원가입 패널 닫기
                    GameManager.Instance.OpenSigninPanel(); // 로그인 패널 열기
                });
            },
            failure: (statusCode) =>
            {
                if (statusCode == 400)
                {
                    GameManager.Instance.OpenConfirmPanel("이메일 형식이 잘못되었습니다.", () =>
                    {
                        emailInputField.text = "";
                    });
                }
                else if (statusCode == 409)
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

    public void OnClickCloseButton()
    {
        Destroy(gameObject);
    }
}
