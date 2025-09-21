using HJ;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TestText : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text tmp;
    public int index;
    [SerializeField] private Sprite blackBlockSprite;
    [SerializeField] private Sprite whiteBlockSprite;
    [SerializeField] private Sprite xBlockSprite;
    [SerializeField] Image image;
    [ReadOnly] public int score;
    public Constants.PlayerType blockType;

    public void OnPointerClick(PointerEventData eventData)
    {
        var blockindex = TestTextGroup.Instance.GetBoardIndex(index);

        if(TestTextGroup.Instance.chooseType == Constants.PlayerType.PlayerA)
        {
            image.sprite = blackBlockSprite;
            blockType = Constants.PlayerType.PlayerA;
            TestTextGroup.Instance.board[blockindex.row, blockindex.col] = Constants.PlayerType.PlayerA;
        }
        else if(TestTextGroup.Instance.chooseType == Constants.PlayerType.PlayerB)
        {
            image.sprite = whiteBlockSprite;
            blockType= Constants.PlayerType.PlayerB;
            TestTextGroup.Instance.board[blockindex.row, blockindex.col] = Constants.PlayerType.PlayerB;
        }
        else
        {
            image.sprite = null;
            TestTextGroup.Instance.board[blockindex.row, blockindex.col] = Constants.PlayerType.None;
        }
        TestTextGroup.Instance.UpdateBoardScore();

        var bannedList = GomokuAI.GetBannedPosList(TestTextGroup.Instance.board);

        foreach(var bannedBlock in bannedList)
        {
            TestTextGroup.Instance.texts[bannedBlock.row * 15 + bannedBlock.col].image.sprite = xBlockSprite;
        }
    }
}
