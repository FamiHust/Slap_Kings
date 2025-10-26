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

        private void Start()
        {
            UpdateDisplay();
        }

        private void OnEnable()
        {
            // Subscribe to events when UI becomes active
            var persistentData = PersistentDataManager.Instance;
            if (persistentData != null)
            {
                persistentData.OnHealthUpgradePurchased -= UpdateDisplay; // avoid double subscribe
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
            // Unsubscribe when UI is hidden/disabled
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

            // Update coin display
            if (m_CoinText != null)
            {
                m_CoinText.text = coinManager.GetCurrentCoins().ToString();
            }

            // Update health upgrade price
            if (m_HealthPriceText != null)
            {
                int healthPrice = coinManager.GetHealthUpgradePrice();
                m_HealthPriceText.text = healthPrice.ToString();
            }

            // Update power upgrade price
            if (m_PowerPriceText != null)
            {
                int powerPrice = coinManager.GetPowerUpgradePrice();
                m_PowerPriceText.text = powerPrice.ToString();
            }

            // Update level display
            if (m_LevelText != null)
            {
                m_LevelText.text = "LEVEL" + coinManager.GetLevelCount().ToString();
            }

            // Update victory reward display
            if (m_VictoryRewardText != null)
            {
                int victoryReward = coinManager.CalculateReward();
                m_VictoryRewardText.text = victoryReward.ToString();
            }

            // Update defeat reward display
            if (m_DefeatRewardText != null)
            {
                int defeatReward = coinManager.GetLoseReward();
                m_DefeatRewardText.text = defeatReward.ToString();
            }

            // Update health upgrade count display
            if (m_HealthUpgradeCountText != null)
            {
                int healthUpgradeCount = coinManager.GetHealthUpgradeCount();
                m_HealthUpgradeCountText.text = "(" + healthUpgradeCount.ToString() + ")";
            }

            // Update power upgrade count display
            if (m_PowerUpgradeCountText != null)
            {
                int powerUpgradeCount = coinManager.GetPowerUpgradeCount();
                m_PowerUpgradeCountText.text = "(" + powerUpgradeCount.ToString() + ")";
            }
        }

        // Called by UI buttons
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
    }
}
