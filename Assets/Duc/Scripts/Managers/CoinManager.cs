using UnityEngine;

public class CoinManager : SingletonManager<CoinManager>
{
    [Header("Data References")]
    [SerializeField] private GameConfigData m_GameConfigData;
    [SerializeField] private PlayerStatsData m_PlayerStatsData;
    
    private PersistentDataManager m_PersistentData;
    
    protected override void Awake()
    {
        base.Awake();
        m_PersistentData = PersistentDataManager.Instance;
    }
    
    private void Start()
    {
        ReacquireReferences();
    }

    private void ReacquireReferences()
    {
        // Auto-assign data from DataManager if not set (moved to Start to ensure DataManager is initialized)
        if (m_GameConfigData == null && DataManager.Get() != null)
        {
            m_GameConfigData = DataManager.Get().GameConfig;
        }
        
        if (m_PlayerStatsData == null && DataManager.Get() != null)
        {
            m_PlayerStatsData = DataManager.Get().PlayerStats;
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (m_PersistentData != null)
        {
            m_PersistentData.OnHealthUpgradePurchased = null;
            m_PersistentData.OnPowerUpgradePurchased = null;
            m_PersistentData.OnProgressReset = null;
        }
    }

    public void OnPlayerVictory()
    {
        if (m_PersistentData != null)
        {
            m_PersistentData.OnPlayerVictory();
        }
    }

    public void OnPlayerDefeat()
    {
        if (m_PersistentData != null)
        {
            m_PersistentData.OnPlayerDefeat();
        }
    }

    public int CalculateReward()
    {
        if (m_PersistentData != null)
        {
            var dataManager = DataManager.Get();
            if (dataManager != null)
            {
                return dataManager.GetVictoryReward(m_PersistentData.GetVictoryCount());
            }
        }
        
        // Fallback calculation
        int baseReward = 100;
        int rewardIncrement = 50;
        int maxReward = 1000;
        
        int victoryCount = m_PersistentData != null ? m_PersistentData.GetVictoryCount() : 0;
        int reward = baseReward + (victoryCount * rewardIncrement);
        return Mathf.Min(reward, maxReward);
    }

    public int GetLoseReward()
    {
        if (m_PersistentData != null)
        {
            var dataManager = DataManager.Get();
            if (dataManager != null)
            {
                return dataManager.GetDefeatReward();
            }
        }
        
        return 25; // Fallback
    }

    public bool CanAffordHealthUpgrade()
    {
        return m_PersistentData != null && m_PersistentData.CanAffordHealthUpgrade();
    }

    public bool CanAffordPowerUpgrade()
    {
        return m_PersistentData != null && m_PersistentData.CanAffordPowerUpgrade();
    }

    public void PurchaseHealthUpgrade()
    {
        if (m_PersistentData != null)
        {
            m_PersistentData.PurchaseHealthUpgrade();
        }
    }

    public void PurchasePowerUpgrade()
    {
        if (m_PersistentData != null)
        {
            m_PersistentData.PurchasePowerUpgrade();
        }
    }

    public int GetHealthUpgradePrice()
    {
        return m_PersistentData != null ? m_PersistentData.GetHealthUpgradePrice() : 100;
    }

    public int GetPowerUpgradePrice()
    {
        return m_PersistentData != null ? m_PersistentData.GetPowerUpgradePrice() : 150;
    }

    public int GetPowerBonusDamage()
    {
        if (m_PersistentData != null)
        {
            var dataManager = DataManager.Get();
            if (dataManager != null)
            {
                return dataManager.GetPlayerPowerBonusDamage(m_PersistentData.GetPowerUpgradeCount());
            }
        }
        
        int powerUpgradeCount = m_PersistentData != null ? m_PersistentData.GetPowerUpgradeCount() : 0;
        return powerUpgradeCount * 1; 
    }

    public void ResetProgress()
    {
        if (m_PersistentData != null)
        {
            m_PersistentData.ResetProgress();
        }
    }

    public int GetCurrentCoins() => m_PersistentData != null ? m_PersistentData.GetCurrentCoins() : 0;
    public int GetVictoryCount() => m_PersistentData != null ? m_PersistentData.GetVictoryCount() : 0;
    public int GetLevelCount() => m_PersistentData != null ? m_PersistentData.GetLevelCount() : 1;
    public int GetHealthUpgradeCount() => m_PersistentData != null ? m_PersistentData.GetHealthUpgradeCount() : 0;
    public int GetPowerUpgradeCount() => m_PersistentData != null ? m_PersistentData.GetPowerUpgradeCount() : 0;

    [System.Obsolete("Use CalculateReward() instead")]
    public void SetBaseReward(int value) { }
    
    [System.Obsolete("Use CalculateReward() instead")]
    public void SetRewardIncrement(int value) { }
    
    [System.Obsolete("Use CalculateReward() instead")]
    public void SetMaxReward(int value) { }
}
