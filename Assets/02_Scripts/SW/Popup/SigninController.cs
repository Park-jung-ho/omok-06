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
            Shake(); // PanelController�� ���� ȿ��
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
                GameManager.Instance.OpenConfirmPanel("�α��� ����!", () =>
                {
                    Hide(); // �α��� �г� �ݱ�
                    // TODO: �α��� ���� �� �ٸ� �г� ���� (��: ���� �޴�)
                });
            },
            failure: (statusCode) =>
            {
                if (statusCode == 400) // �̸��� ���� �߸���
                {
                    GameManager.Instance.OpenConfirmPanel("�̸��� ������ �߸��Ǿ����ϴ�.", () =>
                    {
                        emailInputField.text = "";
                        passwordInputField.text = "";
                    });
                }
                else if (statusCode == 401) // �߸��� ���� or ��й�ȣ
                {
                    // ������ INVALID_EMAIL(0)�� INVALID_PASSWORD(1)�� �����ؼ� �����ִϱ�
                    // �ʿ��ϸ� ���� body �Ľ��ؼ� �и��� ���� ����
                    GameManager.Instance.OpenConfirmPanel("�̸��� �Ǵ� ��й�ȣ�� �߸��Ǿ����ϴ�.", () =>
                    {
                        passwordInputField.text = "";
                    });
                }
                else
                {
                    GameManager.Instance.OpenConfirmPanel("�α��� ����, �ٽ� �õ����ּ���.", () => { });
                }
            }));
    }

    public void OnClickSignupButton()
    {
        GameManager.Instance.OpenSignupPanel(); // ȸ������ �г� ����
    }
}
