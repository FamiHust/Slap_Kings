using UnityEngine;
using System.Collections;

namespace Duc
{
    public class AIStateMachine : StateMachine
    {
        [Header("AI References")]
        [SerializeField] private AIHealth m_AIHealth;
        [SerializeField] private Collider m_AICollider;
        [SerializeField] private Animator m_Animator;
        [SerializeField] private int m_SlapVariantCount = 20;
        private int m_HashStartGettingSlapped;
        private int m_HashGetSlappedNumber;
        private int m_HashSlapPower;
        
        [Header("Counter System")]
        [SerializeField] private CounterSystem m_CounterSystem;
        [SerializeField] private float m_CounterStartDelay = 0.5f; // Start counter 0.5s before AI attack
        
        [Header("AI Settings")]
        [SerializeField] private bool m_EnableAIInIdle = true;
        [SerializeField] private bool m_EnableAIInWaiting = false;
        [SerializeField] private bool m_EnableAIInAttacking = false;
        [SerializeField] private bool m_EnableAIInDead = false;
        [SerializeField] private float m_AttackDuration = 2f;
        
        private Coroutine m_AttackCoroutine;
        private int m_HashIsWaiting;
        private int m_HashIsStartingSlap;
        private int m_HashIsSlapMega;
        private int m_HashIsSlapSpecial;
        private int m_HashSlapNumber;
        private bool m_LoggedAnimatorParams;
        private float m_CurrentSlapPower = 0f; // normalized 0..1

