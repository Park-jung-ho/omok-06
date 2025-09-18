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

        // 게임 모드에 따라 상대 정보 다르게 세팅
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
            if (!GameManager.Instance.isSwitched)
            {

            }
            else
            {


            }
            // 멀티플레이일 경우 → 초기값
            // 아직 미구현이라 닉네임, 랭크 따로 세팅 안함
            playerBNicknameText.text = "???";
            playerBRankText.text = "-";
        }
    }

    /// <summary>
    /// 멀티플레이 상대 정보 서버에서 받아와 세팅
    /// </summary>
    public void SetOpponentInfo(string nickname, int rank)
    {
        playerBNicknameText.text = nickname;
        playerBRankText.text = $"{rank}급";
    }
}
