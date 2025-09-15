using System;
using UnityEngine;

public class GameModeCheck : MonoBehaviour
{
    void Start()
    {
        Debug.Log($"현재 게임모드" + GameManager._gameType);
            
    }

}
