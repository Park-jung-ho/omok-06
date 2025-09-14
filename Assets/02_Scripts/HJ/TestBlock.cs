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

            if (TestGameManager.Instance.gameLogic.board[boardIndex.row, boardIndex.col] != BlockType.None)
            {
                blockImage.sprite = noneSprite;
                TestGameManager.Instance.gameLogic.board[boardIndex.row, boardIndex.col] = BlockType.None;
                return;
            }


            if (TestGameManager.Instance.playerType == BlockType.Black)
            {
                TestGameManager.Instance.lastBlock = this;
                blockImage.sprite = blackBlockSprite;
                TestGameManager.Instance.gameLogic.board[boardIndex.row, boardIndex.col] = BlockType.Black;
            }
            else
            {
                TestGameManager.Instance.lastBlock = this;
                blockImage.sprite = whiteBlockSprite;
                TestGameManager.Instance.gameLogic.board[boardIndex.row, boardIndex.col] = BlockType.White;
            }
        }

        public void ChangeSprite(BlockType blockType)
        {
            switch (blockType)
            {
                case BlockType.Black:
                    blockImage.sprite = blackBlockSprite;
                    break;
                case BlockType.White:
                    blockImage.sprite = whiteBlockSprite;
                    break;
                case BlockType.None:
                    blockImage.sprite = noneSprite;
                    break;
            }
        }

        private void Awake()
        {
            blockImage = GetComponent<Image>();
        }
    }

    public enum BlockType { None, Black, White }
    public static class BoardData
    {
        public const int row = 15, col = 15;
    }

    public class GameLogic
    {
        public BlockType[,] board;

        public void InitBoard()
        {
            board = new BlockType[BoardData.row, BoardData.col];
        }

    }
}
