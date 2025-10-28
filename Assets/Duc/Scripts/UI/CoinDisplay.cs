using UnityEngine;
using TMPro;

namespace Duc
{
    public class CoinDisplay : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI m_CoinText;
        [SerializeField] private TextMeshProUGUI m_HealthPriceText;
        [SerializeField] private TextMeshProUGUI m_PowerPriceText;
        [SerializeField] private TextMeshProUGUI m_LevelText;
        [SerializeField] private TextMeshProUGUI m_VictoryRewardText;
        [SerializeField] private TextMeshProUGUI m_DefeatRewardText;
        [SerializeField] private TextMeshProUGUI m_HealthUpgradeCountText;
        [SerializeField] private TextMeshProUGUI m_PowerUpgradeCountText;

        // Cache the displayed reward to prevent it from updating after victory
        private int m_CurrentVictoryReward = -1;
        private bool m_IsShowingCurrentReward = false;

        private void Start()
        {
            UpdateDisplay();
        }

        private void OnEnable()
        {
            var persistentData = PersistentDataManager.Instance;
            if (persistentData != null)
            {
                persistentData.OnHealthUpgradePurchased -= UpdateDisplay;
                persistentData.OnPowerUpgradePurchased -= UpdateDisplay;
                persistentData.OnProgressReset -= UpdateDisplay;
                persistentData.OnCoinsChanged -= UpdateDisplay;

                persistentData.OnHealthUpgradePurchased += UpdateDisplay;
                persistentData.OnPowerUpgradePurchased += UpdateDisplay;
                persistentData.OnProgressReset += UpdateDisplay;
                persistentData.OnCoinsChanged += UpdateDisplay;
            }
            UpdateDisplay();
        }

        private void OnDisable()
        {
            var persistentData = PersistentDataManager.Instance;
            if (persistentData != null)
            {
                persistentData.OnHealthUpgradePurchased -= UpdateDisplay;
                persistentData.OnPowerUpgradePurchased -= UpdateDisplay;
                persistentData.OnProgressReset -= UpdateDisplay;
                persistentData.OnCoinsChanged -= UpdateDisplay;
            }
        }

        public void UpdateDisplay()
        {
            var coinManager = CoinManager.Get();
            if (coinManager == null) return;

            if (m_CoinText != null)
            {
                m_CoinText.text = coinManager.GetCurrentCoins().ToString();
            }

            if (m_HealthPriceText != null)
            {
                int healthPrice = coinManager.GetHealthUpgradePrice();
                m_HealthPriceText.text = healthPrice.ToString();
            }

            if (m_PowerPriceText != null)
            {
                int powerPrice = coinManager.GetPowerUpgradePrice();
                m_PowerPriceText.text = powerPrice.ToString();
            }

            if (m_LevelText != null)
            {
                int levelCount = coinManager.GetLevelCount();
                var dataManager = DataManager.Get();
                
                if (dataManager != null && dataManager.IsBossLevel(levelCount))
                {
                    m_LevelText.text = "BOSS";
                    m_LevelText.color = new Color(1f, 0f, 0f); 
                }
                else
                {
                    m_LevelText.text = "MATCH " + levelCount.ToString();
                    m_LevelText.color = Color.white;
                }
            }

            if (m_VictoryRewardText != null)
            {
                // If we're showing a cached reward (during victory screen), keep displaying it
                if (m_IsShowingCurrentReward && m_CurrentVictoryReward >= 0)
                {
                    m_VictoryRewardText.text = m_CurrentVictoryReward.ToString();
                }
                else
                {
                    int victoryReward = coinManager.CalculateReward();
                    m_VictoryRewardText.text = victoryReward.ToString();
                }
            }

            if (m_DefeatRewardText != null)
            {
                int defeatReward = coinManager.GetLoseReward();
                m_DefeatRewardText.text = defeatReward.ToString();
            }

            if (m_HealthUpgradeCountText != null)
            {
                int healthUpgradeCount = coinManager.GetHealthUpgradeCount();
                m_HealthUpgradeCountText.text = "(" + healthUpgradeCount.ToString() + ")";
            }

            if (m_PowerUpgradeCountText != null)
            {
                int powerUpgradeCount = coinManager.GetPowerUpgradeCount();
                m_PowerUpgradeCountText.text = "(" + powerUpgradeCount.ToString() + ")";
            }
        }

        public void OnHealthUpgradeButtonClick()
        {
            var coinManager = CoinManager.Get();
            if (coinManager != null)
            {
                coinManager.PurchaseHealthUpgrade();
            }
            UpdateDisplay();
        }

        public void OnPowerUpgradeButtonClick()
        {
            var coinManager = CoinManager.Get();
            if (coinManager != null)
            {
                coinManager.PurchasePowerUpgrade();
            }
            UpdateDisplay();
        }

        public void OnResetProgressButtonClick()
        {
            var coinManager = CoinManager.Get();
            if (coinManager != null)
            {
                coinManager.ResetProgress();
            }
            UpdateDisplay();
        }
        
        public void LockVictoryReward(int rewardAmount)
        {
            m_CurrentVictoryReward = rewardAmount;
            m_IsShowingCurrentReward = true;
            UpdateDisplay();
        }
        
        public void UnlockVictoryReward()
        {
            m_IsShowingCurrentReward = false;
            m_CurrentVictoryReward = -1;
        }
    }
}
