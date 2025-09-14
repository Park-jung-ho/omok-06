using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HJ
{
    public class TestGameManager : Singleton<TestGameManager>
    {
        public BlockType playerType;   // 해당 플레이어 타입의 돌 착수        
        [SerializeField] List<TestBlock> blocks;
        public GameLogic gameLogic;
        public TestBlock lastBlock;

        private void Awake()
        {
            gameLogic = new GameLogic();

        }


        public void CheckGameWinner()
        {
            var winner = BoardStateChecker.CheckBoardState(gameLogic.board, GetBoardIndex(lastBlock.blockIndex));
            if (winner == BlockType.White)
            {
                Debug.Log("흰돌승");
            }
            else if (winner == BlockType.Black)
            {
                Debug.Log("검은돌승");
            }
            else
            {
                Debug.Log("게임 진행중");
            }
        }

        private void Start()
        {
            gameLogic.InitBoard();

            for (int i = 0; i < blocks.Count; i++)
            {
                blocks[i].blockIndex = i;
            }
        }
        public (int row, int col) GetBoardIndex(int blockIndex)
        {
            return (blockIndex / BoardData.row, blockIndex % BoardData.col);
        }

        protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            throw new System.NotImplementedException();
        }

        public void DoAiTurn()
        {
            BlockType aiBlockType = playerType == BlockType.Black ? BlockType.White : BlockType.Black;
            var aiMovePos = AILogic.GetPosition(gameLogic.board, aiBlockType);
            if(aiMovePos.row == -1)
            {
                Debug.Log("에러");
                return;
            }
            lastBlock.blockIndex = aiMovePos.row * 15 + aiMovePos.col;
            gameLogic.board[aiMovePos.row, aiMovePos.col] = aiBlockType;
            blocks[aiMovePos.row * 15 + aiMovePos.col].ChangeSprite(aiBlockType);
        }
    }
}
