using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HJ;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public static class GomokuAI
{
    // 보드 전체의 경우의 수 15 x 15
    // 보드에 놓여져 있는 돌 기준으로 탐색 범위 제한
    // depth(예상 턴 수)를 제한해 탐색 시행 횟수 제한
    // 평가 함수를 사용해 최적의 수 점수 반환

    private static Constants.PlayerType playerBlockType;
    private static Constants.PlayerType aiBlockType;
    private static AIDifficultyType difficultyType;

    private enum BlockType { Wall, None, PlayerA, PlayerB }

    /// <summary>
    /// Ai가 놓을 블록 위치 값 반환 
    /// </summary>
    /// <param name="board">보드판</param>
    /// <param name="aiBlockType">AI 돌 타입</param>
    /// <param name="difficultyType">AI 난이도</param>
    /// <returns></returns>
    public static (int row, int col) GetPosition(Constants.PlayerType[,] board, Constants.PlayerType aiBlockType, AIDifficultyType difficultyType)
    {
        GomokuAI.difficultyType = difficultyType;

        if (aiBlockType == Constants.PlayerType.PlayerB)
        {
            GomokuAI.aiBlockType = aiBlockType;
            playerBlockType = Constants.PlayerType.PlayerA;
        }
        else
        {
            GomokuAI.aiBlockType = aiBlockType;
            playerBlockType = Constants.PlayerType.PlayerB;
        }

        int bestScore = int.MinValue;
        var movePosition = (7, 7);
        List<(int, int)> candidateMoves = new List<(int, int)>();

        switch (difficultyType) // 난이도별 후보수 선별 방식 조절
        {
            case AIDifficultyType.Easy:
                candidateMoves = FindCandidateMoveByScore(board, 2, 1);
                break;
            case AIDifficultyType.Normal:
                candidateMoves = FindCandidateMoveByScore(board, 4, 1);
                break;
            case AIDifficultyType.Hard:
                candidateMoves = FindCandidateMoveByScore(board, 6, 1);
                break;
        }

        if (candidateMoves.Count() == 0) // 첫 수면 정중앙 착수
        {
            return movePosition;
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

    /// <summary>
    /// 흑돌의 금수 좌표 리스트 반환
    /// </summary>
    /// <param name="board">보드판</param>
    /// <returns></returns>
    public static List<(int row, int col)> GetBannedPosList(Constants.PlayerType[,] board)
    {
        List<(int row, int col)> result = new List<(int row, int col)>();

        int openThreeCount = 0;
        int openFourCount = 0;
        var temp = GomokuAI.FindCandidateMove(board, 1);

        foreach (var move in temp)
        {
            openFourCount = 0;
            openThreeCount = 0;
            board[move.Item1, move.Item2] = Constants.PlayerType.PlayerA;

            if (GameResultChecker.CheckBoardState(board, (move.Item1, move.Item2)) == Constants.PlayerType.PlayerA)
            {
                board[move.Item1, move.Item2] = Constants.PlayerType.None;
                continue;
            }

            var lines = AnalyzeLine(board, move);

            foreach (var line in lines)
            {
                if (CalculateOpenFour(line.line, line.pos)) openFourCount++;
                if (CalculateOpenThree(line.line, line.pos)) openThreeCount++;
            }

            if (openFourCount >= 2 || openThreeCount >= 2) result.Add(move);            

            board[move.Item1, move.Item2] = Constants.PlayerType.None;
        }

        return result;
    }

    /// <summary>
    /// 해당 좌표가 흑돌의 금수 자리인지 체크
    /// </summary>
    /// <param name="board">보드판</param>
    /// <param name="move">착수할 위치값</param>
    /// <returns></returns>
    public static bool IsBannedPos(Constants.PlayerType[,] board, (int row, int col) move)
    {
        board[move.row, move.col] = Constants.PlayerType.PlayerA;

        if (GameResultChecker.CheckBoardState(board, move) == Constants.PlayerType.PlayerA)
        {
            board[move.row, move.col] = Constants.PlayerType.None;
            return false;
        }

        int openThreeCount = 0;
        int openFourCount = 0;
        var lines = AnalyzeLine(board, move);

        foreach (var line in lines)
        {
            if (CalculateOpenFour(line.line, line.pos)) openFourCount++;
            if (CalculateOpenThree(line.line, line.pos)) openThreeCount++;
        }

        board[move.row, move.col] = Constants.PlayerType.None;

        return openThreeCount >= 2 || openFourCount >= 2;
    }

    /// <summary>
    /// 착수 할 위치를 기준으로 모든 방향을 순회해서 라인 반환
    /// </summary>
    /// <param name="board">보드판</param>
    /// <param name="blockPos">착수 위치</param>
    /// <returns></returns>
    private static List<(List<Constants.PlayerType> line, int pos)> AnalyzeLine(Constants.PlayerType[,] board, (int row, int col) blockPos)
    {
        List<(List<Constants.PlayerType>, int pos)> resultLines = new List<(List<Constants.PlayerType>, int pos)>();

        List<Constants.PlayerType> line = new List<Constants.PlayerType>();
        int pos = 0;
        for (int i = 0; i < 15; i++)
        {
            if (blockPos.col == i)
            {
                pos = i;
            }
            line.Add(board[blockPos.row, i]);
        }
        resultLines.Add((line, pos));

        line = new List<Constants.PlayerType>();
        for (int i = 0; i < 15; i++)
        {
            if (blockPos.row == i)
            {
                pos = i;
            }
            line.Add(board[i, blockPos.col]);
        }
        resultLines.Add((line, pos));

        (int row, int col) startPos = (blockPos.row, blockPos.col);
        int count = 0;
        line = new List<Constants.PlayerType>();
        while (startPos.row > 0 && startPos.col > 0)
        {
            startPos = (startPos.row - 1, startPos.col - 1);
        }
        while (startPos.row + count < 15 && startPos.col + count < 15)
        {
            if (blockPos.row == startPos.row + count &&
                blockPos.col == startPos.col + count)
            {
                pos = count;
            }
            line.Add(board[startPos.row + count, startPos.col + count]);
            count++;
        }
        resultLines.Add((line, pos));

        startPos = (blockPos.row, blockPos.col);
        count = 0;
        line = new List<Constants.PlayerType>();
        while (startPos.row < 14 && startPos.col > 0)
        {
            startPos = (startPos.row + 1, startPos.col - 1);
        }
        while (startPos.row - count >= 0 && startPos.col + count < 15)
        {
            if (blockPos.row == startPos.row - count &&
                blockPos.col == startPos.col + count)
            {
                pos = count;
            }
            line.Add(board[startPos.row - count, startPos.col + count]);
            count++;
        }
        resultLines.Add((line, pos));

        return resultLines;
    }

    private static bool CalculateOpenFour(List<Constants.PlayerType> line, int pos)
    {
        return FarLeftPattern() || LeftPattern() || RightPattern() || FarRightPattern();

        bool FarLeftPattern()
        {
            if (pos > 0 && pos + 4 < line.Count &&
                line[pos + 1] == Constants.PlayerType.PlayerA &&
                line[pos + 2] == Constants.PlayerType.PlayerA &&
                line[pos + 3] == Constants.PlayerType.PlayerA)
            {
                if (line[pos - 1] == Constants.PlayerType.None &&
                    line[pos + 4] == Constants.PlayerType.None)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        bool LeftPattern()
        {
            if (pos > 1 && pos + 3 < line.Count &&
                line[pos - 1] == Constants.PlayerType.PlayerA &&
                line[pos + 1] == Constants.PlayerType.PlayerA &&
                line[pos + 2] == Constants.PlayerType.PlayerA)
            {
                if (line[pos - 2] == Constants.PlayerType.None &&
                    line[pos + 3] == Constants.PlayerType.None)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        bool RightPattern()
        {
            if (pos > 2 && pos + 2 < line.Count &&
                line[pos - 2] == Constants.PlayerType.PlayerA &&
                line[pos - 1] == Constants.PlayerType.PlayerA &&
                line[pos + 1] == Constants.PlayerType.PlayerA)
            {
                if (line[pos - 3] == Constants.PlayerType.None &&
                    line[pos + 2] == Constants.PlayerType.None)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        bool FarRightPattern()
        {
            if (pos > 3 && pos + 1 < line.Count &&                
                line[pos - 3] == Constants.PlayerType.PlayerA &&
                line[pos - 2] == Constants.PlayerType.PlayerA &&
                line[pos - 1] == Constants.PlayerType.PlayerA)
            {
                if (line[pos - 4] == Constants.PlayerType.None &&
                    line[pos + 1] == Constants.PlayerType.None)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// 해당 라인이 열린 3인지 bool 값 반환
    /// </summary>
    /// <param name="line">라인</param>
    /// <param name="pos">해당 라인에서 착수 위치에 해당하는 인덱스</param>
    /// <returns></returns>
    private static bool CalculateOpenThree(List<Constants.PlayerType> line, int pos)
    {
        return LinkedPattern() || RightBlankPattern() || LeftBlankPattern();

        bool LinkedPattern()
        {
            if (pos > 1 && pos < line.Count - 2 &&
                line[pos - 1] == Constants.PlayerType.PlayerA &&
                line[pos + 1] == Constants.PlayerType.PlayerA)
            {
                if (line[pos + 2] == Constants.PlayerType.None &&
                    line[pos - 2] == Constants.PlayerType.None)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (pos > 0 && pos < line.Count - 3 &&
                     line[pos + 1] == Constants.PlayerType.PlayerA &&
                     line[pos + 2] == Constants.PlayerType.PlayerA)
            {
                if (line[pos - 1] == Constants.PlayerType.None &&
                    line[pos + 3] == Constants.PlayerType.None)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (pos > 2 && pos < line.Count - 1 &&
                    line[pos - 1] == Constants.PlayerType.PlayerA &&
                    line[pos - 2] == Constants.PlayerType.PlayerA)
            {
                if (line[pos + 1] == Constants.PlayerType.None &&
                    line[pos - 3] == Constants.PlayerType.None)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }
        bool LeftBlankPattern()
        {
            if (pos > 0 && pos < line.Count - 4 &&
               line[pos + 1] == Constants.PlayerType.None &&
               line[pos + 2] == Constants.PlayerType.PlayerA &&
               line[pos + 3] == Constants.PlayerType.PlayerA)
            {
                if (line[pos - 1] == Constants.PlayerType.None &&
                    line[pos + 4] == Constants.PlayerType.None)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (pos > 3 && pos < line.Count - 2 &&
                     line[pos - 1] == Constants.PlayerType.None &&
                     line[pos - 2] == Constants.PlayerType.PlayerA &&
                     line[pos + 1] == Constants.PlayerType.PlayerA)
            {
                if (line[pos - 3] == Constants.PlayerType.None &&
                    line[pos + 2] == Constants.PlayerType.None)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (pos > 4 && pos < line.Count - 1 &&
                     line[pos - 1] == Constants.PlayerType.PlayerA &&
                     line[pos - 2] == Constants.PlayerType.None &&
                     line[pos - 3] == Constants.PlayerType.PlayerA)
            {
                if (line[pos - 4] == Constants.PlayerType.None &&
                    line[pos + 1] == Constants.PlayerType.None)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        bool RightBlankPattern()
        {
            if (pos > 0 && pos < line.Count - 4 &&
               line[pos + 1] == Constants.PlayerType.PlayerA &&
               line[pos + 2] == Constants.PlayerType.None &&
               line[pos + 3] == Constants.PlayerType.PlayerA)
            {
                if (line[pos - 1] == Constants.PlayerType.None &&
                    line[pos + 4] == Constants.PlayerType.None)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (pos > 1 && pos < line.Count - 3 &&
                     line[pos - 1] == Constants.PlayerType.PlayerA &&
                     line[pos + 1] == Constants.PlayerType.None &&
                     line[pos + 2] == Constants.PlayerType.PlayerA)
            {
                if (line[pos - 2] == Constants.PlayerType.None &&
                    line[pos + 3] == Constants.PlayerType.None)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (pos > 4 && pos < line.Count - 1 &&
                     line[pos - 1] == Constants.PlayerType.None &&
                     line[pos - 2] == Constants.PlayerType.PlayerA &&
                     line[pos - 3] == Constants.PlayerType.PlayerA)
            {
                if (line[pos - 4] == Constants.PlayerType.None &&
                    line[pos + 1] == Constants.PlayerType.None)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }
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
    private static int MiniMax(Constants.PlayerType[,] board, int depth, int alpha, int beta, bool isMaximizing, (int row, int col) lastBlockPos)
    {
        var result = GameResultChecker.CheckBoardState(board, lastBlockPos);

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

        if (depth >= 7)
        {
            return Heuristic(board);
        }

        List<(int, int)> candidateMoves = new List<(int, int)>();

        switch (difficultyType) // 난이도별 후보수 선별 방식 조절
        {
            case AIDifficultyType.Easy:
                candidateMoves = FindCandidateMoveByScore(board, 3, 1);
                break;
            case AIDifficultyType.Normal:
                candidateMoves = FindCandidateMoveByScore(board, 5, 1);
                break;
            case AIDifficultyType.Hard:
                candidateMoves = FindCandidateMoveByScore(board, 10, 1);
                break;
        }

        if (isMaximizing)
        {
            int maxScore = int.MinValue;
            foreach (var move in candidateMoves)
            {
                (int row, int col) tempMove = move;

                if (board[tempMove.row, tempMove.col] == Constants.PlayerType.None)
                {
                    if (aiBlockType == Constants.PlayerType.PlayerA && IsBannedPos(board, move)) continue;

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
                    if (playerBlockType == Constants.PlayerType.PlayerA && IsBannedPos(board, move)) continue;

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
    // 수비 점수
    // 열린 4목 종료, 닫힌 4목 99점
    // 열린 3목 9점, 닫힌 3목 5점
    // ...
    private static int Heuristic(Constants.PlayerType[,] board)
    {
        //  놓여져 있는 블록을 발견하면 전 블록부터 차례대로 카운팅
        //  같은 블록을 검색하다 다른 블록이 있으면 카운팅 종료
        int resultScore = 0;

        for (int i = 0; i < 15; i++)
        {
            for (int j = 0; j < 15; j++)
            {
                if (board[i, j] == Constants.PlayerType.None) continue;

                for (int di = -1; di <= 1; di++)
                {
                    for (int dj = 0; dj <= 1; dj++)
                    {
                        if (di == 0 && dj == 0) continue;


                        int openCount = 0;
                        int sameCount = 0;
                        (int row, int col) firstPos = (i, j);
                        (int row, int col) currentPos = (i, j);
                        (int row, int col) prevPos = (i - dj, j - dj);



                        if (IsOnBoard(prevPos.row, prevPos.col) &&
                            board[prevPos.row, prevPos.col] == Constants.PlayerType.None)
                        {
                            openCount++;
                        }

                        while (IsOnBoard(currentPos.row, currentPos.col) &&
                            board[currentPos.row, currentPos.col] == board[firstPos.row, firstPos.col])
                        {
                            sameCount++;
                            currentPos = (currentPos.row + di, currentPos.col + dj);

                            if (sameCount >= 5)
                            {
                                if (board[firstPos.row, firstPos.col] == aiBlockType)
                                {
                                    return 100000;
                                }
                                else if (board[firstPos.row, firstPos.col] == playerBlockType)
                                {
                                    return -100000;
                                }
                            }
                        }

                        if (IsOnBoard(currentPos.row, currentPos.col) &&
                            board[currentPos.row, currentPos.col] == Constants.PlayerType.None)
                        {
                            openCount++;
                        }

                        if (openCount > 0)
                        {
                            if (board[firstPos.row, firstPos.col] == aiBlockType)
                            {
                                resultScore += CalculateScore(sameCount, openCount);
                            }
                            else if (board[firstPos.row, firstPos.col] == playerBlockType)
                            {
                                resultScore -= CalculateScore(sameCount, openCount);
                            }
                        }
                    }
                }
            }
        }
        return resultScore;
    }

    // 돌 주변에 있는 빈 곳을 후보로 지정하여 후보수 판별
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

    /// <summary>
    /// 놓여진 돌들을 기준으로 보드를 점수화하여 후보수 선별
    /// </summary>
    /// <param name="board">보드판</param>
    /// <param name="candidateCount">찾을 후보수 개수 제한</param>
    /// <param name="range">선별 할 범위</param>
    /// <returns></returns>
    private static List<(int, int)> FindCandidateMoveByScore(Constants.PlayerType[,] board, int candidateCount, int range)
    {
        List<(int, int, int)> optimizedBoard = new List<(int, int, int)>();

        for (int i = 0; i < 15; i++)
        {
            for (int j = 0; j < 15; j++)
            {
                if (board[i, j] == Constants.PlayerType.None) continue;

                for (int di = -1; di <= 1; di++)
                {
                    for (int dj = -1; dj <= 1; dj++)
                    {
                        if (di == 0 && dj == 0) continue;
                        int count = 1;  // 탐색 범위 카운트
                        int samecount = 1;  // 라인에 존재하는 같은 타입의 돌 개수
                        int othercount = 0; // 라인에 존재하는 다른 타입의 돌 개수
                        (int row, int col) prevPos;
                        (int row, int col) lastPos;

                        prevPos = (i - di, j - dj);

                        if (IsOnBoard(prevPos.row, prevPos.col))    // 탐색 시작점 이전 위치 확인
                        {
                            if (board[prevPos.row, prevPos.col] != board[i, j] &&
                            board[prevPos.row, prevPos.col] != Constants.PlayerType.None)
                            {
                                othercount++;
                            }
                        }

                        while (IsOnBoard(i + di * count, j + dj * count))   // 시작점 부터 탐색
                        {
                            if (board[(i + di * count), (j + dj * count)] != board[i, j] &&
                                board[(i + di * count), (j + dj * count)] != Constants.PlayerType.None)
                            {
                                othercount++;
                                break;
                            }
                            if (board[(i + di * count), (j + dj * count)] == board[i, j])
                            {
                                samecount++;
                            }
                            count++;
                        }

                        lastPos = (i + di * (count), j + dj * (count)); // 탐색이 끝나는 위치

                        count = 1;
                        while ((i + di * count, j + dj * count) != lastPos &&
                            count - samecount <= range - 1)   // 마지막 수, 요청한 범위까지 탐색
                        {
                            if (board[i + di * count, j + dj * count] == Constants.PlayerType.None)
                            {
                                optimizedBoard.Add((i + di * count, j + dj * count,
                                                   (int)Mathf.Pow(samecount - othercount, 2)));    // open-close n목 점수 계산
                            }
                            count++;
                        }
                    }
                }
            }
        }

        optimizedBoard = optimizedBoard.OrderByDescending(x => x.Item3).ToList();   // 점수를 기준으로 내림차순으로 후보수 정렬
        List<(int, int)> result = new List<(int, int)>();

        foreach (var t in optimizedBoard)
        {
            result.Add((t.Item1, t.Item2));
        }

        return result.GetRange(0, Mathf.Min(result.Count, candidateCount));
    }

    // 해당 좌표가 보드안에 있는지 체크
    private static bool IsOnBoard(int r, int c)
    {
        return r >= 0 && r < 15 && c >= 0 && c < 15;
    }

    //  open-closed n목 계산
    private static int CalculateScore(int sameCount, int openCount)
    {
        int resultScore = 0;

        switch (sameCount)
        {
            case 2:
                resultScore = openCount == 2 ? 10 : 9;
                break;
            case 3:
                resultScore = openCount == 2 ? 100 : 81;
                break;
            case 4:
                resultScore = openCount == 2 ? 100000 : 10000;
                break;
            default:
                return 0;
        }

        return resultScore;
    }

    // 4목 연산 최적화
    //private static (int, int)[] AnalyzeLine(Constants.PlayerType[,] board, int row, int col)
    //{
    //    BlockType player = aiBlockType == Constants.PlayerType.PlayerA ? BlockType.PlayerB : BlockType.PlayerA;
    //    BlockType ai = aiBlockType == Constants.PlayerType.PlayerA ? BlockType.PlayerA : BlockType.PlayerB;

    //    List<(BlockType, int, int)> line = new List<(BlockType, int, int)>();
    //    (int row, int col)[] results = { (-1, -1), (-1, -1) };   // [0] AI결과, [1] Player결과   

    //    for (int di = -1; di <= 1; di++)    // 모든 방향으로 라인 검색
    //    {
    //        for (int dj = -1; dj <= 1; dj++)
    //        {
    //            if (di == 0 && dj == 0) continue;
    //            line.Clear();

    //            for (int k = 0; k <= 5; k++)
    //            {
    //                (int row, int col) movePos = (di * k + row, dj * k + col);
    //                if (!IsOnBoard(movePos.row, movePos.col)) line.Add((BlockType.Wall, movePos.row, movePos.col));
    //                else if (board[movePos.row, movePos.col] == Constants.PlayerType.None) line.Add((BlockType.None, movePos.row, movePos.col));
    //                else if (board[movePos.row, movePos.col] == aiBlockType) line.Add((ai, movePos.row, movePos.col));
    //                else if (board[movePos.row, movePos.col] == playerBlockType) line.Add((player, movePos.row, movePos.col));
    //            }

    //            var temp = CalculateLinePattern(line, ai);
    //            if (temp.Item1 != -1) results[0] = temp;
    //            var temp2 = CalculateLinePattern(line, player);
    //            if (temp2.Item1 != -1) results[1] = temp2;
    //        }
    //    }

    //    return results;
    //}

    // 라인에 있는 블록을 확인하고 4목이 이루어져 있으면 최적의 수 반환
    //private static (int, int) CalculateLinePattern(List<(BlockType, int, int)> line, BlockType blockType)
    //{

    //    (int row, int col) result = (-1, -1);

    //    //if(line.Where(type=>type.Item1 == BlockType.PlayerA).Count() >= 5)
    //    //{
    //    //    Debug.Log("6목 제외");
    //    //    return result;
    //    //}

    //    //var openInsideBlock = line.GetRange(1, 5).Where(type => type.Item1 == BlockType.None).ToList();
    //    //bool hasOtherBlockType = line.GetRange(1, 5).Any(type => type.Item1 != BlockType.None && type.Item1 != blockType);

    //    ////  가운데 빈 공간이 있는 2-2, 1-3목 검색
    //    //if (openInsideBlock.Count == 1 && !hasOtherBlockType)
    //    //{
    //    //    Debug.Log("실행");
    //    //    result = (openInsideBlock[0].Item2, openInsideBlock[0].Item3);
    //    //}

    //    //  연결되어 있는 4목 검색
    //    if (line[0].Item1 == BlockType.None &&
    //         line[5].Item1 == BlockType.None)
    //    {
    //        if (line.GetRange(1, 4).All(type => type.Item1 == blockType))
    //        {
    //            result = (line[0].Item2, line[0].Item3);
    //            return result;
    //        }
    //    }
    //    else if (line[0].Item1 == BlockType.None)
    //    {
    //        if (line.GetRange(1, 4).All(type => type.Item1 == blockType))
    //        {
    //            result = (line[0].Item2, line[0].Item3);
    //            return result;
    //        }
    //    }
    //    else if (line[5].Item1 == BlockType.None)
    //    {
    //        if (line.GetRange(1, 4).All(type => type.Item1 == blockType))
    //        {
    //            result = (line[5].Item2, line[5].Item3);
    //            return result;
    //        }
    //    }

    //    return result;
    //}
}
