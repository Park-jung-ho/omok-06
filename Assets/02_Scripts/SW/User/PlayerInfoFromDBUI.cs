using TMPro;
using UnityEngine;

public class PlayerInfoFromDBUI : MonoBehaviour
{
    [SerializeField] private TMP_Text playerANicknameText;
    [SerializeField] private TMP_Text playerARankText;
    [SerializeField] private TMP_Text playerBNicknameText;
    [SerializeField] private TMP_Text playerBRankText;

    public void GameStart()
    {
        // 내 정보 (A 플레이어)
        playerANicknameText.text = UserData.Instance.Nickname;
        playerARankText.text = $"{UserData.Instance.Rank}급";

        // 게임 모드에 따라 상대 정보 표시
        if (GameManager._gameType == Constants.GameType.SinglePlay)
        {
            if (!GameManager.Instance.isSwitched)
            {
                playerANicknameText.text = UserData.Instance.Nickname;
                playerARankText.text = $"{UserData.Instance.Rank}급";

                // 싱글플레이 → 상대는 AI, 같은 급수
                playerBNicknameText.text = "AI";
                playerBRankText.text = $"{UserData.Instance.Rank}급";
            }
            else
            {
                playerANicknameText.text = "AI";
                playerARankText.text = $"{UserData.Instance.Rank}급";

                playerBNicknameText.text = UserData.Instance.Nickname;
                playerBRankText.text = $"{UserData.Instance.Rank}급";
            }
        }
        else if (GameManager._gameType == Constants.GameType.DualPlay)
        {
            if (!GameManager.Instance.isSwitched)
            {
                playerANicknameText.text = "User1";
                playerARankText.text = $"{UserData.Instance.Rank}급";

                playerBNicknameText.text = "User2";
                playerBRankText.text = $"{UserData.Instance.Rank}급";
            }
            else
            {
                playerANicknameText.text = "User2";
                playerARankText.text = $"{UserData.Instance.Rank}급";

                playerBNicknameText.text = "User1";
                playerBRankText.text = $"{UserData.Instance.Rank}급";
            }
        }
        else
        {
            string myInfoName = UserData.Instance.Nickname;
            string myInfoRank = $"{UserData.Instance.Rank}급";

            string oppName = string.IsNullOrEmpty(UserData.Instance.OpponentNickname) ? "???" : UserData.Instance.OpponentNickname;
            string oppRank = (UserData.Instance.OpponentRank > 0) ? $"{UserData.Instance.OpponentRank}급" : "-";

            if (UserData.Instance.IsBlack)
            {
                // 내가 흑 → 위에 나
                playerANicknameText.text = myInfoName;
                playerARankText.text = myInfoRank;

                playerBNicknameText.text = oppName;
                playerBRankText.text = oppRank;
            }
            else
            {
                // 내가 백 → 위에 상대
                playerANicknameText.text = oppName;
                playerARankText.text = oppRank;

                playerBNicknameText.text = myInfoName;
                playerBRankText.text = myInfoRank;
            }

            // Opponent 정보가 아직 없을 때 서버에서 갱신
            if (string.IsNullOrEmpty(UserData.Instance.OpponentNickname))
            {
                StartCoroutine(UserData.Instance.RefreshOpponentData(() =>
                {
                    if (UserData.Instance.IsBlack)
                    {
                        playerBNicknameText.text = UserData.Instance.OpponentNickname;
                        playerBRankText.text = $"{UserData.Instance.OpponentRank}급";
                    }
                    else
                    {
                        playerANicknameText.text = UserData.Instance.OpponentNickname;
                        playerARankText.text = $"{UserData.Instance.OpponentRank}급";
                    }
                }));
            }
        }
    }
}
