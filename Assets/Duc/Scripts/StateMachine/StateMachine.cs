using UnityEngine;
using System;

namespace Duc
{
    public abstract class StateMachine : MonoBehaviour
    {
        [Header("State Settings")]
        [SerializeField] protected CharacterState m_CurrentState = CharacterState.Idle;
        [SerializeField] protected bool m_EnableDebugLogs = true;
        
        [Header("State Events")]
        public Action<CharacterState> OnStateChanged;
        public Action<CharacterState> OnStateEntered;
        public Action<CharacterState> OnStateExited;
        
        protected CharacterState m_PreviousState;
        protected bool m_IsInitialized = false;
        protected ICharacterState m_CurrentStateInstance;

        protected virtual void Awake()
        {
            InitializeStateMachine();
        }

        protected virtual void Start()
        {
            EnterState(m_CurrentState);
            m_IsInitialized = true;
        }

        protected virtual void Update()
        {
            if (m_IsInitialized && m_CurrentStateInstance != null)
            {
                m_CurrentStateInstance.OnUpdate(this);
            }
        }

        protected virtual void InitializeStateMachine()
        {
            m_PreviousState = m_CurrentState;
            m_CurrentStateInstance = StateFactory.GetState(m_CurrentState);
        }

        public virtual void SetState(CharacterState newState)
        {
            if (m_CurrentState == newState) return;

            // Check if transition is allowed
            if (m_CurrentStateInstance != null && !m_CurrentStateInstance.CanTransitionTo(newState))
            {
                if (m_EnableDebugLogs)
                {
                    Debug.LogWarning($"Cannot transition from {m_CurrentState} to {newState}");
                }
                return;
            }

            // Check custom transition rules
            if (!CanChangeState(newState))
            {
                return;
            }

            ExitState(m_CurrentState);
            
            m_PreviousState = m_CurrentState;
            m_CurrentState = newState;
            m_CurrentStateInstance = StateFactory.GetState(m_CurrentState);
            
            EnterState(m_CurrentState);
            
            OnStateChanged?.Invoke(m_CurrentState);
        }

        protected virtual void EnterState(CharacterState state)
        {
            if (m_CurrentStateInstance != null)
            {
                m_CurrentStateInstance.OnEnter(this);
            }

            OnStateEntered?.Invoke(state);
        }

        protected virtual void ExitState(CharacterState state)
        {
            if (m_CurrentStateInstance != null)
            {
                m_CurrentStateInstance.OnExit(this);
            }

            OnStateExited?.Invoke(state);
        }

        public CharacterState GetCurrentState()
        {
            return m_CurrentState;
        }

        public CharacterState GetPreviousState()
        {
            return m_PreviousState;
        }

        public bool IsInState(CharacterState state)
        {
            return m_CurrentState == state;
        }

        protected virtual bool CanChangeState(CharacterState newState)
        {
            return true;
        }

        public virtual void ForceSetState(CharacterState newState)
        {
            if (m_CurrentState == newState) return;

            ExitState(m_CurrentState);
            m_PreviousState = m_CurrentState;
            m_CurrentState = newState;
            m_CurrentStateInstance = StateFactory.GetState(m_CurrentState);
            EnterState(m_CurrentState);
            OnStateChanged?.Invoke(m_CurrentState);
        }

        // Abstract methods for derived classes to implement specific behavior
        protected abstract void OnEnterIdle();
        protected abstract void OnExitIdle();
        protected abstract void OnEnterWaiting();
        protected abstract void OnExitWaiting();
        protected abstract void OnEnterHitted();
        protected abstract void OnExitHitted();
        protected abstract void OnEnterAttacking();
        protected abstract void OnExitAttacking();
        protected abstract void OnEnterDead();
        protected abstract void OnExitDead();
    }
}
