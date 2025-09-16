using UnityEngine;
using UnityEngine.SceneManagement;

public class UserData : Singleton<UserData>
{
    // 로그인한 유저 정보
    public string Email { get; set; }
    public string Nickname { get; set; }
    public int Rank { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }

    // 로그아웃 시 초기화
    public void Clear()
    {
        Email = null;
        Nickname = null;
        Rank = 0;
        Wins = 0;
        Losses = 0;
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode) { }
}