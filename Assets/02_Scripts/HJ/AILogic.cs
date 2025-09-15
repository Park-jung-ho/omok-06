using System.Collections;
using System.Collections.Generic;
using HJ;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public static class AILogic
{
    // 보드 전체의 경우의 수 15 x 15
    // 보드에 놓여져 있는 돌 기준으로 탐색 범위 제한
    // depth(예상 턴 수)를 제한해 탐색 시행 횟수 제한
    // 평가 함수를 사용해 최적의 수 점수 반환

    private static BlockType playerBlockType;
    private static BlockType aiBlockType;

    // Ai가 놓을 블록 위치 값 반환
    public static (int row, int col) GetPosition(BlockType[,] board, BlockType aiBlockType)
    {
        if (aiBlockType == BlockType.White)
        {
            AILogic.aiBlockType = aiBlockType;
            playerBlockType = BlockType.Black;
        }
        else
        {
            AILogic.aiBlockType = aiBlockType;
            playerBlockType = BlockType.White;
        }
        
        int bestScore = int.MinValue;
        var movePosition = (7, 7);    
        
        var candidateMoves = FindCandidateMove(board, 1);        

        foreach (var move in candidateMoves)
        {
            (int row, int col) tempMove = move;

            board[tempMove.row, tempMove.col] = aiBlockType;
            int score = MiniMax(board, 0, int.MinValue, int.MaxValue, false, tempMove);
            board[tempMove.row, tempMove.col] = BlockType.None;

            if (score > bestScore)
            {
                bestScore = score;
                movePosition = tempMove;
            }
        }

        return movePosition;
    }

    // 돌 주변(반경 2블록) 에 있는 빈 곳 탐색 후 반환
    private static HashSet<(int, int)> FindCandidateMove(BlockType[,] board, int range)
    {
        var candidateMoves = new HashSet<(int, int)>();

        for (int i = 0; i < 15; i++)
        {
            for (int j = 0; j < 15; j++)
            {
                if (board[i, j] == BlockType.None) continue;

                // 8방향 탐색
                for(int di = -range; di <= range; di++)
                {
                    for(int dj = -range; dj <= range; dj++)
                    {
                        if (di == 0 && dj == 0) continue;

                        int row = i + di;
                        int col = j + dj;

                        if(IsOnBoard(row, col) && board[row, col] == BlockType.None)
                        {
                            candidateMoves.Add((row, col));
                        }
                    }
                }
            }
        }

        return candidateMoves;
    }

    private static bool IsOnBoard(int r, int c)
    {
        return r >= 0 && r < 15 && c >= 0 && c < 15;
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
    private static int MiniMax(BlockType[,] board, int depth, int alpha, int beta, bool isMaximizing, (int row, int col) lastBlockIndex)
    {
        var result = BoardStateChecker.CheckBoardState(board, lastBlockIndex);
        if (result == aiBlockType)
        {
            return 100000 - depth;
        }
        if (result == playerBlockType)
        {
            return -100000 + depth;
        }
        if (BoardStateChecker.CheckGameDraw(board))
        {
            return 0;
        }

        if (depth >= 3)
        {
            return 5;
        }

        var candidateMoves = FindCandidateMove(board, 1);

        if (isMaximizing)
        {
            int maxScore = int.MinValue;
            foreach (var move in candidateMoves)
            {
                (int row, int col) tempMove = move;

                if (board[tempMove.row, tempMove.col] == BlockType.None)
                {                                        
                    board[tempMove.row, tempMove.col] = aiBlockType;
                    int score = MiniMax(board, depth + 1, alpha, beta, false, tempMove);                    
                    board[tempMove.row, tempMove.col] = BlockType.None;
                    maxScore = Mathf.Max(maxScore, score);
                    alpha = Mathf.Max(alpha, maxScore);
                    if(beta <= alpha)
                    {
                        break;
                    }
                }
            }
            
            return maxScore;
        }
        else
        {
            int minScore = int.MaxValue;
            foreach (var move in candidateMoves)
            {
                (int row, int col) tempMove = move;

                if (board[tempMove.row, tempMove.col] == BlockType.None)
                {                    
                    board[tempMove.row, tempMove.col] = playerBlockType;
                    int score = MiniMax(board, depth + 1, alpha, beta,true, tempMove);
                    board[tempMove.row, tempMove.col] = BlockType.None;
                    minScore = Mathf.Min(minScore, score);
                    beta = Mathf.Min(beta, minScore);
                    if (beta <= alpha)
                    {
                        break;
                    }
                }
            }
            return minScore;
        }
    }


    // 돌의 n목, open-close 여부에 따라 평가 함수 작성
    // 공격 점수
    // 열린 4목 100점 닫힌 4목 100점
    // 열린 3목 10점, 닫힌 3목 = 6점
    // 열린 2목 5점, 닫힌 2목 = 2점
    // 열린 1목 3점, 닫힌 1목 = 1점
    // 수비 점수
    // 열린 4목 종료, 닫힌 4목 99점
    // 열린 3목 9점, 닫힌 3목 5점
    // ...
    // 공격 점수와 수비 점수를 계산해 최적의 수 도출 // 공격 점수 x 수비 점수 = 최종 점수
    //private static int Heuristic(BlockType[,] board)
    //{
    //    int[,] attackPoints = new int[15,15];
    //    int[,] defensePoints = new int[15,15];
    //}
}
