using HJ;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public enum AIDifficultyType { Easy, Normal, Hard }
public enum AIRuleType { Normal, Renju }
public class AIManager : MonoBehaviour
{
    [SerializeField, ReadOnly] AIDifficultyType difficultyType;
    [SerializeField, ReadOnly] AIRuleType ruleType;
}

