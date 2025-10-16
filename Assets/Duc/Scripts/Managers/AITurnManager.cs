using UnityEngine;
using System.Collections;

public class AITurnManager : MonoBehaviour
{
    [Header("AI Turn Settings")]
    [SerializeField] private float m_WaitTimeBeforeAttack = 3f;
    [SerializeField] private float m_WaitTimeAfterAttack = 3f;
    [SerializeField] private int m_MinDamage = 10;
    [SerializeField] private int m_MaxDamage = 30;
    
    [Header("References")]
    [SerializeField] private CameraSwitcher m_CameraSwitcher;
    [SerializeField] private PlayerHealth m_PlayerHealth;
    [SerializeField] private AIHealth m_AIHealth;
    
    private Coroutine m_AITurnCoroutine;
    private bool m_IsAITurn = false;

    private void Start()
    {
        if (m_CameraSwitcher == null)
            m_CameraSwitcher = FindObjectOfType<CameraSwitcher>();
        
        if (m_PlayerHealth == null)
            m_PlayerHealth = FindObjectOfType<PlayerHealth>();
        
        if (m_AIHealth == null)
            m_AIHealth = FindObjectOfType<AIHealth>();
    }

    public void StartAITurn()
    {
        if (m_IsAITurn) return;
        
        m_IsAITurn = true;
        
        if (m_AITurnCoroutine != null)
        {
            StopCoroutine(m_AITurnCoroutine);
        }
        
        m_AITurnCoroutine = StartCoroutine(AITurnSequence());
    }

    public void StopAITurn()
    {
        m_IsAITurn = false;
        
        if (m_AITurnCoroutine != null)
        {
            StopCoroutine(m_AITurnCoroutine);
            m_AITurnCoroutine = null;
        }
    }

    private IEnumerator AITurnSequence()
    {
        
        yield return new WaitForSeconds(m_WaitTimeBeforeAttack);
        
        if (m_AIHealth != null && !m_AIHealth.IsDead() && m_IsAITurn)
        {
            AttackPlayer();
        }
        
        yield return new WaitForSeconds(m_WaitTimeAfterAttack);
        
        if (m_IsAITurn && m_CameraSwitcher != null)
        {
            m_CameraSwitcher.SwitchCamera();
            m_IsAITurn = false;
        }
        
        m_AITurnCoroutine = null;
    }

    private void AttackPlayer()
    {
        if (m_PlayerHealth == null || m_PlayerHealth.IsDead())
        {
            return;
        }
        
        int randomDamage = Random.Range(m_MinDamage, m_MaxDamage + 1);
        
        m_PlayerHealth.TakeDamage(randomDamage);
    }

    public bool IsAITurn()
    {
        return m_IsAITurn;
    }

    public void SetDamageRange(int minDamage, int maxDamage)
    {
        m_MinDamage = minDamage;
        m_MaxDamage = maxDamage;
    }

    public void SetWaitTimes(float waitBeforeAttack, float waitAfterAttack)
    {
        m_WaitTimeBeforeAttack = waitBeforeAttack;
        m_WaitTimeAfterAttack = waitAfterAttack;
    }
}
