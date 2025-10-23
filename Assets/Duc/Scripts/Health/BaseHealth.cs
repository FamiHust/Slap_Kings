using UnityEngine;

namespace Duc
{
    public abstract class BaseHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] protected int m_MaxHealth = 100;
        [SerializeField] protected int m_CurrentHealth;
        [SerializeField] protected bool m_IsDead = false;
        [SerializeField] protected int m_LastDamage = 0;
        
        [Header("UI")]
        [SerializeField] protected HealthUI m_HealthUI;
        
        public System.Action<int, int> OnHealthChanged; 
        public System.Action<int> OnDamageTaken;
        public System.Action OnDeath;
        public System.Action OnHealthUpgraded; 

        protected virtual void Awake()
        {
            InitializeHealth();
        }

        protected virtual void Start()
        {
            SetupHealthUI();
            SubscribeToEvents();
        }

        protected virtual void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #region Abstract Methods

        protected abstract void InitializeHealth();

        protected abstract void HandleDeath();

        protected abstract StateMachine GetStateMachine();
        
        protected abstract ICharacterStats GetCharacterStats();
        #endregion

        #region Virtual Methods
        protected virtual void SetupHealthUI()
        {
            if (m_HealthUI == null)
            {
                m_HealthUI = FindObjectOfType<HealthUI>();
            }
            
            UpdateHealthUI();
        }

        protected virtual void SubscribeToEvents()
        {
            // Override in derived classes for specific event subscriptions
        }

        protected virtual void UnsubscribeFromEvents()
        {
            // Override in derived classes for specific event unsubscriptions
        }

        protected virtual void UpdateHealthUI()
        {
            if (m_HealthUI != null)
            {
                if (this is PlayerHealth)
                    m_HealthUI.SetPlayerHealth(this as PlayerHealth);
                else if (this is AIHealth)
                    m_HealthUI.SetAIHealth(this as AIHealth);
            }
        }

        protected virtual void ActivateRagdoll()
        {
            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.enabled = false;
            }
            
            var rbs = GetComponentsInChildren<Rigidbody>(true);
            foreach (var rb in rbs)
            {
                if (rb != null) rb.isKinematic = false;
            }
            
            var cols = GetComponentsInChildren<Collider>(true);
            foreach (var col in cols)
            {
                if (col != null) col.enabled = true;
            }
        }

        protected virtual void ApplyKnockbackForce(ICharacterStats stats, int damage)
        {
            if (stats == null) 
            {
                Debug.LogWarning("ApplyKnockbackForce: stats is null");
                return;
            }

            var rbs = GetComponentsInChildren<Rigidbody>(true);
            if (rbs.Length == 0) 
            {
                Debug.LogWarning("ApplyKnockbackForce: No rigidbodies found");
                return;
            }

            // Calculate force based on damage and stats
            float force = stats.KnockbackForce + (damage * stats.KnockbackMultiplier);
            Vector3 direction = stats.KnockbackDirection;
            
            // Check if direction is zero or too small
            if (direction.magnitude < 0.01f)
            {
                Debug.LogWarning($"ApplyKnockbackForce: Direction is zero or too small: {stats.KnockbackDirection}. No force will be applied.");
                return; // Don't apply any force if direction is zero
            }
            
            direction = direction.normalized;

            Debug.Log($"ApplyKnockbackForce: Force={force}, Direction={direction}, Damage={damage}");

            // Apply force to rigidbodies
            foreach (var rb in rbs)
            {
                if (rb != null)
                {
                    // Make sure rigidbody is not kinematic before applying force
                    if (rb.isKinematic)
                    {
                        rb.isKinematic = false;
                    }
                    
                    if (stats.UseAllRigidbodies)
                    {
                        Vector3 finalForce = direction * force * stats.RigidbodyForceMultiplier;
                        rb.AddForce(finalForce, ForceMode.Impulse);
                        Debug.Log($"Applied force to {rb.name}: {finalForce}");
                    }
                    else
                    {
                        // Only apply to main rigidbody
                        var mainRb = GetComponent<Rigidbody>();
                        if (mainRb != null && mainRb == rb)
                        {
                            Vector3 finalForce = direction * force;
                            mainRb.AddForce(finalForce, ForceMode.Impulse);
                            Debug.Log($"Applied force to main rigidbody: {finalForce}");
                        }
                    }
                }
            }
        }
        #endregion

        #region Public Methods
        public virtual void TakeDamage(int damage)
        {
            if (m_IsDead || damage <= 0) return;

            m_LastDamage = damage;
            m_CurrentHealth = Mathf.Max(0, m_CurrentHealth - damage);

            OnDamageTaken?.Invoke(damage);
            OnHealthChanged?.Invoke(m_CurrentHealth, m_MaxHealth);

            if (m_CurrentHealth <= 0 && !m_IsDead)
            {
                Die();
            }
            else
            {
                var stateMachine = GetStateMachine();
                if (stateMachine != null)
                {
                    stateMachine.SetState(CharacterState.Attacking);
                }
            }
        }

        public virtual void Heal(int amount)
        {
            if (m_IsDead || amount <= 0) return;

            int oldHealth = m_CurrentHealth;
            m_CurrentHealth = Mathf.Min(m_MaxHealth, m_CurrentHealth + amount);
            
            int actualHealing = m_CurrentHealth - oldHealth;
            if (actualHealing > 0)
            {
                OnHealthChanged?.Invoke(m_CurrentHealth, m_MaxHealth);
            }
        }

        public virtual void SetMaxHealth(int newMaxHealth, bool fillToMax = false)
        {
            if (newMaxHealth <= 0) return;

            m_MaxHealth = newMaxHealth;
            
            if (fillToMax)
            {
                m_CurrentHealth = m_MaxHealth;
            }
            else
            {
                m_CurrentHealth = Mathf.Min(m_CurrentHealth, m_MaxHealth);
            }

            OnHealthUpgraded?.Invoke();
            OnHealthChanged?.Invoke(m_CurrentHealth, m_MaxHealth);
            UpdateHealthUI();
        }

        protected virtual void Die()
        {
            if (m_IsDead) return;

            m_IsDead = true;
            
            var stateMachine = GetStateMachine();
            if (stateMachine != null)
            {
                stateMachine.SetState(CharacterState.Dead);
            }

            // Apply knockback force BEFORE activating ragdoll
            var stats = GetCharacterStats();
            if (stats != null)
            {
                ApplyKnockbackForce(stats, m_LastDamage);
            }
            
            ActivateRagdoll();
            
            HandleDeath();
            
            OnDeath?.Invoke();
        }

        public virtual void Revive(int healthAmount = -1)
        {
            if (!m_IsDead) return;

            m_IsDead = false;
            m_CurrentHealth = healthAmount > 0 ? healthAmount : m_MaxHealth;
            
            OnHealthChanged?.Invoke(m_CurrentHealth, m_MaxHealth);
            UpdateHealthUI();
        }
        #endregion

        #region Getters
        public virtual int GetCurrentHealth() => m_CurrentHealth;
        public virtual int GetMaxHealth() => m_MaxHealth;
        public virtual bool IsDead() => m_IsDead;
        public virtual int GetLastDamage() => m_LastDamage;
        public virtual float GetHealthPercentage() => (float)m_CurrentHealth / m_MaxHealth;
        #endregion
    }
}
