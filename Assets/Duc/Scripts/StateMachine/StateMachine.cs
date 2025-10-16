using UnityEngine;
using System;

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

    protected virtual void Awake()
    {
        InitializeStateMachine();
    }

    protected virtual void Start()
    {
        EnterState(m_CurrentState);
        m_IsInitialized = true;
    }

    protected virtual void InitializeStateMachine()
    {
        m_PreviousState = m_CurrentState;
    }

    public virtual void SetState(CharacterState newState)
    {
        if (m_CurrentState == newState) return;

        ExitState(m_CurrentState);
        
        m_PreviousState = m_CurrentState;
        m_CurrentState = newState;
        
        EnterState(m_CurrentState);
        
        OnStateChanged?.Invoke(m_CurrentState);
    }

    protected virtual void EnterState(CharacterState state)
    {
        switch (state)
        {
            case CharacterState.Idle:
                OnEnterIdle();
                break;
            case CharacterState.Waiting:
                OnEnterWaiting();
                break;
            case CharacterState.Hitted:
                OnEnterHitted();
                break;
            case CharacterState.Attacking:
                OnEnterAttacking();
                break;
            case CharacterState.Dead:
                OnEnterDead();
                break;
        }

        OnStateEntered?.Invoke(state);
    }

    protected virtual void ExitState(CharacterState state)
    {
        switch (state)
        {
            case CharacterState.Idle:
                OnExitIdle();
                break;
            case CharacterState.Waiting:
                OnExitWaiting();
                break;
            case CharacterState.Hitted:
                OnExitHitted();
                break;
            case CharacterState.Attacking:
                OnExitAttacking();
                break;
            case CharacterState.Dead:
                OnExitDead();
                break;
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
        EnterState(m_CurrentState);
        OnStateChanged?.Invoke(m_CurrentState);
    }

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
