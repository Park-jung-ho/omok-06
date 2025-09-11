using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private GameObject confirmPanel;           // 확인 패널
    [SerializeField] private GameObject signinPanel;            // 로그인 패널
    [SerializeField] private GameObject signupPanel;            // 회원가입 패널
    [SerializeField] private GameObject rankingPanel;            // 랭킹 패널
    
    public static Constants.GameType _gameType;
    
    private Canvas _canvas;

    void Awake()
    {
        _canvas = FindFirstObjectByType<Canvas>();
    }

    public void ChangeToGameScene(Constants.GameType gameType)
    {
        _gameType = gameType;
        SceneManager.LoadScene("test_game"); //임시로 테스트 씬으로 이동
    }

    public void OpenconfirmPanel()
    {
        Instantiate(confirmPanel, _canvas.transform);
    }
    
    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        //_canvas = FindFirstObjectByType<Canvas>();
        /*if (scene.name == "")
        {
            
            
        }*/
    }
}
