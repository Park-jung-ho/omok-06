using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private GameObject confirmPrefab;           // 확인 패널
    [SerializeField] private GameObject signinPrefab;            // 로그인 패널
    [SerializeField] private GameObject signupPrefab;            // 회원가입 패널
    [SerializeField] private GameObject settingPrefab;            // 설정 패널
    [SerializeField] private GameObject rankingPrefab;            // 랭킹 패널
    //각 패널은 프리팹으로 만들어 어떤 상황에서도 동작하게 만들기
    private GameObject settingPanel;
    
    public static Constants.GameType _gameType;
    
    private Canvas _canvas;
    
    void Start()
    {
        _canvas =  GameObject.Find("Canvas").GetComponent<Canvas>();
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (settingPanel == null)
            {
                settingPanel = Instantiate(settingPrefab, _canvas.transform);
                settingPanel.SetActive(true);
            }
            else
                settingPanel.SetActive(!settingPanel.activeSelf);
        }
    }
    public void ChangeToGameScene(Constants.GameType gameType)
    {
        _gameType = gameType;
        SceneManager.LoadScene("test_game"); //임시로 테스트 씬으로 이동
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        _canvas = FindFirstObjectByType<Canvas>();
        if (scene.name == "")
        {
            
            
        }
    }
}
