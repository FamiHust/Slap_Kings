using UnityEngine;
using System.Collections;

public class PlayerStateMachine : StateMachine
{
    [Header("Player References")]
    [SerializeField] private PlayerHealth m_PlayerHealth;
    [SerializeField] private GameplayInput m_GameplayInput;
    [SerializeField] private Animator m_Animator;
    [SerializeField] private int m_SlapVariantCount = 20;
    private int m_HashIsWaiting;
    private int m_HashIsStartingSlap;
    private int m_HashIsSlapMega;
    private int m_HashIsSlapSpecial;
    private int m_HashSlapNumber;
    private int m_HashStartGettingSlapped;
    private int m_HashGetSlappedNumber;
    private bool m_LoggedAnimatorParams;
    
    [Header("Player Settings")]
    [SerializeField] private bool m_EnableControlsInIdle = true;
    [SerializeField] private bool m_EnableControlsInWaiting = false;
    [SerializeField] private bool m_EnableControlsInAttacking = false;
    [SerializeField] private bool m_EnableControlsInDead = false;

    protected override void Awake()
    {
        base.Awake();
        
        if (m_PlayerHealth == null)
            m_PlayerHealth = GetComponent<PlayerHealth>();
        
        if (m_GameplayInput == null)
            m_GameplayInput = GetComponent<GameplayInput>();
        
        if (m_Animator == null)
            m_Animator = GetComponent<Animator>();

        m_HashIsWaiting = Animator.StringToHash("IsWaiting");
        m_HashIsStartingSlap = Animator.StringToHash("IsStartingSlap");
        m_HashIsSlapMega = Animator.StringToHash("IsSlapMega");
        m_HashIsSlapSpecial = Animator.StringToHash("IsSlapSpecial");
        m_HashSlapNumber = Animator.StringToHash("SlapNumber");
        m_HashStartGettingSlapped = Animator.StringToHash("StartGettingSlapped");
        m_HashGetSlappedNumber = Animator.StringToHash("GetSlappedNumber");

        if (m_Animator != null)
        {
            bool found = false;
            foreach (var p in m_Animator.parameters)
            {
                if (p.nameHash == m_HashSlapNumber && p.type == AnimatorControllerParameterType.Float)
                {
                    found = true;
                    break;
                }
            }
            if (!found && m_EnableDebugLogs && !m_LoggedAnimatorParams)
            {
                m_LoggedAnimatorParams = true;
            }

            bool foundHit = false;
            foreach (var p in m_Animator.parameters)
            {
                if (p.nameHash == m_HashGetSlappedNumber && p.type == AnimatorControllerParameterType.Float)
                {
                    foundHit = true;
                    break;
                }
            }
            if (!foundHit && m_EnableDebugLogs)
            {
                Debug.LogWarning("Player Animator missing float parameter 'GetSlappedNumber'.");
            }
        }
    }

    protected override void Start()
    {
        base.Start();
        
        if (m_PlayerHealth != null)
        {
            
        }

        // Subscribe to victory event to play celebration animation
        var persistentGameManager = PersistentGameManager.Instance;
        if (persistentGameManager != null)
        {
            persistentGameManager.OnPlayerVictory += OnPlayerVictory;
        }
    }

    private void OnDestroy()
    {
        var persistentGameManager = PersistentGameManager.Instance;
        if (persistentGameManager != null)
        {
            persistentGameManager.OnPlayerVictory -= OnPlayerVictory;
        }
    }

    protected override bool CanChangeState(CharacterState newState)
    {
        if (m_CurrentState == CharacterState.Dead)
        {
            return false;
        }
        
        if (newState == CharacterState.Dead && m_PlayerHealth != null && m_PlayerHealth.IsDead())
        {
            return true; 
        }
        
        return true;
    }

    #region State Enter Methods

    protected override void OnEnterIdle()
    {
        SetPlayerControls(m_EnableControlsInIdle);
        
        PlayIdleAnimation();
    }

    protected override void OnEnterWaiting()
    {
        SetPlayerControls(m_EnableControlsInWaiting);
        
        PlayWaitingAnimation();
    }

