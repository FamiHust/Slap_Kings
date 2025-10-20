using UnityEngine;

public class PersistentDataManager : MonoBehaviour
{
    private static PersistentDataManager s_Instance;
    
    // Persistent data
    private int m_CurrentCoins;
    private int m_VictoryCount;
    private int m_LevelCount;
    private int m_HealthUpgradeCount;
    private int m_PowerUpgradeCount;
    
    // Events
    public System.Action OnHealthUpgradePurchased;
    public System.Action OnPowerUpgradePurchased;
    public System.Action OnProgressReset;
    
    // Keys
    private const string COIN_KEY = "PlayerCoins";
    private const string VICTORY_COUNT_KEY = "VictoryCount";
    private const string LEVEL_COUNT_KEY = "LevelCount";
    private const string HEALTH_UPGRADE_COUNT_KEY = "HealthUpgradeCount";
    private const string POWER_UPGRADE_COUNT_KEY = "PowerUpgradeCount";
    
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
    }
    
    private void SaveData()
    {
        PlayerPrefs.SetInt(COIN_KEY, m_CurrentCoins);
        PlayerPrefs.SetInt(VICTORY_COUNT_KEY, m_VictoryCount);
        PlayerPrefs.SetInt(LEVEL_COUNT_KEY, m_LevelCount);
        PlayerPrefs.SetInt(HEALTH_UPGRADE_COUNT_KEY, m_HealthUpgradeCount);
        PlayerPrefs.SetInt(POWER_UPGRADE_COUNT_KEY, m_PowerUpgradeCount);
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
        SaveData();
        
        Debug.Log($"Player Victory! Reward: {reward}, Total Coins: {m_CurrentCoins}, Level: {m_LevelCount}");
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
        int basePrice = 100;
        int increment = 50;
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
        int basePrice = 150;
        int increment = 75;
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
    
    // Getters
    public int GetCurrentCoins() => m_CurrentCoins;
    public int GetVictoryCount() => m_VictoryCount;
    public int GetLevelCount() => m_LevelCount;
    public int GetHealthUpgradeCount() => m_HealthUpgradeCount;
    public int GetPowerUpgradeCount() => m_PowerUpgradeCount;
}
