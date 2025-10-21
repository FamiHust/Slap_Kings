using UnityEngine;

namespace Duc
{
    public class PlayerHealth : BaseHealth
    {
        [Header("State Machine")]
        [SerializeField] private PlayerStateMachine m_StateMachine;
        
        [Header("Data References")]
        [SerializeField] private PlayerStatsData m_PlayerStatsData;

        protected override void Awake()
        {
            base.Awake();
            m_StateMachine = GetComponent<PlayerStateMachine>();
        }

        protected override void Start()
        {
            base.Start();
            
            // Apply health upgrades at start
            ApplyHealthUpgrades();
        }

        protected override void InitializeHealth()
        {
            m_CurrentHealth = m_MaxHealth;
            m_IsDead = false;
        }

        protected override void SubscribeToEvents()
        {
            base.SubscribeToEvents();
            
            // Listen for health upgrade purchases to update max health immediately
            var persistentData = PersistentDataManager.Instance;
            if (persistentData != null)
            {
                persistentData.OnHealthUpgradePurchased -= OnHealthUpgradePurchased;
                persistentData.OnHealthUpgradePurchased += OnHealthUpgradePurchased;
            }
        }

        protected override void UnsubscribeFromEvents()
        {
            base.UnsubscribeFromEvents();
            
            var persistentData = PersistentDataManager.Instance;
            if (persistentData != null)
            {
                persistentData.OnHealthUpgradePurchased -= OnHealthUpgradePurchased;
            }
        }

        private void ApplyHealthUpgrades()
        {
            var coinManager = CoinManager.Get();
            var dataManager = DataManager.Get();
            if (coinManager != null && dataManager != null)
            {
                int upgrades = coinManager.GetHealthUpgradeCount();
                int newMax = dataManager.GetPlayerMaxHealth(upgrades);
                if (newMax > 0)
                {
                    SetMaxHealth(newMax, true);
                }
            }
        }

        private void OnHealthUpgradePurchased()
        {
            var persistentData = PersistentDataManager.Instance;
            var dataManager = DataManager.Get();
            if (persistentData == null || dataManager == null) return;
            
            int upgrades = persistentData.GetHealthUpgradeCount();
            int newMax = dataManager.GetPlayerMaxHealth(upgrades);
            if (newMax <= 0) return;

            // Increase max health and fill current health to new max
            SetMaxHealth(newMax, true);
        }

        protected override void HandleDeath()
        {
            Debug.Log("Player has died! Game Over!");
            
            var gameplayInput = GetComponent<GameplayInput>();
            if (gameplayInput != null)
            {
                gameplayInput.enabled = false;
            }

            // Notify GameManager about player death
            var gameManager = GameManager.Get();
            if (gameManager != null)
            {
                gameManager.OnPlayerDied();
            }
        }

        protected override StateMachine GetStateMachine()
        {
            return m_StateMachine;
        }
    }
}
