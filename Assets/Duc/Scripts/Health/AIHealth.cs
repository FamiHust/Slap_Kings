using UnityEngine;

public class AIHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int m_MaxHealth = 100;
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
        m_CurrentHealth = m_MaxHealth;
        m_IsDead = false;
        m_StateMachine = GetComponent<AIStateMachine>();
    }

    protected virtual void Start()
    {
        if (m_HealthUI == null)
        {
            m_HealthUI = FindObjectOfType<HealthUI>();
        }
        
        if (m_HealthUI != null)
        {
            m_HealthUI.SetAIHealth(this);
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
}