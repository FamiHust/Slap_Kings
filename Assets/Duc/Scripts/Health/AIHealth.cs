using UnityEngine;
using System.Collections;

namespace Duc
{
    public class AIHealth : BaseHealth
    {
        [Header("State Machine")]
        [SerializeField] private AIStateMachine m_StateMachine;
        
        [Header("Data References")]
        [SerializeField] private AIStatsData m_AIStatsData;

        protected override void Awake()
        {
            base.Awake();
            m_StateMachine = GetComponent<AIStateMachine>();
            // Don't set m_CurrentHealth here, wait for InitializeHealthForLevel()
        }

        protected override void Start()
        {
            // Start coroutine to ensure proper initialization order
            StartCoroutine(InitializeWithDelay());
        }

        protected override void InitializeHealth()
        {
            m_IsDead = false;
            // Don't set m_CurrentHealth here, wait for InitializeHealthForLevel()
        }

        protected override void SubscribeToEvents()
        {
            base.SubscribeToEvents();
            
            // Subscribe to AI stats updates
            var persistentData = PersistentDataManager.Instance;
            if (persistentData != null)
            {
                persistentData.OnAIStatsUpdated += OnAIStatsUpdated;
            }
        }

        protected override void UnsubscribeFromEvents()
        {
            base.UnsubscribeFromEvents();
            
            // Unsubscribe from events
            var persistentData = PersistentDataManager.Instance;
            if (persistentData != null)
            {
                persistentData.OnAIStatsUpdated -= OnAIStatsUpdated;
            }
        }

        protected override StateMachine GetStateMachine()
        {
            return m_StateMachine;
        }

        protected override void HandleDeath()
        {
            Debug.Log("AI has died!");

            // Notify GameManager about AI death
            var gameManager = GameManager.Get();
            if (gameManager != null)
            {
                gameManager.OnAIDied();
            }
        }

        private IEnumerator InitializeWithDelay()
        {
            // Wait a few frames to ensure PersistentDataManager is fully initialized
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            
            // Initialize health based on current level
            InitializeHealthForLevel();
            
            // Update UI after health is properly initialized
            UpdateHealthUI();
            
            // Debug current health values
            Debug.Log($"AI Health after delayed initialization: Current={m_CurrentHealth}, Max={m_MaxHealth}");
        }
        
        private void OnAIStatsUpdated()
        {
            // Refresh health when AI stats are updated
            InitializeHealthForLevel();
            UpdateHealthUI();
            Debug.Log($"AI Health updated from event: Current={m_CurrentHealth}, Max={m_MaxHealth}");
        }

        private void InitializeHealthForLevel()
        {
            var persistentData = PersistentDataManager.Instance;
            
            if (persistentData != null)
            {
                // Use current AI health from persistent data (only increases on victory)
                int persistentHealth = persistentData.GetCurrentAIHealth();
                Debug.Log($"PersistentData AI Health: {persistentHealth}");
                
                SetMaxHealth(persistentHealth, true);
                
                Debug.Log($"AI Health initialized from PersistentData: {m_CurrentHealth}/{m_MaxHealth}");
            }
            else
            {
                // Fallback to default health from inspector or base value
                Debug.LogWarning("PersistentDataManager not found, using default AI health");
                Debug.Log($"Using inspector MaxHealth: {m_MaxHealth}");
                m_CurrentHealth = m_MaxHealth;
            }
        }

        public void TakeDamageFromPowerMeter()
        {
            if (PowerMeter.Get() != null)
            {
                int powerValue = PowerMeter.Get().GetPowerValue();
                TakeDamage(powerValue);
            }
        }
        
        // Public method to force refresh AI health
        public void RefreshAIHealth()
        {
            InitializeHealthForLevel();
            UpdateHealthUI();
            Debug.Log($"AI Health manually refreshed: Current={m_CurrentHealth}, Max={m_MaxHealth}");
        }
    }
}