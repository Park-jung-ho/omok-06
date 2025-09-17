using System.Collections;
using HJ;
using UnityEngine;

public static class GameResultChecker   // 게임 결과 체크 클래스
{
    // 보드 상태 가져오기
    // 보드 배열 탐색
    // 돌이 있을 경우 주변 탐색 (8방향).    

    /// <summary>
    /// 게임 상태를 반환하는 보드 순환 메서드
    /// </summary>
    /// <param name="board">보드판</param>
    /// <param name="lastBlockPosition">마지막 착수 위치</param>
    /// <returns></returns>
    public static Constants.PlayerType CheckBoardState(Constants.PlayerType[,] board, (int row, int col) lastBlockPosition) 
    {
        Constants.PlayerType winPlayerBlock = Constants.PlayerType.None;

        switch (board[lastBlockPosition.row, lastBlockPosition.col])
        {
            case Constants.PlayerType.PlayerA:
                winPlayerBlock = CalculateWinner(Constants.PlayerType.PlayerA, lastBlockPosition, board);
                break;
            case Constants.PlayerType.PlayerB:
                winPlayerBlock = CalculateWinner(Constants.PlayerType.PlayerB, lastBlockPosition, board);
                break;
        }

        return winPlayerBlock;
    }

    public static bool CheckGameDraw(Constants.PlayerType[,] board)
    {
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                if (board[i, j] == Constants.PlayerType.None)
                {
                    return false;
                }
            }
        }

        return true;
    }


    /// <summary>
    /// 승리 체크 메서드
    /// </summary>
    /// <param name="blocktype">해당 턴에 착수한 플레이어의 블록 타입</param>
    /// <param name="position">놓여진 돌의 위치</param>
    /// <param name="board">보드판</param>
    /// <returns></returns>
    private static Constants.PlayerType CalculateWinner(Constants.PlayerType blocktype, (int row, int col) position, Constants.PlayerType[,] board) 
    {
        bool isWhitePlayer = blocktype == Constants.PlayerType.PlayerB;
        bool[,] isVisited = new bool[BoardData.row, BoardData.col]; // 방문한 블록 체크 변수          
        int count = 0; // 오목 카운트
        CheckRow(position.row); // 수직선 오목 체크
        if (count >= 5 && isWhitePlayer) // 오목 성공시 승리한 블록의 타입 반환
        {
            return blocktype;
        }
        else if (count == 5 && !isWhitePlayer)
        {
            return blocktype;
        }

        // 상태 초기화
        isVisited = new bool[BoardData.row, BoardData.col];
        count = 0;
        CheckCol(position.col); // 수평선 오목 체크
        if (count >= 5 && isWhitePlayer) // 오목 성공시 승리한 블록의 타입 반환
        {
            return blocktype;
        }
        else if (count == 5 && !isWhitePlayer)
        {
            return blocktype;
        }

        isVisited = new bool[BoardData.row, BoardData.col];
        count = 0;
        CheckDia1(position.row, position.col); // 대각선 오목 체크
        if (count >= 5 && isWhitePlayer) // 오목 성공시 승리한 블록의 타입 반환
        {
            return blocktype;
        }
        else if (count == 5 && !isWhitePlayer)
        {
            return blocktype;
        }

        isVisited = new bool[BoardData.row, BoardData.col];
        count = 0;
        CheckDia2(position.row, position.col); // 대각선 오목 체크
        if (count >= 5 && isWhitePlayer) // 오목 성공시 승리한 블록의 타입 반환
        {
            return blocktype;
        }
        else if (count == 5 && !isWhitePlayer)
        {
            return blocktype;
        }


        return Constants.PlayerType.None;


        void CheckRow(int rowindex) // 수직선 오목 체크 메서드
        {

            // 바둑판 영역 밖이거나 바둑이 없다면 종료
            if (rowindex < 0 || rowindex >= BoardData.row || board[rowindex, position.col] != blocktype)
            {
                return;
            }

            if (isVisited[rowindex, position.col])
            {
                return;
            }

            // 바둑이 있을경우 카운트 증가시키고 양 사이드 탐색

            count++;
            isVisited[rowindex, position.col] = true;

            CheckRow(rowindex - 1);
            CheckRow(rowindex + 1);

        }
        void CheckCol(int colIndex) // 수평선 오목 체크 메서드
        {
            // 바둑판 영역 밖이거나 바둑이 없다면 종료
            if (colIndex < 0 || colIndex >= BoardData.col || board[position.row, colIndex] != blocktype)
            {
                return;
            }
            if (isVisited[position.row, colIndex])
            {
                return;
            }

            // 바둑이 있을경우 카운트 증가시키고 양 사이드 탐색

            count++;
            isVisited[position.row, colIndex] = true;
            CheckCol(colIndex - 1);
            CheckCol(colIndex + 1);
        }
        void CheckDia1(int rowindex, int colIndex) // 대각선 체크 메서드 1
        {
            // 바둑판 영역 밖이거나 바둑이 없다면 패스
            if (rowindex < 0 || rowindex >= BoardData.row ||
                colIndex < 0 || colIndex >= BoardData.col ||
                board[rowindex, colIndex] != blocktype)
            {
                return;
            }

            // 방문한 블록이면 패스
            if (isVisited[rowindex, colIndex])
            {
                return;
            }

            // 바둑이 있을경우 카운트 증가시키고 양 사이드 탐색

            count++;
            isVisited[rowindex, colIndex] = true;
            CheckDia1(rowindex - 1, colIndex - 1);
            CheckDia1(rowindex + 1, colIndex + 1);
        }
        void CheckDia2(int rowindex, int colIndex) // 대각선 체크 메서드 2
        {
            // 바둑판 영역 밖이거나 바둑이 없다면 패스
            if (rowindex < 0 || rowindex >= BoardData.row ||
                colIndex < 0 || colIndex >= BoardData.col ||
                board[rowindex, colIndex] != blocktype)
            {
                return;
            }

            // 방문한 블록이면 패스
            if (isVisited[rowindex, colIndex])
            {
                return;
            }

            // 바둑이 있을경우 카운트 증가시키고 양 사이드 탐색

            count++;
            isVisited[rowindex, colIndex] = true;
            CheckDia2(rowindex - 1, colIndex + 1);
            CheckDia2(rowindex + 1, colIndex - 1);
        }
    }
}

