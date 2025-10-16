using UnityEngine;

public abstract class HealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] protected int m_MaxHealth = 100;
    [SerializeField] protected int m_CurrentHealth;
    [SerializeField] protected bool m_IsDead = false;
    
    [Header("State Machine")]
    [SerializeField] protected StateMachine m_StateMachine;

    protected virtual void Awake()
    {
        m_CurrentHealth = m_MaxHealth;
        m_IsDead = false;
        m_StateMachine = GetComponent<StateMachine>();
    }

    protected virtual void Start()
    {

    }

    public virtual void TakeDamage(int damage)
    {
        if (m_IsDead || damage <= 0) return;

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
        
        HandleDeath();
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

    protected abstract void HandleDeath();
}
