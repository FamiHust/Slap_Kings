using UnityEngine;

public class AIHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int m_MaxHealth = 200;
    [SerializeField] private int m_CurrentHealth;
    [SerializeField] private bool m_IsDead = false;
    [SerializeField] private int m_LastDamage = 0;
    
    [Header("State Machine")]
    [SerializeField] private AIStateMachine m_StateMachine;
    
    [Header("UI")]
    [SerializeField] private HealthUI m_HealthUI;
    
    [Header("Data References")]
    [SerializeField] private AIStatsData m_AIStatsData;

    protected virtual void Awake()
    {
        m_IsDead = false;
        m_StateMachine = GetComponent<AIStateMachine>();
        // Don't set m_CurrentHealth here, wait for InitializeHealthForLevel()
    }

    protected virtual void Start()
    {
        // Start coroutine to ensure proper initialization order
        StartCoroutine(InitializeWithDelay());
    }
    
    private System.Collections.IEnumerator InitializeWithDelay()
    {
        // Wait a few frames to ensure PersistentDataManager is fully initialized
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        // Initialize health based on current level
        InitializeHealthForLevel();
        
        if (m_HealthUI == null)
        {
            m_HealthUI = FindObjectOfType<HealthUI>();
        }
        
        // Update UI after health is properly initialized
        UpdateHealthUI();
        
        // Subscribe to AI stats updates
        var persistentData = PersistentDataManager.Instance;
        if (persistentData != null)
        {
            persistentData.OnAIStatsUpdated += OnAIStatsUpdated;
        }
        
        // Debug current health values
        Debug.Log($"AI Health after delayed initialization: Current={m_CurrentHealth}, Max={m_MaxHealth}");
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        var persistentData = PersistentDataManager.Instance;
        if (persistentData != null)
        {
            persistentData.OnAIStatsUpdated -= OnAIStatsUpdated;
        }
    }
    
    private void OnAIStatsUpdated()
    {
        // Refresh health when AI stats are updated
        InitializeHealthForLevel();
        UpdateHealthUI();
        Debug.Log($"AI Health updated from event: Current={m_CurrentHealth}, Max={m_MaxHealth}");
    }
    
    private void UpdateHealthUI()
    {
        if (m_HealthUI != null)
        {
            m_HealthUI.SetAIHealth(this);
            Debug.Log($"AI Health UI updated: Current={m_CurrentHealth}, Max={m_MaxHealth}");
        }
    }

    private void InitializeHealthForLevel()
    {
        var persistentData = PersistentDataManager.Instance;
        
        if (persistentData != null)
        {
            // Use current AI health from persistent data (only increases on victory)
            int persistentHealth = persistentData.GetCurrentAIHealth();
            Debug.Log($"PersistentData AI Health: {persistentHealth}");
            
            m_MaxHealth = persistentHealth;
            m_CurrentHealth = m_MaxHealth;
            
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

    public void TakeDamageFromPowerMeter()
    {
        if (PowerMeter.Get() != null)
        {
            int powerValue = PowerMeter.Get().GetPowerValue();
            TakeDamage(powerValue);
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
        
        Debug.Log("AI has died!");

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

        // Notify GameManager about AI death
        var gameManager = GameManager.Get();
        if (gameManager != null)
        {
            gameManager.OnAIDied();
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
    
    // Public method to force refresh AI health
    public void RefreshAIHealth()
    {
        InitializeHealthForLevel();
        UpdateHealthUI();
        Debug.Log($"AI Health manually refreshed: Current={m_CurrentHealth}, Max={m_MaxHealth}");
    }
}