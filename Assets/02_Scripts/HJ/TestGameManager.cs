using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HJ
{
    public enum PlayerType { Player_Black,  Player_White }
    public class TestGameManager : Singleton<TestGameManager>
    {
        public PlayerType playerType;   // 해당 플레이어 타입의 돌 착수        
        [SerializeField] List<TestBlock> blocks;
        public GameLogic gameLogic;

        private void Awake()
        {
            gameLogic = new GameLogic();

        }


        public void CheckGameWinner()
        {
            var winner = BoardStateChecker.CheckBoardState(gameLogic.board);
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
    }
}
