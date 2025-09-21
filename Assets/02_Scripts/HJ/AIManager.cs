using HJ;
using Unity.VisualScripting;
using UnityEngine;

public enum AIDifficultyType { Easy, Normal, Hard }
public enum AIRuleType { Normal, Renju }
public class AIManager : MonoBehaviour
{
    [SerializeField, ReadOnly] private AIDifficultyType difficultyType;
    public AIDifficultyType DifficultyType
    {
        get => difficultyType;
        set
        {

            difficultyType = value;
        }
    }
    [SerializeField, ReadOnly] private AIRuleType ruleType;





    public void SetAIMode(AIDifficultyType difficultyType, AIRuleType ruleType)
    {
        this.difficultyType = difficultyType;
        this.ruleType = ruleType;
    }
    public AIRuleType GetRuleType()
    {
        return ruleType;
    }
    public AIDifficultyType GetDifficultyType()
    {
        return difficultyType;
    }
}