    protected override void OnEnterHitted()
    {
        if (m_Animator != null)
        {
            m_Animator.SetBool(m_HashIsWaiting, false);
            m_Animator.SetBool(m_HashIsStartingSlap, false);
            m_Animator.SetBool(m_HashIsSlapMega, false);
            m_Animator.SetBool(m_HashIsSlapSpecial, false);

            int randomGetSlapped = Random.Range(0, 5); 
            m_Animator.SetFloat(m_HashGetSlappedNumber, (float)randomGetSlapped);
            m_Animator.SetBool(m_HashStartGettingSlapped, true);
        }
    }

    protected override void OnExitHitted()
    {
        if (m_Animator != null)
        {
            m_Animator.SetBool(m_HashStartGettingSlapped, false);
        }
    }
    
    public void MaintainWaitingAnimation()
    {
        if (m_Animator != null)
        {
            m_Animator.SetBool("IsWaiting", true);

            m_Animator.SetBool("IsStartingSlap", false);
            m_Animator.SetBool("IsSlapMega", false);
            m_Animator.SetBool("IsSlapSpecial", false);
        }
    }

    protected override void OnEnterAttacking()
    {
        SetPlayerControls(m_EnableControlsInAttacking);
        
        PlayAttackAnimation();
    }

    protected override void OnEnterDead()
    {
        SetPlayerControls(m_EnableControlsInDead);
        
        PlayDeathAnimation();
        
        // Ragdoll removed per request
        
        TriggerGameOver();
    }

    #endregion

    #region State Exit Methods

    protected override void OnExitIdle()
    {

    }

    protected override void OnExitWaiting()
    {

    }

    protected override void OnExitAttacking()
    {

    }

    protected override void OnExitDead()
    {

    }

    #endregion

    #region Player-specific Methods

    private void SetPlayerControls(bool enabled)
    {
        if (m_GameplayInput != null)
        {
            m_GameplayInput.enabled = enabled;
        }
    }

    private void PlayIdleAnimation()
    {
        if (m_Animator != null)
        {
            m_Animator.SetBool(m_HashIsWaiting, false);

            m_Animator.SetBool(m_HashIsStartingSlap, false);
            m_Animator.SetBool(m_HashIsSlapMega, false);
            m_Animator.SetBool(m_HashIsSlapSpecial, false);
        }
    }

    private void PlayWaitingAnimation()
    {
        if (m_Animator != null)
        {
            m_Animator.SetBool(m_HashIsWaiting, true);

            m_Animator.SetBool(m_HashIsStartingSlap, false);
            m_Animator.SetBool(m_HashIsSlapMega, false);
            m_Animator.SetBool(m_HashIsSlapSpecial, false);
        }
    }

    private void PlayAttackAnimation()
    {
        if (m_Animator != null)
        {
            m_Animator.SetBool(m_HashIsWaiting, false);
            
            var turnManager = FindObjectOfType<TurnManager>();
            if (turnManager != null && turnManager.IsPlayerTurn())
            {
                int power = PowerMeter.Get() != null ? PowerMeter.Get().GetPowerValue() : 0;
                int maxPower = PowerMeter.Get() != null ? PowerMeter.Get().GetMaxPower() : 100;
                bool isMega = power >= (maxPower / 2);

                m_Animator.SetBool(m_HashIsStartingSlap, false);
                m_Animator.SetBool(m_HashIsSlapMega, false);
                m_Animator.SetBool(m_HashIsSlapSpecial, false);

                if (isMega)
                {
                    int randomMega = Random.Range(0, 25);
                    m_Animator.SetFloat(m_HashSlapNumber, (float)randomMega);
                    m_Animator.SetBool(m_HashIsSlapMega, true);
                    m_Animator.SetBool(m_HashIsSlapSpecial, false);
                }
                else
                {
                    int randomNormal = Random.Range(0, 19);
                    m_Animator.SetFloat(m_HashSlapNumber, (float)randomNormal);
                    m_Animator.SetBool(m_HashIsSlapMega, false);
                    m_Animator.SetBool(m_HashIsSlapSpecial, false);
                }
                StartCoroutine(TriggerSlapAnimationDelayed());
            }
            else
            {
                m_Animator.SetBool(m_HashIsStartingSlap, false);
                m_Animator.SetBool(m_HashIsSlapMega, false);
                m_Animator.SetBool(m_HashIsSlapSpecial, false);
            }
        }
    }

