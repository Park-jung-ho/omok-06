using System.Collections;
using HJ;
using UnityEngine;

public static class BoardStateChecker   // 게임 상태 체크 클래스
{
    // 보드 상태 가져오기
    // 보드 배열 탐색
    // 돌이 있을 경우 주변 탐색 (8방향).    

    public static BlockType CheckBoardState(BlockType[,] board) // 게임 상태를 반환하는 보드 순환 메서드
    {
        BlockType winPlayerBlock = BlockType.None;
        bool isResult = false;

        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                if (isResult) return winPlayerBlock;

                switch (board[i, j])
                {
                    case BlockType.None:
                        continue;
                    case BlockType.Black:
                        winPlayerBlock = CalculateWinner(BlockType.Black, (i, j), board);
                        if(winPlayerBlock != BlockType.None) isResult = true;
                        break;
                    case BlockType.White:
                        winPlayerBlock = CalculateWinner(BlockType.White, (i, j), board);
                        if (winPlayerBlock != BlockType.None) isResult = true;
                        break;
                }
            }
        }

        return winPlayerBlock;
    }

    public static bool CheckGameDraw(BlockType[,] board)
    {
        for(int i = 0;i < board.GetLength(0);i++)
        {
            for(int j=0;j < board.GetLength(1); j++)
            {
                if (board[i, j] == BlockType.None)
                {
                    return false;
                }
            }
        }

        return true;
    }



    private static BlockType CalculateWinner(BlockType blocktype, (int row, int col) blockIndex, BlockType[,] board) // 승리 체크 메서드
    {
        bool isWhitePlayer = blocktype == BlockType.White; 
        bool[,] isVisited = new bool[BoardData.row, BoardData.col]; // 방문한 블록 체크 변수          
        int count = 0; // 오목 카운트
        CheckRow(blockIndex.row); // 수직선 오목 체크
        if (count == 5) // 오목 성공시 승리한 블록의 타입 반환
        {            
            return blocktype;
        }

        // 상태 초기화
        isVisited = new bool[BoardData.row, BoardData.col];
        count = 0;
        CheckCol(blockIndex.col); // 수평선 오목 체크
        if (count == 5)
        {
            return blocktype;
        }

        isVisited = new bool[BoardData.row, BoardData.col];
        count = 0;
        CheckDia1(blockIndex.row, blockIndex.col); // 대각선 오목 체크
        if (count == 5)
        {
            return blocktype;
        }

        isVisited = new bool[BoardData.row, BoardData.col];
        count = 0;
        CheckDia2(blockIndex.row, blockIndex.col); // 대각선 오목 체크
        if (count == 5)
        {
            return blocktype;
        }


        return BlockType.None;


        void CheckRow(int rowindex) // 수직선 오목 체크 메서드
        {

            // 바둑판 영역 밖이거나 바둑이 없다면 종료
            if (rowindex < 0 || rowindex >= BoardData.row || board[rowindex, blockIndex.col] != blocktype)
            {
                return;
            }

            if (isVisited[rowindex, blockIndex.col])
            {
                return;
            }

            // 바둑이 있을경우 카운트 증가시키고 양 사이드 탐색

            count++;
            isVisited[rowindex, blockIndex.col] = true;

            if (!isWhitePlayer) CheckRow(rowindex - 1);
            CheckRow(rowindex + 1);

        }
        void CheckCol(int colIndex) // 수평선 오목 체크 메서드
        {
            if (isVisited[blockIndex.row, colIndex])
            {
                return;
            }
            // 바둑판 영역 밖이거나 바둑이 없다면 종료
            if (colIndex < 0 || colIndex >= BoardData.col || board[blockIndex.row, colIndex] != blocktype)
            {
                return;
            }

            // 바둑이 있을경우 카운트 증가시키고 양 사이드 탐색

            count++;
            isVisited[blockIndex.row, colIndex] = true;
            if (!isWhitePlayer) CheckCol(colIndex - 1);
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
            if (!isWhitePlayer) CheckDia1(rowindex - 1, colIndex - 1);
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
            if (!isWhitePlayer) CheckDia2(rowindex - 1, colIndex + 1);
            CheckDia2(rowindex + 1, colIndex - 1);
        }
    }
}

