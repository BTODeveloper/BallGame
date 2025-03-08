using UnityEngine;

[CreateAssetMenu(fileName = "GameplaySettings", menuName = "Match3/GameplaySettings")]
public class GameplaySettings : ScriptableObject
{
    public enum DifficultyType { Easy, Normal, Hard }
    
    [field: Header("Difficulty")]
    [field: SerializeField] public DifficultyType Difficulty { get; private set; } = DifficultyType.Normal;
    
    [field: Header("Game Rules")]
    [field: SerializeField] public int TapsLimit { get; private set; } = 20;
    [field: SerializeField] public float GameDuration { get; private set; } = 30f;
    [field: SerializeField] public int TargetScore { get; private set; } = 400;
}