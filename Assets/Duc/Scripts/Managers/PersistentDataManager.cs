using UnityEngine;
using System.Collections;

namespace Duc
{
    public class PersistentDataManager : MonoBehaviour
    {
        private static PersistentDataManager s_Instance;
        
        // Persistent data
        private int m_CurrentCoins;
        private int m_VictoryCount;
        private int m_LevelCount;
        private int m_HealthUpgradeCount;
        private int m_PowerUpgradeCount;
        
        // AI Stats tracking (only increases on victory)
        private int m_CurrentAIHealth;
        private int m_CurrentAIMinDamage;
        private int m_CurrentAIMaxDamage;
        
        // Events
        public System.Action OnHealthUpgradePurchased;
        public System.Action OnPowerUpgradePurchased;
        public System.Action OnProgressReset;
        public System.Action OnAIStatsUpdated;
        
        // Keys
        private const string COIN_KEY = "PlayerCoins";
        private const string VICTORY_COUNT_KEY = "VictoryCount";
        private const string LEVEL_COUNT_KEY = "LevelCount";
        private const string HEALTH_UPGRADE_COUNT_KEY = "HealthUpgradeCount";
        private const string POWER_UPGRADE_COUNT_KEY = "PowerUpgradeCount";
        private const string AI_HEALTH_KEY = "CurrentAIHealth";
        private const string AI_MIN_DAMAGE_KEY = "CurrentAIMinDamage";
        private const string AI_MAX_DAMAGE_KEY = "CurrentAIMaxDamage";
        