        protected override void Awake()
        {
            base.Awake();
            
            if (m_AIHealth == null)
                m_AIHealth = GetComponent<AIHealth>();
            
            if (m_AICollider == null)
                m_AICollider = GetComponent<Collider>();
            
            if (m_Animator == null)
                m_Animator = GetComponent<Animator>();

            // Find CounterSystem if not assigned
            if (m_CounterSystem == null)
                m_CounterSystem = CounterSystem.Instance;

            
            m_HashStartGettingSlapped = Animator.StringToHash("StartGettingSlapped");
            m_HashGetSlappedNumber = Animator.StringToHash("GetSlappedNumber");
            m_HashSlapPower = Animator.StringToHash("SlapPower");

            // Cache animator parameter hashes
            m_HashIsWaiting = Animator.StringToHash("IsWaiting");
            m_HashIsStartingSlap = Animator.StringToHash("IsStartingSlap");
            m_HashIsSlapMega = Animator.StringToHash("IsSlapMega");
            m_HashIsSlapSpecial = Animator.StringToHash("IsSlapSpecial");
            m_HashSlapNumber = Animator.StringToHash("SlapNumber");

            // Validate parameter exists
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
                    Debug.LogWarning($"AI Animator missing int parameter 'SlapNumber'. Available: " + string.Join(", ", System.Array.ConvertAll(m_Animator.parameters, pr => pr.name + ":" + pr.type)));
                }
            }
        }

        protected override void Start()
        {
            base.Start();
            
            // Subscribe to health events
            if (m_AIHealth != null)
            {
                // We'll add health event subscription when integrating with HealthManager
            }
        }

        protected override bool CanChangeState(CharacterState newState)
        {
            if (m_CurrentState == CharacterState.Dead)
            {
                return false;
            }
            
            if (newState == CharacterState.Dead && m_AIHealth != null && m_AIHealth.IsDead())
            {
                return true; 
            }
            
            return true;
        }

        #region State Enter Methods

        protected override void OnEnterIdle()
        {
            if (m_EnableDebugLogs)
            {
                Debug.Log("AI entering Idle state");
            }

            SetAIComponents(m_EnableAIInIdle);
            PlayIdleAnimation();
        }

        protected override void OnEnterWaiting()
        {
            SetAIComponents(m_EnableAIInWaiting);
            
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

                // Choose normal (0..4) or mega (0..7) get-slapped based on SlapPower
                float power = m_CurrentSlapPower;
                bool isMegaGetSlapped = power >= 0.85f;
                int variant = isMegaGetSlapped ? Random.Range(0, 7) : Random.Range(0, 5);
                m_Animator.SetFloat(m_HashGetSlappedNumber, (float)variant);
                m_Animator.SetFloat(m_HashSlapPower, power);
                m_Animator.SetBool(m_HashStartGettingSlapped, true);
            }
        }

        protected override void OnExitHitted()
        {
            if (m_Animator != null)
            {
                m_Animator.SetBool(m_HashStartGettingSlapped, false);
                m_Animator.SetFloat(m_HashSlapPower, 0f);
            }
        }

        public void MaintainWaitingAnimation()
        {
            if (m_Animator != null)
            {
                m_Animator.SetBool("IsWaiting", true);

                // Reset slap animations to ensure AI doesn't show attack animation
                m_Animator.SetBool("IsStartingSlap", false);
                m_Animator.SetBool("IsSlapMega", false);
                m_Animator.SetBool("IsSlapSpecial", false);
            }
        }

        protected override void OnEnterAttacking()
        {
            SetAIComponents(m_EnableAIInAttacking);
            
            PlayAttackAnimation();
            
            StartAttackSequence();
        }

        protected override void OnEnterDead()
        {
            SetAIComponents(m_EnableAIInDead);
            
            PlayDeathAnimation();
            
            // Ragdoll removed per request
            
            StartDeathSequence();
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
            StopAttackSequence();
        }

        protected override void OnExitDead()
        {
            
        }

        #endregion

        #region AI-specific Methods

        private void SetAIComponents(bool enabled)
        {
            if (m_AICollider != null)
            {
                m_AICollider.enabled = enabled;
            }
        }

        private void PlayIdleAnimation()
        {
            if (m_Animator != null)
            {
                m_Animator.SetBool(m_HashIsWaiting, false);

                // Reset slap animations
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
                // Reset slap animations
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
                if (turnManager != null && turnManager.IsAITurn())
                {
                    // Randomize slap number for blend tree (0-19)
                    int randomSlapNumber = Random.Range(0, 20);
                    
                    if (m_EnableDebugLogs)
                    {
                        Debug.Log($"AI Attack - Random SlapNumber: {randomSlapNumber}");
                    }

                    // Reset all animations first
                    m_Animator.SetBool(m_HashIsStartingSlap, false);
                    m_Animator.SetBool(m_HashIsSlapMega, false);
                    m_Animator.SetBool(m_HashIsSlapSpecial, false);

                    // Set SlapNumber first (float parameter)
                    m_Animator.SetFloat(m_HashSlapNumber, (float)randomSlapNumber);
                    
                    // Wait one frame then trigger animation
                    StartCoroutine(TriggerSlapAnimationDelayed());
                }
                else
                {
                    // Reset slap animations when not AI's turn
                    m_Animator.SetBool(m_HashIsStartingSlap, false);
                    m_Animator.SetBool(m_HashIsSlapMega, false);
                    m_Animator.SetBool(m_HashIsSlapSpecial, false);
                }
            }
        }

        private void PlayDeathAnimation()
        {
            
        }

        private void StartAttackSequence()
        {
            if (m_AttackCoroutine != null)
            {
                StopCoroutine(m_AttackCoroutine);
            }
            
            // Start counter system before AI attack
            if (m_CounterSystem != null)
            {
                StartCoroutine(StartCounterBeforeAttack());
            }
            else
            {
                m_AttackCoroutine = StartCoroutine(AttackSequenceCoroutine());
            }
        }

        private IEnumerator StartCounterBeforeAttack()
        {
            // Subscribe to counter events (counter already started by TurnManager)
            if (m_CounterSystem != null)
            {
                m_CounterSystem.OnCounterAttempted += OnCounterAttempted;
                m_CounterSystem.OnCounterEnded += OnCounterEnded;
            }
            
            // Wait for counter start delay before attack
            yield return new WaitForSeconds(m_CounterStartDelay);
            
            // Start attack sequence
            m_AttackCoroutine = StartCoroutine(AttackSequenceCoroutine());
        }

        private void OnCounterAttempted(float counterValue)
        {
            Debug.Log($"Player attempted counter with value: {counterValue:F2}");
            
            // Store counter value for damage calculation
            m_CurrentSlapPower = counterValue;
        }

        private void OnCounterEnded()
        {
            // Unsubscribe from events
            if (m_CounterSystem != null)
            {
                m_CounterSystem.OnCounterAttempted -= OnCounterAttempted;
                m_CounterSystem.OnCounterEnded -= OnCounterEnded;
            }
        }

        private void StopAttackSequence()
        {
            if (m_AttackCoroutine != null)
            {
                StopCoroutine(m_AttackCoroutine);
                m_AttackCoroutine = null;
            }
        }

        private void StartDeathSequence()
        {
            StartCoroutine(DeathSequenceCoroutine());
        }
    
        private IEnumerator AttackSequenceCoroutine()
        {
            yield return new WaitForSeconds(m_AttackDuration);
            
            SetState(CharacterState.Idle);
        }

        private IEnumerator DeathSequenceCoroutine()
        {
            yield return new WaitForSeconds(2f);
            
            Destroy(gameObject, 1f);
        }

        private IEnumerator TriggerSlapAnimationDelayed()
        {
            // Wait one frame to ensure SlapNumber is properly set
            yield return null;
            
            if (m_Animator != null)
            {
                // Double check it's still AI's turn
                var turnManager = FindObjectOfType<TurnManager>();
                if (turnManager != null && turnManager.IsAITurn())
                {
                    m_Animator.SetBool(m_HashIsStartingSlap, true);
                    
                    if (m_EnableDebugLogs)
                    {
                        Debug.Log($"AI - Triggered IsStartingSlap with SlapNumber: {m_Animator.GetInteger(m_HashSlapNumber)}");
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
                turnManager.ApplyAIDamage();
            }
        }

        public void SetAISettings(bool idle, bool waiting, bool attacking, bool dead)
        {
            m_EnableAIInIdle = idle;
            m_EnableAIInWaiting = waiting;
            m_EnableAIInAttacking = attacking;
            m_EnableAIInDead = dead;
        }

        public void SetAttackDuration(float duration)
        {
            m_AttackDuration = duration;
        }

        // Called by TurnManager right before setting AI to Hitted, to feed power
        public void PrepareGetSlapped(float normalizedPower)
        {
            m_CurrentSlapPower = Mathf.Clamp01(normalizedPower);
        }

        #endregion
    }
}
