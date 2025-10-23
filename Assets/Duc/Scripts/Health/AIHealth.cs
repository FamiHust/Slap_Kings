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
        }

        protected override void Start()
        {
            StartCoroutine(InitializeWithDelay());
        }

        protected override void InitializeHealth()
        {
            m_IsDead = false;
        }

        protected override void SubscribeToEvents()
        {
            base.SubscribeToEvents();
            
            var persistentData = PersistentDataManager.Instance;
            if (persistentData != null)
            {
                persistentData.OnAIStatsUpdated += OnAIStatsUpdated;
            }
        }

        protected override void UnsubscribeFromEvents()
        {
            base.UnsubscribeFromEvents();
            
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

        protected override ICharacterStats GetCharacterStats()
        {
            return m_AIStatsData;
        }

        protected override void HandleDeath()
        {
            var gameManager = GameManager.Get();
            if (gameManager != null)
            {
                gameManager.OnAIDied();
            }
        }

        private IEnumerator InitializeWithDelay()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            
            InitializeHealthForLevel();
            
            UpdateHealthUI();
        }
        
        private void OnAIStatsUpdated()
        {
            InitializeHealthForLevel();
            UpdateHealthUI();
            
            // Update AI appearance when stats change
            UpdateAIAppearance();
        }
        
        private void UpdateAIAppearance()
        {
            var appearanceManager = GetComponent<AIAppearanceManager>();
            if (appearanceManager != null)
            {
                appearanceManager.UpdateAppearanceForCurrentLevel();
            }
        }

        private void InitializeHealthForLevel()
        {
            var persistentData = PersistentDataManager.Instance;
            
            if (persistentData != null)
            {
                int persistentHealth = persistentData.GetCurrentAIHealth();
                
                SetMaxHealth(persistentHealth, true);
            }
            else
            {
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
        
        public void RefreshAIHealth()
        {
            InitializeHealthForLevel();
            UpdateHealthUI();
        }
    }
}