        public static PersistentDataManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = FindObjectOfType<PersistentDataManager>();
                    if (s_Instance == null)
                    {
                        GameObject go = new GameObject("PersistentDataManager");
                        s_Instance = go.AddComponent<PersistentDataManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return s_Instance;
            }
        }
        
        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadData();
            }
            else if (s_Instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void LoadData()
        {
            m_CurrentCoins = PlayerPrefs.GetInt(COIN_KEY, 0);
            m_VictoryCount = PlayerPrefs.GetInt(VICTORY_COUNT_KEY, 0);
            m_LevelCount = PlayerPrefs.GetInt(LEVEL_COUNT_KEY, 1);
            m_HealthUpgradeCount = PlayerPrefs.GetInt(HEALTH_UPGRADE_COUNT_KEY, 0);
            m_PowerUpgradeCount = PlayerPrefs.GetInt(POWER_UPGRADE_COUNT_KEY, 0);
            
            // Load AI stats - always use base values first, then refresh from DataManager if available
            m_CurrentAIHealth = PlayerPrefs.GetInt(AI_HEALTH_KEY, 200); // Base health from AIStatsData
            m_CurrentAIMinDamage = PlayerPrefs.GetInt(AI_MIN_DAMAGE_KEY, 50); // Base min damage
            m_CurrentAIMaxDamage = PlayerPrefs.GetInt(AI_MAX_DAMAGE_KEY, 100); // Base max damage
            
            Debug.Log($"PersistentDataManager loaded AI stats from PlayerPrefs: Health={m_CurrentAIHealth}, Damage={m_CurrentAIMinDamage}-{m_CurrentAIMaxDamage}");
            
            // Try to refresh from DataManager if it's available
            StartCoroutine(RefreshAIStatsFromDataManager());
        }
        
        private void SaveData()
        {
            PlayerPrefs.SetInt(COIN_KEY, m_CurrentCoins);
            PlayerPrefs.SetInt(VICTORY_COUNT_KEY, m_VictoryCount);
            PlayerPrefs.SetInt(LEVEL_COUNT_KEY, m_LevelCount);
            PlayerPrefs.SetInt(HEALTH_UPGRADE_COUNT_KEY, m_HealthUpgradeCount);
            PlayerPrefs.SetInt(POWER_UPGRADE_COUNT_KEY, m_PowerUpgradeCount);
            PlayerPrefs.SetInt(AI_HEALTH_KEY, m_CurrentAIHealth);
            PlayerPrefs.SetInt(AI_MIN_DAMAGE_KEY, m_CurrentAIMinDamage);
            PlayerPrefs.SetInt(AI_MAX_DAMAGE_KEY, m_CurrentAIMaxDamage);
            PlayerPrefs.Save();
        }
        
        // Coin management
        public void OnPlayerVictory()
        {
            var dataManager = DataManager.Get();
            int reward = 100; // Fallback
            if (dataManager != null)
            {
                reward = dataManager.GetVictoryReward(m_VictoryCount);
            }
            
            m_CurrentCoins += reward;
            m_VictoryCount++;
            m_LevelCount++;
            
            // Increase AI stats only on victory
            IncreaseAIStats();
            
            SaveData();
            
            Debug.Log($"Player Victory! Reward: {reward}, Total Coins: {m_CurrentCoins}, Level: {m_LevelCount}");
            Debug.Log($"AI Stats increased - Health: {m_CurrentAIHealth}, Damage: {m_CurrentAIMinDamage}-{m_CurrentAIMaxDamage}");
        }
        
        public void OnPlayerDefeat()
        {
            var dataManager = DataManager.Get();
            int reward = 25; // Fallback
            if (dataManager != null)
            {
                reward = dataManager.GetDefeatReward();
            }
            
            m_CurrentCoins += reward;
            SaveData();
            
            Debug.Log($"Player Defeat! Reward: {reward}, Total Coins: {m_CurrentCoins}");
            Debug.Log($"AI Stats remain unchanged - Health: {m_CurrentAIHealth}, Damage: {m_CurrentAIMinDamage}-{m_CurrentAIMaxDamage}");
        }
        
        // Upgrade system
        public bool CanAffordHealthUpgrade()
        {
            return m_CurrentCoins >= GetHealthUpgradePrice();
        }
        
        public bool CanAffordPowerUpgrade()
        {
            return m_CurrentCoins >= GetPowerUpgradePrice();
        }
        
        public void PurchaseHealthUpgrade()
        {
            if (CanAffordHealthUpgrade())
            {
                int price = GetHealthUpgradePrice();
                m_CurrentCoins -= price;
                m_HealthUpgradeCount++;
                SaveData();
                
                OnHealthUpgradePurchased?.Invoke();
                Debug.Log($"Health Upgrade Purchased! Cost: {price}, Remaining Coins: {m_CurrentCoins}");
            }
        }
        
        public void PurchasePowerUpgrade()
        {
            if (CanAffordPowerUpgrade())
            {
                int price = GetPowerUpgradePrice();
                m_CurrentCoins -= price;
                m_PowerUpgradeCount++;
                SaveData();
                
                OnPowerUpgradePurchased?.Invoke();
                Debug.Log($"Power Upgrade Purchased! Cost: {price}, Remaining Coins: {m_CurrentCoins}");
            }
        }
        
        public int GetHealthUpgradePrice()
        {
            var dataManager = DataManager.Get();
            if (dataManager != null)
            {
                return dataManager.GetHealthUpgradePrice(m_HealthUpgradeCount);
            }
            
            // Fallback calculation
            int basePrice = 20;
            int increment = 10;
            return Mathf.Max(0, basePrice + (m_HealthUpgradeCount * increment));
        }
        
        public int GetPowerUpgradePrice()
        {
            var dataManager = DataManager.Get();
            if (dataManager != null)
            {
                return dataManager.GetPowerUpgradePrice(m_PowerUpgradeCount);
            }
            
            // Fallback calculation
            int basePrice = 20;
            int increment = 10;
            return Mathf.Max(0, basePrice + (m_PowerUpgradeCount * increment));
        }
        
        public void ResetProgress()
        {
            m_CurrentCoins = 0;
            m_VictoryCount = 0;
            m_LevelCount = 1;
            m_HealthUpgradeCount = 0;
            m_PowerUpgradeCount = 0;
            SaveData();
            
            OnHealthUpgradePurchased?.Invoke();
            OnPowerUpgradePurchased?.Invoke();
            OnProgressReset?.Invoke();
            
            Debug.Log("Progress Reset!");
        }
        
        // AI Stats management
        private void IncreaseAIStats()
        {
            var dataManager = DataManager.Get();
            if (dataManager != null)
            {
                // Use current level (already incremented in OnPlayerVictory)
                int currentLevel = m_LevelCount;
                m_CurrentAIHealth = dataManager.GetAIMaxHealth(currentLevel);
                m_CurrentAIMinDamage = dataManager.GetAIMinDamage(currentLevel);
                m_CurrentAIMaxDamage = dataManager.GetAIMaxDamage(currentLevel);
                
                Debug.Log($"IncreaseAIStats: Using level {currentLevel}, Health={m_CurrentAIHealth}, Damage={m_CurrentAIMinDamage}-{m_CurrentAIMaxDamage}");
            }
            else
            {
                m_CurrentAIHealth += 20;
                m_CurrentAIMinDamage += 5;
                m_CurrentAIMaxDamage += 5;
                
                Debug.Log($"IncreaseAIStats: Using fallback scaling, Health={m_CurrentAIHealth}, Damage={m_CurrentAIMinDamage}-{m_CurrentAIMaxDamage}");
            }
        }
        
        // Getters
        public int GetCurrentCoins() => m_CurrentCoins;
        public int GetVictoryCount() => m_VictoryCount;
        public int GetLevelCount() => m_LevelCount;
        public int GetHealthUpgradeCount() => m_HealthUpgradeCount;
        public int GetPowerUpgradeCount() => m_PowerUpgradeCount;
        
        // AI Stats getters
        public int GetCurrentAIHealth() 
        {
            Debug.Log($"GetCurrentAIHealth called, returning: {m_CurrentAIHealth}");
            return m_CurrentAIHealth;
        }
        public int GetCurrentAIMinDamage() => m_CurrentAIMinDamage;
        public int GetCurrentAIMaxDamage() => m_CurrentAIMaxDamage;
        
        // Refresh AI stats from DataManager if it becomes available later
        private IEnumerator RefreshAIStatsFromDataManager()
        {
            // Wait a few frames for DataManager to be fully initialized
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            
            var dataManager = DataManager.Get();
            if (dataManager != null)
            {
                int newHealth = dataManager.GetAIMaxHealth(1);
                int newMinDamage = dataManager.GetAIMinDamage(1);
                int newMaxDamage = dataManager.GetAIMaxDamage(1);
                
                // Check if we have old/incorrect values and update them
                bool needsUpdate = false;
                
                // If we have old values (100, 10, 30) or no saved values, update to correct values
                if (m_CurrentAIHealth == 100 && m_CurrentAIMinDamage == 10 && m_CurrentAIMaxDamage == 30)
                {
                    needsUpdate = true;
                    Debug.Log("Detected old AI stats values, updating to correct values");
                }
                else if (!PlayerPrefs.HasKey(AI_HEALTH_KEY))
                {
                    needsUpdate = true;
                    Debug.Log("No saved AI stats found, using DataManager values");
                }
                
                if (needsUpdate)
                {
                    m_CurrentAIHealth = newHealth;
                    m_CurrentAIMinDamage = newMinDamage;
                    m_CurrentAIMaxDamage = newMaxDamage;
                    
                    Debug.Log($"PersistentDataManager refreshed AI stats from DataManager: Health={m_CurrentAIHealth}, Damage={m_CurrentAIMinDamage}-{m_CurrentAIMaxDamage}");
                    SaveData();
                    
                    // Notify that AI stats have been updated
                    OnAIStatsUpdated?.Invoke();
                    
                    // Also try to refresh AI health directly
                    RefreshAIHealthInScene();
                }
            }
        }
        
        // Method to refresh AI health in the current scene
        private void RefreshAIHealthInScene()
        {
            var aiHealth = FindObjectOfType<AIHealth>();
            if (aiHealth != null)
            {
                aiHealth.RefreshAIHealth();
                Debug.Log("AI Health in scene refreshed successfully");
            }
        else
        {
            Debug.LogWarning("AIHealth component not found in scene");
        }
    }

    private void OnApplicationQuit()
    {
        // Save data before quitting
        SaveData();
        
        // Cleanup events to prevent memory leaks
        OnHealthUpgradePurchased = null;
        OnPowerUpgradePurchased = null;
        OnProgressReset = null;
        OnAIStatsUpdated = null;
    }

    private void OnDestroy()
    {
        // Reset static instance when destroyed
        if (s_Instance == this)
        {
            s_Instance = null;
        }
    }
}
}
