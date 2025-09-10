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
            Shake(); // PanelController�� �ִ� ���� ȿ��
            return;
        }

        StartCoroutine(NetworkManager.Instance.Signup(signupData,
            success: () =>
            {
                GameManager.Instance.OpenConfirmPanel("ȸ������ ����!", () =>
                {
                    Hide(); // ȸ������ �г� �ݱ�
                    GameManager.Instance.OpenSigninPanel(); // �ٽ� �α��� �г� ����
                });
            },
            failure: (result) =>
            {
                if (result == 0) // INVALID_EMAIL (�ߺ� �̸���)
                {
                    GameManager.Instance.OpenConfirmPanel("�ߺ� �̸��� �Դϴ�.", () =>
                    {
                        emailInputField.text = "";
                    });
                }
                else
                {
                    GameManager.Instance.OpenConfirmPanel("ȸ������ ����, �ٽ� �õ����ּ���.", () => { });
                }
            }));
    }
}