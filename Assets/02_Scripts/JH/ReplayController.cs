using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


public class ReplayController : MonoBehaviour
{
    [Serializable]
    public class BlockData
    {
        public int row;
        public int col;
    }
    [Serializable]
    public class PlayerData
    {
        public int rank;
        public string name;
        public bool isBlack;
    }
    [Serializable]
    public class ReplayData
    {
        public PlayerData[] playersDatas;
        public List<BlockData> replay;
    }
    [Serializable]
    public class UserReplayData
    {
        public List<ReplayData> replayData;
    }
    [SerializeField] private Transform boardRoot;
    [SerializeField] private Transform replayRoot;
    [SerializeField] private GameObject replayButtonPrefab;

    // data
    private UserReplayData _userReplayData;
    private ReplayData _currentReplayData;
    private int idx;

    // Board
    private List<BlockData> replay = new List<BlockData>();
    private Image[,] board = new Image[15, 15];

    public Sprite[] blockSprites;
    private int _prevBlockIdx;

    private void Start()
    {
        if (_userReplayData == null) _userReplayData = LoadReplay("data");
    }

    #region 데이터 저장

    private string GetFilePath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    public void AddReplay(ReplayData data)
    {
        if (_userReplayData == null) _userReplayData = LoadReplay("data");
        if (_userReplayData.replayData == null)
        {
            _userReplayData.replayData = new List<ReplayData>();
        }
        _userReplayData.replayData.Add(data);

        SaveReplay("data", _userReplayData);
    }

    public void SaveReplay(string filename, UserReplayData replayData)
    {
        string json = JsonUtility.ToJson(replayData);
        string path = GetFilePath(filename);
        File.WriteAllText(path, json);

        Debug.Log($"리플레이 저장 완료 [{path}]");
    }

    public UserReplayData LoadReplay(string filename)
    {
        string path = GetFilePath(filename);
        if (!File.Exists(path))
        {
            Debug.Log($"{path} 경로에 {filename} 없음! 새로운 데이터 생성");
            return new UserReplayData();
        }
        string json = File.ReadAllText(path);
        UserReplayData replayData = JsonUtility.FromJson<UserReplayData>(json);
        Debug.Log($"리플레이 데이터 로드: {path}");
        return replayData;
    }

    #endregion

    void InitBoard()
    {
        for (int i = 0; i < 15; i++)
        {
            for (int j = 0; j < 15; j++)
            {
                board[i, j] = boardRoot.GetChild(i*15 + j).GetComponent<Image>();
                board[i, j].enabled = false;
            }
        }

        idx = -1;
        _prevBlockIdx = 1;
    }

    public void OpenReplayPanel()
    {
        InitBoard();
        if (replayRoot.childCount != 0) return;
        int replayCount = _userReplayData.replayData.Count;
        for (int i = 0; i < replayCount; i++)
        {
            ReplayPanel replayPanel = Instantiate(replayButtonPrefab, replayRoot).GetComponent<ReplayPanel>();
            for (int j = 0; j < 2; j++)
            {
                replayPanel.userUIData[j].rank.text = $"{_userReplayData.replayData[i].playersDatas[j].rank}급";
                replayPanel.userUIData[j].nickname.text = _userReplayData.replayData[i].playersDatas[j].name;
                replayPanel.SetBlock(j,
                    _userReplayData.replayData[i].playersDatas[j].isBlack ? blockSprites[0] : blockSprites[1]);
            }
            int k = i;
            replayPanel.button.onClick.AddListener(() => OnClickReplayButton(k));
        }
    }

    public void CloseReplayPanel()
    {

    }
    public void OnClickReplayButton(int i)
    {
        _currentReplayData = _userReplayData.replayData[i];
        InitBoard();
    }
    public void Move(int moveType) // -1 = prev , 1 = next
    {
        BlockData pos;
        if (moveType == -1 && idx >= 0)
        {
            pos = _currentReplayData.replay[idx];
            _prevBlockIdx = _prevBlockIdx == 0 ? 1 : 0;
            board[pos.row, pos.col].enabled = false;
            idx--;
        }
        else if (moveType == 1 && idx < _currentReplayData.replay.Count-1)
        {
            idx++;
            pos = _currentReplayData.replay[idx];
            _prevBlockIdx = _prevBlockIdx == 0 ? 1 : 0;
            board[pos.row, pos.col].sprite = blockSprites[_prevBlockIdx];
            board[pos.row, pos.col].enabled = true;
        }
    }

    public void MoveAll(int moveIdx) // -1 = start , 1 = end
    {
        var target = moveIdx == -1 ? 0 : _currentReplayData.replay.Count;
        var t = Math.Abs(idx - target);
        for (int i = 0; i < t; i++)
        {
            Move(moveIdx);
        }
    }
}

