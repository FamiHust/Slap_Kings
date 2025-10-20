using UnityEngine;

[CreateAssetMenu(fileName = "GameConfigData", menuName = "Game/Game Config Data")]
public class GameConfigData : ScriptableObject
{
    [System.Serializable]
    public class CoinSystemSettings
    {
        [Header("Victory Rewards")]
        public int baseReward = 100;
        public int rewardIncrement = 50;
        public int maxReward = 1000;
        
        [Header("Defeat Rewards")]
        public int loseReward = 25;
        
        public int CalculateReward(int victoryCount)
        {
            int reward = baseReward + (victoryCount * rewardIncrement);
            return Mathf.Min(reward, maxReward);
        }
    }

    [System.Serializable]
    public class UpgradeSystemSettings
    {
        [Header("Health Upgrades")]
        public int baseHealthPrice = 100;
        public int healthPriceIncrement = 50;
        
        [Header("Power Upgrades")]
        public int basePowerPrice = 150;
        public int powerPriceIncrement = 75;
        
        public int CalculateHealthUpgradePrice(int upgradeCount)
        {
            return Mathf.Max(0, baseHealthPrice + (upgradeCount * healthPriceIncrement));
        }
        
        public int CalculatePowerUpgradePrice(int upgradeCount)
        {
            return Mathf.Max(0, basePowerPrice + (upgradeCount * powerPriceIncrement));
        }
    }

    [System.Serializable]
    public class TurnSystemSettings
    {
        [Header("Player Turn Timing")]
        public float playerWaitTime = 1.0f;
        public float playerAttackTime = 2.0f;
        public float playerGetSlappedDelay = 0.5f;
    }

    [System.Serializable]
    public class CameraSystemSettings
    {
        [Header("Camera Shake")]
        public float shakeIntensity = 1.0f;
        public float shakeDuration = 0.5f;
    }

    [System.Serializable]
    public class GameplaySettings
    {
        [Header("Game Flow")]
        public bool autoStartGame = false;
        public float gameStartDelay = 0f;
        
        [Header("UI Settings")]
        public float uiUpdateDelay = 0.1f;
        public bool showDebugInfo = false;
    }

    [Header("Game Configuration")]
    public CoinSystemSettings coinSystem = new CoinSystemSettings();
    public UpgradeSystemSettings upgradeSystem = new UpgradeSystemSettings();
    public TurnSystemSettings turnSystem = new TurnSystemSettings();
    public CameraSystemSettings cameraSystem = new CameraSystemSettings();
    public GameplaySettings gameplay = new GameplaySettings();

    // Convenience getters for backward compatibility
    public int BaseReward => coinSystem.baseReward;
    public int RewardIncrement => coinSystem.rewardIncrement;
    public int MaxReward => coinSystem.maxReward;
    public int LoseReward => coinSystem.loseReward;
    public int BaseHealthUpgradePrice => upgradeSystem.baseHealthPrice;
    public int HealthUpgradeIncrement => upgradeSystem.healthPriceIncrement;
    public int BasePowerUpgradePrice => upgradeSystem.basePowerPrice;
    public int PowerUpgradeIncrement => upgradeSystem.powerPriceIncrement;
    public float PlayerWaitTime => turnSystem.playerWaitTime;
    public float PlayerAttackTime => turnSystem.playerAttackTime;
    public float PlayerGetSlappedDelay => turnSystem.playerGetSlappedDelay;
    public float CameraShakeIntensity => cameraSystem.shakeIntensity;
    public float CameraShakeDuration => cameraSystem.shakeDuration;

    // Legacy methods for backward compatibility
    public int CalculateReward(int victoryCount) => coinSystem.CalculateReward(victoryCount);
    public int CalculateHealthUpgradePrice(int upgradeCount) => upgradeSystem.CalculateHealthUpgradePrice(upgradeCount);
    public int CalculatePowerUpgradePrice(int upgradeCount) => upgradeSystem.CalculatePowerUpgradePrice(upgradeCount);
}