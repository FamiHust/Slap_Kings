using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStatsData", menuName = "Game/Character Stats Data")]
public class CharacterStatsData : ScriptableObject
{
    [System.Serializable]
    public class LevelScalingSettings
    {
        [Header("Scaling Curves")]
        public AnimationCurve healthScalingCurve = AnimationCurve.Linear(0, 1, 100, 2);
        public AnimationCurve damageScalingCurve = AnimationCurve.Linear(0, 1, 100, 1.5f);
        public AnimationCurve powerScalingCurve = AnimationCurve.Linear(0, 1, 100, 1.2f);
        
        [Header("Difficulty Settings")]
        public int maxLevel = 100;
        public float difficultyMultiplier = 1.0f;
        
        public float GetHealthMultiplier(int level)
        {
            return healthScalingCurve.Evaluate(Mathf.Clamp01((float)level / maxLevel)) * difficultyMultiplier;
        }
        
        public float GetDamageMultiplier(int level)
        {
            return damageScalingCurve.Evaluate(Mathf.Clamp01((float)level / maxLevel)) * difficultyMultiplier;
        }
        
        public float GetPowerMultiplier(int level)
        {
            return powerScalingCurve.Evaluate(Mathf.Clamp01((float)level / maxLevel)) * difficultyMultiplier;
        }
    }

    [System.Serializable]
    public class SaveLoadSettings
    {
        [Header("Save System")]
        public bool enableAutoSave = true;
        public float autoSaveInterval = 30f;
        public string saveFileName = "GameSave";
        
        [Header("Data Version")]
        public int dataVersion = 1;
    }

    [Header("Character Data References")]
    public PlayerStatsData playerStats;
    public AIStatsData aiStats;
    public GameConfigData gameConfig;

    [Header("Level Scaling")]
    public LevelScalingSettings levelScaling = new LevelScalingSettings();

    [Header("Save/Load Settings")]
    public SaveLoadSettings saveLoad = new SaveLoadSettings();

    private void OnValidate()
    {
        // Ensure all required references are set
        if (playerStats == null)
            Debug.LogWarning("PlayerStatsData is not assigned in CharacterStatsData");
        
        if (aiStats == null)
            Debug.LogWarning("AIStatsData is not assigned in CharacterStatsData");
        
        if (gameConfig == null)
            Debug.LogWarning("GameConfigData is not assigned in CharacterStatsData");
    }

    // Validation methods
    public bool IsValid()
    {
        return playerStats != null && aiStats != null && gameConfig != null;
    }

    public string GetValidationErrors()
    {
        var errors = new System.Text.StringBuilder();
        
        if (playerStats == null) errors.AppendLine("PlayerStatsData is missing");
        if (aiStats == null) errors.AppendLine("AIStatsData is missing");
        if (gameConfig == null) errors.AppendLine("GameConfigData is missing");
        
        return errors.ToString();
    }
}