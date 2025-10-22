using UnityEngine;
using System;
using System.Collections.Generic;

namespace Duc
{
    public interface ICharacterState
    {
        CharacterState StateType { get; }
        void OnEnter(StateMachine stateMachine);
        void OnExit(StateMachine stateMachine);
        void OnUpdate(StateMachine stateMachine);
        bool CanTransitionTo(CharacterState targetState);
    }

    public abstract class BaseCharacterState : ICharacterState
    {
        public abstract CharacterState StateType { get; }

        public virtual void OnEnter(StateMachine stateMachine)
        {
            // Override in derived classes
        }

        public virtual void OnExit(StateMachine stateMachine)
        {
            // Override in derived classes
        }

        public virtual void OnUpdate(StateMachine stateMachine)
        {
            // Override in derived classes
        }

        public virtual bool CanTransitionTo(CharacterState targetState)
        {
            return true; 
        }
    }

    public static class StateFactory
    {
        private static Dictionary<CharacterState, ICharacterState> s_StateInstances = new Dictionary<CharacterState, ICharacterState>();

        public static ICharacterState GetState(CharacterState stateType)
        {
            if (!s_StateInstances.ContainsKey(stateType))
            {
                s_StateInstances[stateType] = CreateState(stateType);
            }
            return s_StateInstances[stateType];
        }

        private static ICharacterState CreateState(CharacterState stateType)
        {
            switch (stateType)
            {
                case CharacterState.Idle:
                    return new IdleState();
                case CharacterState.Waiting:
                    return new WaitingState();
                case CharacterState.Hitted:
                    return new HittedState();
                case CharacterState.Attacking:
                    return new AttackingState();
                case CharacterState.Dead:
                    return new DeadState();
                default:
                    throw new ArgumentException($"Unknown state type: {stateType}");
            }
        }
    }

    public class IdleState : BaseCharacterState
    {
        public override CharacterState StateType => CharacterState.Idle;

        public override void OnEnter(StateMachine stateMachine)
        {
            var method = typeof(StateMachine).GetMethod("OnEnterIdle", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(stateMachine, null);
        }

        public override void OnExit(StateMachine stateMachine)
        {
            var method = typeof(StateMachine).GetMethod("OnExitIdle", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(stateMachine, null);
        }
    }

    public class WaitingState : BaseCharacterState
    {
        public override CharacterState StateType => CharacterState.Waiting;

        public override void OnEnter(StateMachine stateMachine)
        {
            var method = typeof(StateMachine).GetMethod("OnEnterWaiting", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(stateMachine, null);
        }

        public override void OnExit(StateMachine stateMachine)
        {
            var method = typeof(StateMachine).GetMethod("OnExitWaiting", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(stateMachine, null);
        }
    }

    public class HittedState : BaseCharacterState
    {
        public override CharacterState StateType => CharacterState.Hitted;

        public override void OnEnter(StateMachine stateMachine)
        {
            var method = typeof(StateMachine).GetMethod("OnEnterHitted", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(stateMachine, null);
        }

        public override void OnExit(StateMachine stateMachine)
        {
            var method = typeof(StateMachine).GetMethod("OnExitHitted", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(stateMachine, null);
        }
    }

    public class AttackingState : BaseCharacterState
    {
        public override CharacterState StateType => CharacterState.Attacking;

        public override void OnEnter(StateMachine stateMachine)
        {
            var method = typeof(StateMachine).GetMethod("OnEnterAttacking", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(stateMachine, null);
        }

        public override void OnExit(StateMachine stateMachine)
        {
            var method = typeof(StateMachine).GetMethod("OnExitAttacking", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(stateMachine, null);
        }
    }

    public class DeadState : BaseCharacterState
    {
        public override CharacterState StateType => CharacterState.Dead;

        public override void OnEnter(StateMachine stateMachine)
        {
            var method = typeof(StateMachine).GetMethod("OnEnterDead", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(stateMachine, null);
        }

        public override void OnExit(StateMachine stateMachine)
        {
            var method = typeof(StateMachine).GetMethod("OnExitDead", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(stateMachine, null);
        }

        public override bool CanTransitionTo(CharacterState targetState)
        {
            return false; 
        }
    }
}
