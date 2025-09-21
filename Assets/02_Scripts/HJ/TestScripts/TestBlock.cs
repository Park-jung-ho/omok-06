using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace HJ
{
    public class TestBlock : MonoBehaviour, IPointerClickHandler
    {
        public int blockIndex;
        private Image blockImage;
        [SerializeField] private Sprite noneSprite;
        [SerializeField] private Sprite blackBlockSprite;
        [SerializeField] private Sprite whiteBlockSprite;


        public void OnPointerClick(PointerEventData eventData)
        {
            var boardIndex = TestGameManager.Instance.GetBoardIndex(blockIndex);

            if (TestGameManager.Instance.gameLogic.board[boardIndex.row, boardIndex.col] != Constants.PlayerType.None)
            {
                blockImage.sprite = noneSprite;
                TestGameManager.Instance.gameLogic.board[boardIndex.row, boardIndex.col] = Constants.PlayerType.None;
                return;
            }


            if (TestGameManager.Instance.playerType == Constants.PlayerType.PlayerA)
            {
                TestGameManager.Instance.lastBlock = this;
                blockImage.sprite = blackBlockSprite;
                TestGameManager.Instance.gameLogic.board[boardIndex.row, boardIndex.col] = Constants.PlayerType.PlayerA;
            }
            else if (TestGameManager.Instance.playerType == Constants.PlayerType.PlayerB)
            {
                TestGameManager.Instance.lastBlock = this;
                blockImage.sprite = whiteBlockSprite;
                TestGameManager.Instance.gameLogic.board[boardIndex.row, boardIndex.col] = Constants.PlayerType.PlayerB;
            }
        }

        public void ChangeSprite(Constants.PlayerType blockType)
        {
            switch (blockType)
            {
                case Constants.PlayerType.PlayerA:
                    blockImage.sprite = blackBlockSprite;
                    break;
                case Constants.PlayerType.PlayerB:
                    blockImage.sprite = whiteBlockSprite;
                    break;
                case Constants.PlayerType.None:
                    blockImage.sprite = noneSprite;
                    break;
            }
        }

        private void Awake()
        {
            blockImage = GetComponent<Image>();
        }
    }
    public static class BoardData
    {
        public const int row = 15, col = 15;
    }

    public class GameLogic
    {
        public Constants.PlayerType[,] board;

        public void InitBoard()
        {
            board = new Constants.PlayerType[BoardData.row, BoardData.col];
        }

    }
}
