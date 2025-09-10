using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private GameObject confirmPanel;      //종료 확인 패널     
    [SerializeField] private GameObject signinPanel;       //로그인 패널    
    [SerializeField] private GameObject signupPanel;       //회원가입 패널
    
    //게임타입 받아오는 코드 Constant 오류날까봐 아직 구현 안함
    //private Constants.GameType _gameType;
    
    private void Start()
    {
        // 로그인
        var sid = PlayerPrefs.GetString("sid");
        if (string.IsNullOrEmpty(sid))
        {
            OpenSigninPanel();
        }
    }
    
    public void OpenSigninPanel()
    {
        
    }

    private Canvas _canvas;
    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        _canvas = FindFirstObjectByType<Canvas>();

        

    }
}
