using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HJ
{
    public class TestGameManager : Singleton<TestGameManager>
    {
        public Constants.PlayerType playerType;   // 해당 플레이어 타입의 돌 착수
        [SerializeField] List<TestBlock> blocks;
        public GameLogic gameLogic;
        [ReadOnly] public TestBlock lastBlock;
        [SerializeField] private AIDifficultyType difficultyType;

        protected override void Awake()
        {
            base.Awake();
            gameLogic = new GameLogic();
        }

        public void CheckGameWinner()
        {
            var winner = GameResultChecker.CheckBoardState(gameLogic.board, GetBoardIndex(lastBlock.blockIndex));
            if (winner == Constants.PlayerType.PlayerB)
            {
                Debug.Log("흰돌승");
            }
            else if (winner == Constants.PlayerType.PlayerA)
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

        }

        public async void DoAiTurn()
        {
            Constants.PlayerType aiBlockType = playerType == Constants.PlayerType.PlayerA ? Constants.PlayerType.PlayerB : Constants.PlayerType.PlayerA;
            (int row, int col) aiMovePos = (-1, -1);

            if (aiBlockType != Constants.PlayerType.None)
            {
                await Task.Run(() =>
                {
                    aiMovePos = GomokuAI.GetPosition(gameLogic.board, aiBlockType, difficultyType);
                });
            }

            if (lastBlock == null)
            {
                lastBlock = blocks[aiMovePos.row * 15 + aiMovePos.col];
            }

            lastBlock.blockIndex = aiMovePos.row * 15 + aiMovePos.col;
            gameLogic.board[aiMovePos.row, aiMovePos.col] = aiBlockType;
            blocks[aiMovePos.row * 15 + aiMovePos.col].ChangeSprite(aiBlockType);
        }
    }
}
