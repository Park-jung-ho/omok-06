using TMPro;
using UnityEngine;

public class PlayerInfoFromDBUI : MonoBehaviour
{
    [SerializeField] private TMP_Text playerANicknameText;
    [SerializeField] private TMP_Text playerARankText;
    [SerializeField] private TMP_Text playerBNicknameText;
    [SerializeField] private TMP_Text playerBRankText;

    private void Start()
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
        else if (GameManager._gameType == Constants.GameType.DualPlay)
        {
            if (!GameManager.Instance.isSwitched)
            {

            }
            else
            {


            }
            // 멀티플레이일 경우 → 초기값
            // 아직 미구현이라 닉네임, 랭크 따로 세팅 안함
            playerBNicknameText.text = "???";
            // 듀얼플레이 → 같은 PC 2인 대전
            playerBNicknameText.text = "PlayerB";
            playerBRankText.text = "-";
        }
        else if (GameManager._gameType == Constants.GameType.MultiPlay)
        {
            // 멀티플레이 → 상대 정보 (UserData에서 가져오기)
            if (!string.IsNullOrEmpty(UserData.Instance.OpponentNickname))
            {
                playerBNicknameText.text = UserData.Instance.OpponentNickname;
                playerBRankText.text = $"{UserData.Instance.OpponentRank}급";
            }
            else
            {
                // 아직 서버에서 OpponentData 갱신 전이면 기본값
                playerBNicknameText.text = "???";
                playerBRankText.text = "-";

                // 서버 갱신 시도
                StartCoroutine(UserData.Instance.RefreshOpponentData(() =>
                {
                    playerBNicknameText.text = UserData.Instance.OpponentNickname;
                    playerBRankText.text = $"{UserData.Instance.OpponentRank}급";
                }));
            }
        }
    }
}
