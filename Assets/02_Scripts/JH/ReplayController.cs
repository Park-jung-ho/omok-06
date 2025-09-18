using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class blockData
{
    public int row;
    public int col;
}
[Serializable]
public class ReplayData
{
    public List<blockData> replay;
}

public class ReplayController : MonoBehaviour
{
    private List<blockData> replay = new List<blockData>();
    private int idx;


    #region 데이터 저장

    private string GetFilePath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    public void SaveReplay(string filename, ReplayData replayData)
    {
        string json = JsonUtility.ToJson(replayData);
        string path = GetFilePath(filename);
        File.WriteAllText(path, json);

        Debug.Log($"리플레이 저장 완료 [{path}]");
    }

    public ReplayData LoadReplay(string filename)
    {
        string path = GetFilePath(filename);
        if (!File.Exists(path))
        {
            Debug.Log($"{path} 경로에 {filename} 없음!");
            return null;
        }
        string json = File.ReadAllText(path);
        ReplayData replayData = JsonUtility.FromJson<ReplayData>(json);
        return replayData;
    }

    #endregion

    public void Move(int moveType) // -1 = prev , 1 = next
    {
        idx = Math.Clamp(idx + moveType, 0, replay.Count-1);
    }
}

