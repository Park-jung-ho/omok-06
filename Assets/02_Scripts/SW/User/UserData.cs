using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class UserData : Singleton<UserData>
{
    // 내 정보
    public string Email { get; set; }
    public string Nickname { get; set; }
    public int Rank { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Points { get; set; }

    // 상대 정보
    public string OpponentEmail { get; set; }
    public string OpponentNickname { get; set; }
    public int OpponentRank { get; set; }
    public int OpponentWins { get; set; }
    public int OpponentLosses { get; set; }
    public int OpponentPoints { get; set; }

    // true면 흑, false면 백
    public bool IsBlack { get; set; }

    // 리플레이 기록
    public ReplayController.ReplayData replayData;

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
            yield break;
        }

        // string url = $"{Constants.ServerUrl}/users/{targetEmail}";

        string encodedEmail = UnityWebRequest.EscapeURL(targetEmail);
        string url = $"{Constants.ServerUrl}/users/{encodedEmail}";

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
                    OpponentPoints = data.points;

                    replayData.playersDatas[1].name = OpponentNickname;
                    replayData.playersDatas[1].rank = OpponentRank;
                }
                else
                {
                    Nickname = data.nickname;
                    Rank = data.rank;
                    Wins = data.wins;
                    Losses = data.losses;
                    Points = data.points;
                }
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
    }

    [System.Serializable]
    private class UserResponse
    {
        public string nickname;
        public int rank;
        public int wins;
        public int losses;
        public int points;
    }

    public void SetReplayData(string otherName, int rank, bool isFirst = true)
    {
        replayData = new ReplayController.ReplayData
        {
            playersDatas = new ReplayController.PlayerData[]
            {
                new ReplayController.PlayerData
                {
                    rank = Rank,
                    name = Nickname,
                    isBlack = isFirst
                },
                new ReplayController.PlayerData
                {
                    rank = rank,
                    name = otherName,
                    isBlack = !isFirst
                }
            },
            replay = new List<ReplayController.BlockData>()
        };
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Main")
        {
            var replayController = FindFirstObjectByType<ReplayController>();
            replayController.AddReplay(replayData);
        }
    }
}
