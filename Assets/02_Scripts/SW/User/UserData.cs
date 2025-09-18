using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;

public class UserData : Singleton<UserData>
{
    // 내 정보
    public string Email { get; set; }
    public string Nickname { get; set; }
    public int Rank { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }

    // 상대 정보
    public string OpponentEmail { get; set; }
    public string OpponentNickname { get; set; }
    public int OpponentRank { get; set; }
    public int OpponentWins { get; set; }
    public int OpponentLosses { get; set; }

    // true면 흑, false면 백
    public bool IsBlack { get; set; }   


    // 내 정보 최신화
    public IEnumerator RefreshMyData(System.Action onComplete = null)
    {
        yield return RefreshUserData(Email, isOpponent: false, onComplete);
    }


    // 상대방 정보 최신화
    public IEnumerator RefreshOpponentData(System.Action onComplete = null)
    {
        yield return RefreshUserData(OpponentEmail, isOpponent: true, onComplete);
    }

    // 공통 유저 데이터 요청
    private IEnumerator RefreshUserData(string targetEmail, bool isOpponent, System.Action onComplete)
    {
        if (string.IsNullOrEmpty(targetEmail))
        {
            Debug.LogWarning($"{(isOpponent ? "상대" : "내")} Email 없음 → UserData 갱신 불가");
            yield break;
        }

        // string url = $"{Constants.ServerUrl}/users/{targetEmail}";
        // Debug.Log($"[UserData] 요청 URL = {url}, targetEmail = {targetEmail}");

        string encodedEmail = UnityWebRequest.EscapeURL(targetEmail);
        string url = $"{Constants.ServerUrl}/users/{encodedEmail}";
        Debug.Log($"[UserData] 요청 URL = {url}, targetEmail = {targetEmail}, encodedEmail = {encodedEmail}");

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var data = JsonUtility.FromJson<UserResponse>(www.downloadHandler.text);

                if (isOpponent)
                {
                    OpponentNickname = data.nickname;
                    OpponentRank = data.rank;
                    OpponentWins = data.wins;
                    OpponentLosses = data.losses;
                    Debug.Log("상대 UserData 갱신 완료");
                }
                else
                {
                    Nickname = data.nickname;
                    Rank = data.rank;
                    Wins = data.wins;
                    Losses = data.losses;
                    Debug.Log("내 UserData 갱신 완료");
                }
            }
            else
            {
                Debug.LogError($"{(isOpponent ? "상대" : "내")} UserData 갱신 실패: {www.error}");
            }
        }

        onComplete?.Invoke();
    }

    // 상대방 정보만 초기화
    public void ClearOpponent()
    {
        OpponentEmail = null;
        OpponentNickname = null;
        OpponentRank = 0;
        OpponentWins = 0;
        OpponentLosses = 0;

        Debug.Log("상대 UserData 초기화 완료");
    }

    [System.Serializable]
    private class UserResponse
    {
        public string nickname;
        public int rank;
        public int wins;
        public int losses;
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode) { }
}
