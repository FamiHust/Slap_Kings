using UnityEngine;
using System;

namespace Duc
{
    public class DataManager : SingletonManager<DataManager>
    {
        [Header("Master Data")]
        [SerializeField] private CharacterStatsData m_MasterData;

        private PlayerStatsData m_PlayerStats;
        private AIStatsData m_AIStats;
        private GameConfigData m_GameConfig;

        public static event Action OnDataLoaded;
        public static event Action OnDataChanged;

        public PlayerStatsData PlayerStats => m_PlayerStats;
        public AIStatsData AIStats => m_AIStats;
        public GameConfigData GameConfig => m_GameConfig;
        public CharacterStatsData MasterData => m_MasterData;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            
            LoadData();
        }

        private void LoadData()
        {
            if (m_MasterData != null && m_MasterData.IsValid())
            {
                m_PlayerStats = m_MasterData.playerStats;
                m_AIStats = m_MasterData.aiStats;
                m_GameConfig = m_MasterData.gameConfig;
            }
            else
            {
                LoadIndividualData();
            }
            
            OnDataLoaded?.Invoke();
        }

        private void LoadIndividualData()
        {
            if (m_PlayerStats == null)
            {
                m_PlayerStats = Resources.Load<PlayerStatsData>("PlayerStatsData");
            }
            
            if (m_AIStats == null)
            {
                m_AIStats = Resources.Load<AIStatsData>("AIStatsData");
            }
            
            if (m_GameConfig == null)
            {
                m_GameConfig = Resources.Load<GameConfigData>("GameConfigData");
            }
        }

        public void RefreshData()
        {
            LoadData();
            OnDataChanged?.Invoke();
        }

        public int GetPlayerMaxHealth(int upgradeCount)
        {
            if (m_PlayerStats != null) 
                return m_PlayerStats.GetMaxHealthWithUpgrades(upgradeCount);
            return 100 + (upgradeCount * 10); 
        }

        public int GetPlayerPowerBonusDamage(int upgradeCount)
        {
            if (m_PlayerStats != null) 
                return m_PlayerStats.power.GetPowerBonusDamage(upgradeCount);
            return upgradeCount; 
        }

        public int GetAIRandomDamage()
        {
            if (m_AIStats != null) 
                return m_AIStats.damage.GetRandomDamage();
            return UnityEngine.Random.Range(10, 31); 
        }

        public int GetAIRandomDamage(int level)
        {
            if (m_AIStats != null) 
                return m_AIStats.GetScaledRandomDamage(level);
            return UnityEngine.Random.Range(10 + (level - 1) * 5, 31 + (level - 1) * 5);
        }

        public int GetAIAverageDamage()
        {
            if (m_AIStats != null) 
                return m_AIStats.damage.GetAverageDamage();
            return 20; 
        }

        public int GetAIAverageDamage(int level)
        {
            if (m_AIStats != null) 
                return m_AIStats.GetScaledAverageDamage(level);
            return 20 + (level - 1) * 5; 
        }

        public int GetAIMaxHealth(int level)
        {
            if (m_AIStats != null) 
            {
                int baseHealth = m_AIStats.GetScaledHealth(level);
                
                if (m_AIStats.bossLevelData != null && m_AIStats.bossLevelData.IsBossLevel(level))
                {
                    float multiplier = m_AIStats.bossLevelData.GetHealthMultiplier(level);
                    return Mathf.RoundToInt(baseHealth * multiplier);
                }
                
                return baseHealth;
            }
            int fallback = 100 + (level - 1) * 20;
            return fallback; 
        }

        public int GetAIMinDamage(int level)
        {
            if (m_AIStats != null) 
            {
                int baseDamage = m_AIStats.GetScaledMinDamage(level);
                
                if (m_AIStats.bossLevelData != null && m_AIStats.bossLevelData.IsBossLevel(level))
                {
                    float multiplier = m_AIStats.bossLevelData.GetDamageMultiplier(level);
                    return Mathf.RoundToInt(baseDamage * multiplier);
                }
                
                return baseDamage;
            }
            return 10 + (level - 1) * 5; 
        }

