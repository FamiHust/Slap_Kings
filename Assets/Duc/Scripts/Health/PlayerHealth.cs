using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int m_MaxHealth = 100;
    [SerializeField] private int m_CurrentHealth;
    [SerializeField] private bool m_IsDead = false;
    [SerializeField] private int m_LastDamage = 0;
    
    [Header("State Machine")]
    [SerializeField] private PlayerStateMachine m_StateMachine;
    
    [Header("UI")]
    [SerializeField] private HealthUI m_HealthUI;
    
    [Header("Data References")]
    [SerializeField] private PlayerStatsData m_PlayerStatsData;

    protected virtual void Awake()
    {
        m_CurrentHealth = m_MaxHealth;
        m_IsDead = false;
        m_StateMachine = GetComponent<PlayerStateMachine>();
    }

    protected virtual void Start()
    {
        if (m_HealthUI == null)
        {
            m_HealthUI = FindObjectOfType<HealthUI>();
        }
        else
        {
            m_HealthUI.SetPlayerHealth(this);
        }

        // Apply health upgrades at start
        var coinManager = CoinManager.Get();
        var dataManager = DataManager.Get();
        if (coinManager != null && dataManager != null)
        {
            int upgrades = coinManager.GetHealthUpgradeCount();
            int newMax = dataManager.GetPlayerMaxHealth(upgrades);
            if (newMax > 0)
            {
                m_MaxHealth = newMax;
                m_CurrentHealth = m_MaxHealth;
            }
        }

        // Listen for health upgrade purchases to update max health immediately
        var persistentData = PersistentDataManager.Instance;
        if (persistentData != null)
        {
            persistentData.OnHealthUpgradePurchased -= OnHealthUpgradePurchased;
            persistentData.OnHealthUpgradePurchased += OnHealthUpgradePurchased;
        }
    }

    private void OnDestroy()
    {
        var persistentData = PersistentDataManager.Instance;
        if (persistentData != null)
        {
            persistentData.OnHealthUpgradePurchased -= OnHealthUpgradePurchased;
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
        m_MaxHealth = newMax;
        m_CurrentHealth = m_MaxHealth;

        // Refresh UI immediately
        if (m_HealthUI != null)
        {
            m_HealthUI.SetPlayerHealth(this);
        }
    }

    public virtual void TakeDamage(int damage)
    {
        if (m_IsDead || damage <= 0) return;

        m_LastDamage = damage; // Store last damage for ragdoll effect
        m_CurrentHealth = Mathf.Max(0, m_CurrentHealth - damage);

        if (m_CurrentHealth <= 0 && !m_IsDead)
        {
            Die();
        }
        else
        {
            if (m_StateMachine != null)
            {
                m_StateMachine.SetState(CharacterState.Attacking);
            }
        }
    }

    protected virtual void Die()
    {
        if (m_IsDead) return;

        m_IsDead = true;
        
        if (m_StateMachine != null)
        {
            m_StateMachine.SetState(CharacterState.Dead);
        }
        
        Debug.Log("Player has died! Game Over!");
        
        var gameplayInput = GetComponent<GameplayInput>();
        if (gameplayInput != null)
        {
            gameplayInput.enabled = false;
        }

        // Activate ragdoll at the last hit: disable animator and enable rigidbodies/colliders
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

        // Notify GameManager about player death
        var gameManager = GameManager.Get();
        if (gameManager != null)
        {
            gameManager.OnPlayerDied();
        }
    }

    public virtual int GetCurrentHealth()
    {
        return m_CurrentHealth;
    }

    public virtual int GetMaxHealth()
    {
        return m_MaxHealth;
    }

    public virtual bool IsDead()
    {
        return m_IsDead;
    }

    public virtual int GetLastDamage()
    {
        return m_LastDamage;
    }
}