    private void PlayDeathAnimation()
    {

    }

    private void TriggerGameOver()
    {

    }

    private void PlayVictoryAnimation()
    {
        if (m_Animator == null) return;

        // Reset combat flags
        m_Animator.SetBool(m_HashIsWaiting, false);
        m_Animator.SetBool(m_HashIsStartingSlap, false);
        m_Animator.SetBool(m_HashIsSlapMega, false);
        m_Animator.SetBool(m_HashIsSlapSpecial, false);

        // Try to find a victory boolean parameter
        string[] victoryBoolNames = { "IsVictory", "Victory", "IsWin", "StartVictory" };
        foreach (var name in victoryBoolNames)
        {
            foreach (var p in m_Animator.parameters)
            {
                if (p.type == AnimatorControllerParameterType.Bool && p.name == name)
                {
                    m_Animator.SetBool(p.nameHash, true);
                    break;
                }
            }
        }

        // Optional variant selection if available
        string[] victoryNumNames = { "VictoryNumber", "VictoryIndex" };
        foreach (var numName in victoryNumNames)
        {
            foreach (var p in m_Animator.parameters)
            {
                if ((p.type == AnimatorControllerParameterType.Int || p.type == AnimatorControllerParameterType.Float) && p.name == numName)
                {
                    int maxVariants = 12;
                    // Try get from PlayerStatsData if present
                    int configured = 12;
                    if (m_PlayerHealth != null)
                    {
                        var statsField = typeof(PlayerHealth).GetField("m_PlayerStatsData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        var stats = statsField != null ? statsField.GetValue(m_PlayerHealth) as PlayerStatsData : null;
                        if (stats != null)
                        {
                            configured = stats.animation.victoryVariantCount;
                        }
                    }
                    maxVariants = Mathf.Max(1, configured);
                    int pick = Random.Range(0, maxVariants);
                    if (p.type == AnimatorControllerParameterType.Int)
                        m_Animator.SetInteger(p.nameHash, pick);
                    else
                        m_Animator.SetFloat(p.nameHash, (float)pick);
                }
            }
        }
    }

    private void OnPlayerVictory()
    {
        // Force to Idle state then play victory anim to avoid conflicting flags
        SetState(CharacterState.Idle);
        PlayVictoryAnimation();
    }

    private IEnumerator TriggerSlapAnimationDelayed()
    {
        // Wait one frame to ensure SlapNumber is properly set
        yield return null;
        
        if (m_Animator != null)
        {
            // Double check it's still player's turn
            var turnManager = FindObjectOfType<TurnManager>();
            if (turnManager != null && turnManager.IsPlayerTurn())
            {
                m_Animator.SetBool(m_HashIsStartingSlap, true);
                
                if (m_EnableDebugLogs)
                {
                    Debug.Log($"Player - Triggered IsStartingSlap with SlapNumber: {m_Animator.GetFloat(m_HashSlapNumber)}");
                }
            }
        }
    }

    #endregion

    #region Public Methods

    public void TriggerDeath()
    {
        ForceSetState(CharacterState.Dead);
    }

    // Animation Event hook: call from the slap animation when the hit connects
    public void OnSlapHit()
    {
        var turnManager = FindObjectOfType<TurnManager>();
        if (turnManager != null)
        {
            turnManager.ApplyPlayerDamage();
        }
    }


    public void SetControlSettings(bool idle, bool waiting, bool attacking, bool dead)
    {
        m_EnableControlsInIdle = idle;
        m_EnableControlsInWaiting = waiting;
        m_EnableControlsInAttacking = attacking;
        m_EnableControlsInDead = dead;
    }

    #endregion
}
