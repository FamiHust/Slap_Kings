using UnityEngine;
using System.Collections;

namespace Duc
{
    public class PersistentDataManager : MonoBehaviour
    {
        private static PersistentDataManager s_Instance;
        
        private int m_CurrentCoins;
        private int m_VictoryCount;
        private int m_LevelCount;
        private int m_HealthUpgradeCount;
        private int m_PowerUpgradeCount;
        
        private int m_CurrentAIHealth;
        private int m_CurrentAIMinDamage;
        private int m_CurrentAIMaxDamage;
        
        public System.Action OnHealthUpgradePurchased;
        public System.Action OnPowerUpgradePurchased;
        public System.Action OnProgressReset;
        public System.Action OnAIStatsUpdated;
        
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
            
            m_CurrentAIHealth = PlayerPrefs.GetInt(AI_HEALTH_KEY, 200); 
            m_CurrentAIMinDamage = PlayerPrefs.GetInt(AI_MIN_DAMAGE_KEY, 50); 
            m_CurrentAIMaxDamage = PlayerPrefs.GetInt(AI_MAX_DAMAGE_KEY, 100);
            
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
            
            IncreaseAIStats();
            
            SaveData();
        }
        
        public void OnPlayerDefeat()
        {
            var dataManager = DataManager.Get();
            int reward = 25; 
            if (dataManager != null)
            {
                reward = dataManager.GetDefeatReward();
            }
            
            m_CurrentCoins += reward;
            SaveData();
        }
        
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
            }
        }
        
        public int GetHealthUpgradePrice()
        {
            var dataManager = DataManager.Get();
            if (dataManager != null)
            {
                return dataManager.GetHealthUpgradePrice(m_HealthUpgradeCount);
            }
            
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
        }
        
        private void IncreaseAIStats()
        {
            var dataManager = DataManager.Get();
            if (dataManager != null)
            {
                int currentLevel = m_LevelCount;
                m_CurrentAIHealth = dataManager.GetAIMaxHealth(currentLevel);
                m_CurrentAIMinDamage = dataManager.GetAIMinDamage(currentLevel);
                m_CurrentAIMaxDamage = dataManager.GetAIMaxDamage(currentLevel);
                
            }
            else
            {
                m_CurrentAIHealth += 20;
                m_CurrentAIMinDamage += 5;
                m_CurrentAIMaxDamage += 5;
            }
        }
        
        public int GetCurrentCoins() => m_CurrentCoins;
        public int GetVictoryCount() => m_VictoryCount;
        public int GetLevelCount() => m_LevelCount;
        public int GetHealthUpgradeCount() => m_HealthUpgradeCount;
        public int GetPowerUpgradeCount() => m_PowerUpgradeCount;
        
        public int GetCurrentAIHealth() 
        {
            return m_CurrentAIHealth;
        }
        public int GetCurrentAIMinDamage() => m_CurrentAIMinDamage;
        public int GetCurrentAIMaxDamage() => m_CurrentAIMaxDamage;
        
        private IEnumerator RefreshAIStatsFromDataManager()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            
            var dataManager = DataManager.Get();
            if (dataManager != null)
            {
                int newHealth = dataManager.GetAIMaxHealth(1);
                int newMinDamage = dataManager.GetAIMinDamage(1);
                int newMaxDamage = dataManager.GetAIMaxDamage(1);
                
                bool needsUpdate = false;
                
                if (m_CurrentAIHealth == 100 && m_CurrentAIMinDamage == 10 && m_CurrentAIMaxDamage == 30)
                {
                    needsUpdate = true;
                }
                else if (!PlayerPrefs.HasKey(AI_HEALTH_KEY))
                {
                    needsUpdate = true;
                }
                
                if (needsUpdate)
                {
                    m_CurrentAIHealth = newHealth;
                    m_CurrentAIMinDamage = newMinDamage;
                    m_CurrentAIMaxDamage = newMaxDamage;
                    
                    SaveData();
                    
                    OnAIStatsUpdated?.Invoke();
                    
                    RefreshAIHealthInScene();
                }
            }
        }
        
        private void RefreshAIHealthInScene()
        {
            var aiHealth = FindObjectOfType<AIHealth>();
            if (aiHealth != null)
            {
                aiHealth.RefreshAIHealth();
            }
        }

        private void OnApplicationQuit()
        {
            SaveData();
            
            OnHealthUpgradePurchased = null;
            OnPowerUpgradePurchased = null;
            OnProgressReset = null;
            OnAIStatsUpdated = null;
        }

        private void OnDestroy()
        {
            if (s_Instance == this)
            {
                s_Instance = null;
            }
        }
    }
}
