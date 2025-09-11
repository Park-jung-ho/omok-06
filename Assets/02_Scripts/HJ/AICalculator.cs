using HJ;
using Unity.Mathematics;
using UnityEngine;

public static class AICalculator
{
    // 보드 전체의 경우의 수 15 x 15
    // 

    // Ai 턴에 블록을 놓을 위치 반환
    public static (int row, int col) GetPosition(BlockType[,] board, BlockType aiBlcokType)
    {
        int bestScore = int.MinValue;
        var movePosition = (-1, -1);

        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                if (board[i, j] == BlockType.None)
                {
                    board[i, j] = aiBlcokType;
                    int score = AlphaBeta(board, 0, int.MinValue, int.MaxValue, false, aiBlcokType);
                    board[i, j] = BlockType.None;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        movePosition = (i, j);
                    }
                }
            }
        }


        return movePosition;
    }

    /// <summary>
    /// 오목 AI 알고리즘
    /// </summary>
    /// <param name="board">보드</param>
    /// <param name="depth">탐색 깊이</param>
    /// <param name="alpha"></param>
    /// <param name="beta"></param>
    /// <param name="isMaximizing">AI턴 이면 true</param>
    /// <param name="aiBlockType">AI의 블록 타입</param>
    /// <returns></returns>
    public static int AlphaBeta(BlockType[,] board, int depth, int alpha, int beta, bool isMaximizing, BlockType aiBlockType = BlockType.White)
    {
        var result = BoardStateChecker.CheckBoardState(board);
        BlockType playerBlockType = aiBlockType == BlockType.White ? BlockType.Black : BlockType.White;

        if (result == aiBlockType)
        {
            return 100 - depth;
        }
        if (result != aiBlockType || result != BlockType.None)
        {
            return -100 + depth;
        }
        if (BoardStateChecker.CheckGameDraw(board))
        {
            return 0;
        }

        if (isMaximizing)
        {
            int maxScore = int.MinValue;
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (board[i, j] == BlockType.None)
                    {
                        board[i, j] = aiBlockType;
                        int score = AlphaBeta(board, 1, alpha, beta, false, aiBlockType);
                        board[i, j] = BlockType.None;
                        maxScore = Mathf.Max(maxScore, score);
                        alpha = Mathf.Max(alpha, score);
                        if(beta <= alpha)
                        {
                            break;
                        }                        
                    }
                }
            }
            return maxScore;
        }
        else
        {
            int minScore = int.MaxValue;
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (board[i, j] == BlockType.None)
                    {
                        board[i, j] = playerBlockType;
                        int score = AlphaBeta(board, 1, alpha, beta, false, aiBlockType);
                        board[i, j] = BlockType.None;
                        minScore = Mathf.Min(minScore, score);
                        beta = Mathf.Min(beta, score);
                        if (beta <= alpha)
                        {
                            break;
                        }                        
                    }
                }
            }
            return minScore;
        }
    }
}
