using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerController : MonoBehaviour
{
    [SerializeField] protected int m_CurrentHealth = 100;

    [SerializeField] protected Animator m_Anim;

    public int CurrentHealth => m_CurrentHealth;    

    protected void Awake()
    {
        m_Anim = GetComponent<Animator>();
    }

    protected virtual void PerformAttackAnimation() 
    {
        
    }

    protected virtual void RunDeadEffect() { }

    protected virtual void Die()
    {
        RunDeadEffect();
    }

    public virtual void TakeDamage(int damage)
    {
        m_CurrentHealth -= damage;

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }
}
