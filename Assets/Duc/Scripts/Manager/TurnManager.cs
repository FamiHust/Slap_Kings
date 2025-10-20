using UnityEngine;
using System.Collections;

public class TurnManager : MonoBehaviour
{
    [Header("State Machines")]
    [SerializeField] private PlayerStateMachine m_PlayerStateMachine;
    [SerializeField] private AIStateMachine m_AIStateMachine;
    
    [Header("Turn Settings")]
    [SerializeField] private float m_AIWaitTime = 3f;
    [SerializeField] private float m_AIAttackTime = 2f;
    [SerializeField] private float m_AIGetSlappedDelay = 1.0f;
    [SerializeField] private bool m_EnableDebugLogs = true;
    [SerializeField] private int m_AIMinDamage = 10;
    [SerializeField] private int m_AIMaxDamage = 30;
    
    [Header("References")]
    [SerializeField] private CameraSwitcher m_CameraSwitcher;
    [SerializeField] private PowerMeter m_PowerMeter;
    
    private bool m_IsPlayerTurn = true;
    private bool m_IsGameOver = false;
    private Coroutine m_TurnCoroutine;
    private bool m_IsAITurnActive = false; 
    private bool m_NextTurnScheduled = false;

    private void Awake()
    {
        if (m_PlayerStateMachine == null)
            m_PlayerStateMachine = FindObjectOfType<PlayerStateMachine>();
        
        if (m_AIStateMachine == null)
            m_AIStateMachine = FindObjectOfType<AIStateMachine>();
        
        if (m_CameraSwitcher == null)
            m_CameraSwitcher = FindObjectOfType<CameraSwitcher>();
        
        if (m_PowerMeter == null)
            m_PowerMeter = FindObjectOfType<PowerMeter>();
        
    }

    private void Start()
    {
        // Game flow is started explicitly by GameManager.StartGame()
    }

    public void StartPlayerTurn()
    {
        if (m_IsGameOver) return;
        
        m_IsPlayerTurn = true;
        m_IsAITurnActive = false; 

        if (m_PlayerStateMachine != null)
        {
            m_PlayerStateMachine.SetState(CharacterState.Idle);
        }
        
        if (m_AIStateMachine != null)
        {
            m_AIStateMachine.SetState(CharacterState.Waiting);
        }
        
        if (m_CameraSwitcher != null)
        {
            m_CameraSwitcher.SwitchToPlayerCamera();
        }

        if (m_PowerMeter != null)
        {
            m_PowerMeter.gameObject.SetActive(true);
            m_PowerMeter.StartMeter();
        }
    }

    public void StartAITurn()
    {
        if (m_IsGameOver) return;
        
        m_IsPlayerTurn = false;
        m_IsAITurnActive = true; 
        
        if (m_PlayerStateMachine != null)
        {
            m_PlayerStateMachine.SetState(CharacterState.Waiting);
        }
        
        if (m_AIStateMachine != null)
        {
            m_AIStateMachine.SetState(CharacterState.Idle);
        }
        
        if (m_CameraSwitcher != null)
        {
            m_CameraSwitcher.SwitchToAICamera();
        }

        if (m_PowerMeter != null)
        {
            m_PowerMeter.StopMeter();
            m_PowerMeter.gameObject.SetActive(false);
        }
        
        if (m_TurnCoroutine != null)
        {
            StopCoroutine(m_TurnCoroutine);
        }
        m_TurnCoroutine = StartCoroutine(AITurnSequence());
    }

    private IEnumerator AITurnSequence()
    {
        yield return new WaitForSeconds(m_AIWaitTime);
        
        if (m_AIStateMachine != null)
        {
            m_AIStateMachine.SetState(CharacterState.Attacking);
        }
        
        yield return null;
        
        if (m_PlayerStateMachine != null)
        {
            m_PlayerStateMachine.MaintainWaitingAnimation();
        }
        
        yield return new WaitForSeconds(m_AIAttackTime);
        StartPlayerTurn();
    }

    public void PlayerAttacks()
    {
        if (m_IsGameOver || !m_IsPlayerTurn) return;

        if (m_PlayerStateMachine != null)
        {
            m_PlayerStateMachine.SetState(CharacterState.Attacking);
        }
        
        if (m_AIStateMachine != null)
        {
            m_AIStateMachine.MaintainWaitingAnimation();
        }
        // Do not auto-advance here; we'll advance after AI finishes get-slapped
    }

    // PlayerAttackSequence removed – next turn is scheduled after damage is applied

    public void CheckGameOver()
    {
        if (m_IsGameOver) return;
        
        bool playerDead = false;
        bool aiDead = false;
        
        if (m_PlayerStateMachine != null)
        {
            playerDead = m_PlayerStateMachine.IsInState(CharacterState.Dead);
        }
        
        if (m_AIStateMachine != null)
        {
            aiDead = m_AIStateMachine.IsInState(CharacterState.Dead);
        }
        
        if (playerDead || aiDead)
        {
            m_IsGameOver = true;

            if (m_TurnCoroutine != null)
            {
                StopCoroutine(m_TurnCoroutine);
                m_TurnCoroutine = null;
            }
        }
    }

    public bool IsPlayerTurn()
    {
        return m_IsPlayerTurn;
    }

    public bool IsGameOver()
    {
        return m_IsGameOver;
    }

    public void SetAITurnSettings(float waitTime, float attackTime)
    {
        m_AIWaitTime = waitTime;
        m_AIAttackTime = attackTime;
    }

    public void SetStateMachines(PlayerStateMachine playerStateMachine, AIStateMachine aiStateMachine)
    {
        m_PlayerStateMachine = playerStateMachine;
        m_AIStateMachine = aiStateMachine;
    }

