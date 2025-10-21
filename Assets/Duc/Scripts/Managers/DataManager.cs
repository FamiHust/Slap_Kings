using UnityEngine;
using System;

namespace Duc
{
    public class DataManager : SingletonManager<DataManager>
    {
        [Header("Master Data")]
        [SerializeField] private CharacterStatsData m_MasterData;

        // Cached references for performance
        private PlayerStatsData m_PlayerStats;
        private AIStatsData m_AIStats;
        private GameConfigData m_GameConfig;

        // Events for data changes
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
            // Load from master data
            if (m_MasterData != null && m_MasterData.IsValid())
            {
                m_PlayerStats = m_MasterData.playerStats;
                m_AIStats = m_MasterData.aiStats;
                m_GameConfig = m_MasterData.gameConfig;
                
                Debug.Log("Data loaded from MasterData");
                Debug.Log($"AIStats loaded: {(m_AIStats != null ? "SUCCESS" : "NULL")}");
                if (m_AIStats != null)
                {
                    Debug.Log($"AIStats base health: {m_AIStats.BaseMaxHealth}");
                    Debug.Log($"AIStats scaled health for level 1: {m_AIStats.GetScaledHealth(1)}");
                }
            }
            else
            {
                // Try to find individual data assets
                LoadIndividualData();
                
                if (m_MasterData != null)
                {
                    Debug.LogWarning($"MasterData validation failed: {m_MasterData.GetValidationErrors()}");
                }
                else
                {
                    Debug.LogWarning("MasterData is not assigned in DataManager");
                }
            }
            
            OnDataLoaded?.Invoke();
        }

        private void LoadIndividualData()
        {
            // Try to find individual ScriptableObjects
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

        // Convenience methods with fallback values
        public int GetPlayerMaxHealth(int upgradeCount)
        {
            if (m_PlayerStats != null) 
                return m_PlayerStats.GetMaxHealthWithUpgrades(upgradeCount);
            return 100 + (upgradeCount * 10); // Fallback
        }

        public int GetPlayerPowerBonusDamage(int upgradeCount)
        {
            if (m_PlayerStats != null) 
                return m_PlayerStats.power.GetPowerBonusDamage(upgradeCount);
            return upgradeCount; // Fallback
        }

        public int GetAIRandomDamage()
        {
            if (m_AIStats != null) 
                return m_AIStats.damage.GetRandomDamage();
            return UnityEngine.Random.Range(10, 31); // Fallback
        }

        public int GetAIRandomDamage(int level)
        {
            if (m_AIStats != null) 
                return m_AIStats.GetScaledRandomDamage(level);
            return UnityEngine.Random.Range(10 + (level - 1) * 5, 31 + (level - 1) * 5); // Fallback with scaling
        }

        public int GetAIAverageDamage()
        {
            if (m_AIStats != null) 
                return m_AIStats.damage.GetAverageDamage();
            return 20; // Fallback
        }

        public int GetAIAverageDamage(int level)
        {
            if (m_AIStats != null) 
                return m_AIStats.GetScaledAverageDamage(level);
            return 20 + (level - 1) * 5; // Fallback with scaling
        }

        public int GetAIMaxHealth(int level)
        {
            if (m_AIStats != null) 
            {
                int result = m_AIStats.GetScaledHealth(level);
                Debug.Log($"DataManager.GetAIMaxHealth({level}) from AIStats: {result}");
                return result;
            }
            int fallback = 100 + (level - 1) * 20;
            Debug.LogWarning($"DataManager.GetAIMaxHealth({level}) using fallback: {fallback} (m_AIStats is null)");
            return fallback; // Fallback with scaling
        }

        public int GetAIMinDamage(int level)
        {
            if (m_AIStats != null) 
                return m_AIStats.GetScaledMinDamage(level);
            return 10 + (level - 1) * 5; // Fallback with scaling
        }

        public int GetAIMaxDamage(int level)
        {
            if (m_AIStats != null) 
                return m_AIStats.GetScaledMaxDamage(level);
            return 30 + (level - 1) * 5; // Fallback with scaling
        }

        public int GetVictoryReward(int victoryCount)
        {
            if (m_GameConfig != null) 
                return m_GameConfig.coinSystem.CalculateReward(victoryCount);
            return Mathf.Min(100 + (victoryCount * 50), 1000); // Fallback
        }

        public int GetDefeatReward()
        {
            if (m_GameConfig != null) 
                return m_GameConfig.coinSystem.loseReward;
            return 25; // Fallback
        }

        public int GetHealthUpgradePrice(int upgradeCount)
        {
            if (m_GameConfig != null) 
                return m_GameConfig.upgradeSystem.CalculateHealthUpgradePrice(upgradeCount);
            return Mathf.Max(0, 100 + (upgradeCount * 50)); // Fallback
        }

        public int GetPowerUpgradePrice(int upgradeCount)
        {
            if (m_GameConfig != null) 
                return m_GameConfig.upgradeSystem.CalculatePowerUpgradePrice(upgradeCount);
            return Mathf.Max(0, 150 + (upgradeCount * 75)); // Fallback
        }

        // Level scaling methods
        public float GetHealthScalingMultiplier(int level)
        {
            if (m_MasterData != null)
                return m_MasterData.levelScaling.GetHealthMultiplier(level);
            return 1f; // Fallback
        }

        public float GetDamageScalingMultiplier(int level)
        {
            if (m_MasterData != null)
                return m_MasterData.levelScaling.GetDamageMultiplier(level);
            return 1f; // Fallback
        }

        public float GetPowerScalingMultiplier(int level)
        {
            if (m_MasterData != null)
                return m_MasterData.levelScaling.GetPowerMultiplier(level);
            return 1f; // Fallback
        }

        // Validation
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

        // Debug methods
        [ContextMenu("Log Data Status")]
        private void LogDataStatus()
        {
            Debug.Log($"DataManager Status:\n{GetDataStatus()}");
        }

        [ContextMenu("Refresh Data")]
        private void RefreshDataContext()
        {
            RefreshData();
        }

        protected override void OnInitialize()
        {
            // DataManager specific initialization
            LoadData();
        }

        protected override void OnCleanup()
        {
            // DataManager specific cleanup
            OnDataLoaded = null;
            OnDataChanged = null;
        }
    }
}