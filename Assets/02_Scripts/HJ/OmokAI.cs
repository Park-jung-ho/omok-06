using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HJ;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public static class OmokAI
{
    // 보드 전체의 경우의 수 15 x 15
    // 보드에 놓여져 있는 돌 기준으로 탐색 범위 제한
    // depth(예상 턴 수)를 제한해 탐색 시행 횟수 제한
    // 평가 함수를 사용해 최적의 수 점수 반환

    private static Constants.PlayerType playerBlockType;
    private static Constants.PlayerType aiBlockType;

    private enum BlockType { Wall, None, PlayerA, PlayerB }

    // Ai가 놓을 블록 위치 값 반환
    public static (int row, int col) GetPosition(Constants.PlayerType[,] board, Constants.PlayerType aiBlockType)
    {
        if (aiBlockType == Constants.PlayerType.PlayerB)
        {
            OmokAI.aiBlockType = aiBlockType;
            playerBlockType = Constants.PlayerType.PlayerA;
        }
        else
        {
            OmokAI.aiBlockType = aiBlockType;
            playerBlockType = Constants.PlayerType.PlayerB;
        }

        int bestScore = int.MinValue;
        var movePosition = (7, 7);

        var random = new System.Random();
        var candidateMoves = FindCandidateMove(board, 1).OrderBy(x => random.Next());

        if (candidateMoves.Count() == 0) // 첫 수면 정중앙 착수
        {
            return movePosition;
        }

        // 4목은 따로 계산
        foreach (var move in candidateMoves)
        {
            (int row, int col)[] analyzeResult = AnalyzeLine(board, move.Item1, move.Item2); // 4목 라인 체크
            if (analyzeResult[0].row != -1) // ai가 착수하는 이번 턴에 양 플레이어 모두 4목이 있으면 ai승리 수를 먼저 둚
            {                               
                return analyzeResult[0];
            }
            else if (analyzeResult[1].row != -1) // player가 4목을 완성 했으면 playerResult 할당
            {
                return analyzeResult[1];
            }
        }

        foreach (var move in candidateMoves)
        {
            board[move.Item1, move.Item2] = aiBlockType;
            int score = MiniMax(board, 0, int.MinValue, int.MaxValue, false, move);
            board[move.Item1, move.Item2] = Constants.PlayerType.None;

            if (score > bestScore)
            {
                bestScore = score;
                movePosition = move;
            }
        }

        return movePosition;
    }

    // 돌 주변에 있는 빈 곳을 후보로 지정하여 후보 배열 반환
    private static HashSet<(int, int)> FindCandidateMove(Constants.PlayerType[,] board, int range)
    {
        var candidateMoves = new HashSet<(int, int)>();

        for (int i = 0; i < 15; i++)
        {
            for (int j = 0; j < 15; j++)
            {
                if (board[i, j] == Constants.PlayerType.None) continue;

                // 8방향 탐색
                for (int di = -range; di <= range; di++)
                {
                    for (int dj = -range; dj <= range; dj++)
                    {
                        if (di == 0 && dj == 0) continue;

                        int row = i + di;
                        int col = j + dj;

                        if (IsOnBoard(row, col) && board[row, col] == Constants.PlayerType.None) // 보드 범위 안에 있는 빈 블록인지 체크
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
    private static int MiniMax(Constants.PlayerType[,] board, int depth, int alpha, int beta, bool isMaximizing, (int row, int col) lastBlockIndex)
    {
        var result = GameResultChecker.CheckBoardState(board, lastBlockIndex);
        if (result == aiBlockType)
        {
            return 100000 - depth;
        }
        if (result == playerBlockType)
        {
            return -100000 + depth;
        }
        if (GameResultChecker.CheckGameDraw(board))
        {
            return 0;
        }

        if (depth >= 4)
        {
            return 4;
        }

        var candidateMoves = FindCandidateMove(board, 1);

        if (isMaximizing)
        {
            int maxScore = int.MinValue;
            foreach (var move in candidateMoves)
            {
                (int row, int col) tempMove = move;

                if (board[tempMove.row, tempMove.col] == Constants.PlayerType.None)
                {
                    board[tempMove.row, tempMove.col] = aiBlockType;
                    int score = MiniMax(board, depth + 1, alpha, beta, false, tempMove);
                    board[tempMove.row, tempMove.col] = Constants.PlayerType.None;
                    maxScore = Mathf.Max(maxScore, score);
                    alpha = Mathf.Max(alpha, maxScore);
                    if (beta <= alpha)
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

                if (board[tempMove.row, tempMove.col] == Constants.PlayerType.None)
                {
                    board[tempMove.row, tempMove.col] = playerBlockType;
                    int score = MiniMax(board, depth + 1, alpha, beta, true, tempMove);
                    board[tempMove.row, tempMove.col] = Constants.PlayerType.None;
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

    // 4목 연산 최적화
    private static (int, int)[] AnalyzeLine(Constants.PlayerType[,] board, int row, int col)
    {
        BlockType player = aiBlockType == Constants.PlayerType.PlayerA ? BlockType.PlayerB : BlockType.PlayerA;
        BlockType ai = aiBlockType == Constants.PlayerType.PlayerA ? BlockType.PlayerA : BlockType.PlayerB;

        List<(BlockType, int, int)> line = new List<(BlockType, int, int)>();
        (int row, int col)[] results = { (-1, -1), (-1, -1) };   // [0] AI결과, [1] Player결과   

        for (int di = -1; di <= 1; di++)
        {
            for (int dj = -1; dj <= 1; dj++)
            {
                if (di == 0 && dj == 0) continue;
                line.Clear();

                for (int k = 0; k < 6; k++)
                {
                    (int row, int col) movePos = (di * k + row, dj * k + col);
                    if (!IsOnBoard(movePos.row, movePos.col)) line.Add((BlockType.Wall, movePos.row, movePos.col));
                    else if (board[movePos.row, movePos.col] == Constants.PlayerType.None) line.Add((BlockType.None, movePos.row, movePos.col));
                    else if (board[movePos.row, movePos.col] == aiBlockType) line.Add((ai, movePos.row, movePos.col));
                    else if (board[movePos.row, movePos.col] == playerBlockType) line.Add((player, movePos.row, movePos.col));
                }

                var temp = CalculateLinePattern(line, ai);
                if (temp.Item1 != -1) results[0] = temp;
                var temp2 = CalculateLinePattern(line, player);
                if (temp2.Item1 != -1) results[1] = temp2;
            }
        }

        return results;
    }

    private static (int, int) CalculateLinePattern(List<(BlockType, int, int)> line, BlockType blockType)
    {

        (int row, int col) result = (-1, -1);

        if (line[0].Item1 == BlockType.None &&
             line[5].Item1 == BlockType.None)
        {
            if (line.GetRange(1, 4).All(type => type.Item1 == blockType))
            {
                result = (line[0].Item2, line[0].Item3);
                return result;
            }
        }
        else if (line[0].Item1 == BlockType.None)
        {
            if (line.GetRange(1, 4).All(type => type.Item1 == blockType))
            {
                result = (line[0].Item2, line[0].Item3);
                return result;
            }
        }
        else if (line[5].Item1 == BlockType.None)
        {
            if (line.GetRange(1, 4).All(type => type.Item1 == blockType))
            {
                result = (line[5].Item2, line[5].Item3);
                return result;
            }
        }

        return result;
    }
}
