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

        // Signin �Լ��� email, Password �����ϸ鼭 �α��� ��û
        StartCoroutine(NetworkManager.Instance.Signin(signinData,
            success: () =>
            {
                Hide();
            },
            failure: (result) =>
            {
                if (result == 0)
                {
                    GameManager.Instance.OpenConfirmPanel("�̸����� ��ȿ���� �ʽ��ϴ�.", () =>
                    {
                        emailInputField.text = "";
                        passwordInputField.text = "";
                    });
                }
                else if (result == 1)
                {
                    GameManager.Instance.OpenConfirmPanel("�н����尡 ��ȿ���� �ʽ��ϴ�.", () =>
                    {
                        passwordInputField.text = "";
                    });
                }
            }));
    }

    public void OnClickSignupButton()
    {
        // GameManager�� ���� ȸ������ �˾� ����
        GameManager.Instance.OpenSignupPanel();
    }
}