        public int GetAIMaxDamage(int level)
        {
            if (m_AIStats != null) 
            {
                int baseDamage = m_AIStats.GetScaledMaxDamage(level);
                
                if (m_AIStats.bossLevelData != null && m_AIStats.bossLevelData.IsBossLevel(level))
                {
                    float multiplier = m_AIStats.bossLevelData.GetDamageMultiplier(level);
                    return Mathf.RoundToInt(baseDamage * multiplier);
                }
                
                return baseDamage;
            }
            return 30 + (level - 1) * 5; 
        }

        public int GetVictoryReward(int victoryCount)
        {
            if (m_GameConfig != null) 
                return m_GameConfig.coinSystem.CalculateReward(victoryCount);
            return Mathf.Min(100 + (victoryCount * 50), 1000);
        }

        public int GetDefeatReward()
        {
            if (m_GameConfig != null) 
                return m_GameConfig.coinSystem.loseReward;
            return 25;
        }

        public int GetHealthUpgradePrice(int upgradeCount)
        {
            if (m_GameConfig != null) 
                return m_GameConfig.upgradeSystem.CalculateHealthUpgradePrice(upgradeCount);
            return Mathf.Max(0, 100 + (upgradeCount * 50)); 
        }

        public int GetPowerUpgradePrice(int upgradeCount)
        {
            if (m_GameConfig != null) 
                return m_GameConfig.upgradeSystem.CalculatePowerUpgradePrice(upgradeCount);
            return Mathf.Max(0, 150 + (upgradeCount * 75)); 
        }

        public float GetHealthScalingMultiplier(int level)
        {
            if (m_MasterData != null)
                return m_MasterData.levelScaling.GetHealthMultiplier(level);
            return 1f; 
        }

        public float GetDamageScalingMultiplier(int level)
        {
            if (m_MasterData != null)
                return m_MasterData.levelScaling.GetDamageMultiplier(level);
            return 1f; 
        }

        public float GetPowerScalingMultiplier(int level)
        {
            if (m_MasterData != null)
                return m_MasterData.levelScaling.GetPowerMultiplier(level);
            return 1f; 
        }
        
        public float GetPowerMeterSpeedWithBossBonus(int level)
        {
            if (m_PlayerStats != null && m_PlayerStats.power != null)
            {
                return m_PlayerStats.power.GetAnimSpeedWithBossBonus(level, m_PlayerStats.bossLevelData);
            }
            return 2f; 
        }
        
        public float GetCounterSpeedWithBossBonus(int level)
        {
            if (m_PlayerStats != null && m_PlayerStats.bossLevelData != null)
            {
                float baseCounterSpeed = 2f; 
                float cumulativeSpeedMultiplier = m_PlayerStats.bossLevelData.GetCumulativeSpeedMultiplier(level);
                return baseCounterSpeed * Mathf.Max(0f, cumulativeSpeedMultiplier);
            }
            return 2f;
        }
        
        public bool IsBossLevel(int level)
        {
            if (m_AIStats != null && m_AIStats.bossLevelData != null)
            {
                return m_AIStats.bossLevelData.IsBossLevel(level);
            }
            return false;
        }
        
        public string GetBossName(int level)
        {
            if (m_AIStats != null && m_AIStats.bossLevelData != null)
            {
                return m_AIStats.bossLevelData.GetBossName(level);
            }
            return "Normal Level";
        }

        public bool IsDataValid()
        {
            return m_PlayerStats != null && m_AIStats != null && m_GameConfig != null;
        }

        public string GetDataStatus()
        {
            var status = new System.Text.StringBuilder();
            status.AppendLine($"PlayerStats: {(m_PlayerStats != null ? "✓" : "✗")}");
            status.AppendLine($"AIStats: {(m_AIStats != null ? "✓" : "✗")}");
            status.AppendLine($"GameConfig: {(m_GameConfig != null ? "✓" : "✗")}");
            status.AppendLine($"MasterData: {(m_MasterData != null ? "✓" : "✗")}");
            return status.ToString();
        }

        [ContextMenu("Log Data Status")]
        private void LogDataStatus()
        {
            
        }

        [ContextMenu("Refresh Data")]
        private void RefreshDataContext()
        {
            RefreshData();
        }

        protected override void OnInitialize()
        {
            LoadData();
        }

        protected override void OnCleanup()
        {
            OnDataLoaded = null;
            OnDataChanged = null;
        }
    }
}