    public bool IsAITurn()
    {
        return m_IsAITurnActive;
    }

    public void ApplyPlayerDamage()
    {
        if (m_IsGameOver)
        {
            return;
        }
        if (!m_IsPlayerTurn)
        {
            return;
        }

        AIHealth aiHealth = null;
        if (m_AIStateMachine != null)
        {
            aiHealth = m_AIStateMachine.GetComponent<AIHealth>()
                ?? m_AIStateMachine.GetComponentInChildren<AIHealth>()
                ?? m_AIStateMachine.GetComponentInParent<AIHealth>();
        }
        if (aiHealth == null)
        {
            aiHealth = FindObjectOfType<AIHealth>();
        }
        if (aiHealth == null)
        {
            if (m_EnableDebugLogs) Debug.LogWarning("ApplyPlayerDamage: AIHealth not found");
            return;
        }

        int damage = (m_PowerMeter != null) ? Mathf.Max(0, m_PowerMeter.GetPowerValue()) : Random.Range(10, 31);
        aiHealth.TakeDamage(damage);
        // Put AI in Hitted state if currently waiting
        if (m_AIStateMachine != null && m_AIStateMachine.IsInState(CharacterState.Waiting))
        {
            // Provide normalized power to AI for get-slapped selection
            float normalizedPower = 0f;
            if (m_PowerMeter != null)
            {
                int max = Mathf.Max(1, m_PowerMeter.GetMaxPower());
                normalizedPower = Mathf.Clamp01((float)m_PowerMeter.GetPowerValue() / (float)max);
            }
            m_AIStateMachine.PrepareGetSlapped(normalizedPower);
            m_AIStateMachine.SetState(CharacterState.Hitted);

            // Schedule next turn after a short delay to allow get-slapped to play
            if (!m_NextTurnScheduled)
            {
                m_NextTurnScheduled = true;
                if (m_TurnCoroutine != null)
                {
                    StopCoroutine(m_TurnCoroutine);
                }
                m_TurnCoroutine = StartCoroutine(StartAITurnAfterDelay(m_AIGetSlappedDelay));
            }
        }
    }

    private IEnumerator StartAITurnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        m_NextTurnScheduled = false;
        StartAITurn();
    }

    public void ApplyAIDamage()
    {
        if (m_IsGameOver)
        {
            if (m_EnableDebugLogs) Debug.Log("ApplyAIDamage blocked: game over");
            return;
        }
        if (m_IsPlayerTurn)
        {
            return;
        }

        PlayerHealth playerHealth = null;
        if (m_PlayerStateMachine != null)
        {
            playerHealth = m_PlayerStateMachine.GetComponent<PlayerHealth>()
                ?? m_PlayerStateMachine.GetComponentInChildren<PlayerHealth>()
                ?? m_PlayerStateMachine.GetComponentInParent<PlayerHealth>();
        }
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
        }
        if (playerHealth == null)
        {
            return;
        }

        // Get scaled damage based on current level
        int damage = GetScaledAIDamage();
        playerHealth.TakeDamage(damage);
        
        if (m_EnableDebugLogs) Debug.Log($"AI deals {damage} damage to player");
        
        // Put Player in Hitted state if currently waiting
        if (m_PlayerStateMachine != null && m_PlayerStateMachine.IsInState(CharacterState.Waiting))
        {
            m_PlayerStateMachine.SetState(CharacterState.Hitted);
        }
    }

    private int GetScaledAIDamage()
    {
        var persistentData = PersistentDataManager.Instance;
        
        if (persistentData != null)
        {
            // Use current AI damage from persistent data (only increases on victory)
            int minDmg = persistentData.GetCurrentAIMinDamage();
            int maxDmg = persistentData.GetCurrentAIMaxDamage();
            return Random.Range(minDmg, maxDmg + 1);
        }
        else
        {
            // Fallback to original logic
            int minDmg = Mathf.Min(m_AIMinDamage, m_AIMaxDamage);
            int maxDmg = Mathf.Max(m_AIMinDamage, m_AIMaxDamage);
            return Random.Range(minDmg, maxDmg + 1);
        }
    }

    public void StopAllTurns()
    {
        m_IsGameOver = true;
        
        // Stop any running coroutines
        if (m_TurnCoroutine != null)
        {
            StopCoroutine(m_TurnCoroutine);
            m_TurnCoroutine = null;
        }
        
        
        // Stop power meter
        if (m_PowerMeter != null)
        {
            m_PowerMeter.StopMeter();
        }
        
        Debug.Log("All turns stopped - Game Over");
    }

    public void ResetGame()
    {
        m_IsGameOver = false;
        m_IsPlayerTurn = true;
        m_IsAITurnActive = false;
        m_NextTurnScheduled = false;
        
        // Stop any running coroutines
        if (m_TurnCoroutine != null)
        {
            StopCoroutine(m_TurnCoroutine);
            m_TurnCoroutine = null;
        }
        
        // Reset state machines
        if (m_PlayerStateMachine != null)
        {
            m_PlayerStateMachine.SetState(CharacterState.Idle);
        }
        
        if (m_AIStateMachine != null)
        {
            m_AIStateMachine.SetState(CharacterState.Idle);
        }
        
        // Reset power meter
        if (m_PowerMeter != null)
        {
            m_PowerMeter.StopMeter();
        }
        
        // Reset camera
        if (m_CameraSwitcher != null)
        {
            m_CameraSwitcher.SwitchToPlayerCamera();
        }
        
        Debug.Log("Game reset - Ready to start");
    }
}
