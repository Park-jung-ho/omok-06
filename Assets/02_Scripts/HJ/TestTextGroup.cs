using System.Collections.Generic;
using HJ;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestTextGroup : Singleton<TestTextGroup>
{
    public List<TestText> texts;
    public Constants.PlayerType chooseType;
    private Constants.PlayerType playerType = Constants.PlayerType.PlayerB;
    public Constants.PlayerType[,] board;
    public int[,] boardScores;
    private Constants.PlayerType aiType = Constants.PlayerType.PlayerA;



    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
    }

    void Start()
    {
        for (int i = 0; i < texts.Count; i++)
        {
            texts[i].index = i;
        }
        board = new Constants.PlayerType[BoardData.row, BoardData.col];
    }

    public (int row, int col) GetBoardIndex(int blockIndex)
    {
        return (blockIndex / BoardData.row, blockIndex % BoardData.col);
    }

    public void UpdateBoardScore()
    {
        boardScores = new int[BoardData.row, BoardData.col];
        int bestScore = 0;
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
                        int count = 1;
                        int samecount = 1;
                        int othercount = 0;
                        (int row, int col) prevPos;
                        (int row, int col) lastPos;

                        prevPos = (i - di * count, j - dj * count);

                        if(IsOnBoard(prevPos.row, prevPos.col) &&
                            texts[prevPos.row * 15 + prevPos.col].blockType != board[i, j]&&
                            texts[prevPos.row * 15 + prevPos.col].blockType != Constants.PlayerType.None)
                        {
                            othercount++;
                        }

                        while (IsOnBoard(i + di * count, j + dj * count))
                        {
                            if (texts[(i + di * count) * 15 + (j + dj * count)].blockType != board[i,j] &&
                                texts[(i + di * count) * 15 + (j + dj * count)].blockType != Constants.PlayerType.None)
                            {
                                othercount++;
                                break;
                            }
                            if(texts[(i + di * count) * 15 + (j + dj * count)].blockType == board[i, j])
                            {
                                samecount++;
                            }
                            count++;
                        }

                        lastPos = (i + di * (count), j + dj * (count));

                        count = 1;
                        while ((i + di * count, j + dj * count) != lastPos)
                        {
                            boardScores[i + di * count, j + dj * count] += (int)Mathf.Pow(samecount - othercount, 2);

                            texts[(i + di * count) * 15 + (j + dj * count)].tmp.text = boardScores[i + di * count, j + dj * count].ToString();
                            bestScore = Mathf.Max(bestScore, boardScores[i + di * count, j + dj * count]);
                            count++;
                        }
                    }
                }
            }
        }
        Debug.Log(bestScore);
        UpdateBlockColor();
    }

    public void UpdateBlockColor()
    {

        for (int i = 0; i < 15; i++)
        {
            for (int j = 0; j < 15; j++)
            {
                if (boardScores[i, j] < 0)
                {
                    texts[i * 15 + j].tmp.color = Color.red;
                }
                else if (boardScores[i, j] > 0)
                {

                    texts[i * 15 + j].tmp.color = Color.green;

                }
                else
                {
                    texts[i * 15 + j].tmp.color = Color.white;
                }
            }
        }
    }

    private bool IsOnBoard(int r, int c)
    {
        return r >= 0 && r < 15 && c >= 0 && c < 15;
    }
}
