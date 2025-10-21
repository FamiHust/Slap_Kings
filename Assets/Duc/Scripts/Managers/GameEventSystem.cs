using UnityEngine;
using System;
using System.Collections.Generic;

namespace Duc
{
    /// <summary>
    /// Centralized event system for loose coupling
    /// </summary>
    public class GameEventSystem : MonoBehaviour
    {
        private static GameEventSystem s_Instance;
        private Dictionary<Type, List<Delegate>> s_EventHandlers = new Dictionary<Type, List<Delegate>>();
        
        public static GameEventSystem Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    GameObject go = new GameObject("GameEventSystem");
                    s_Instance = go.AddComponent<GameEventSystem>();
                    DontDestroyOnLoad(go);
                }
                return s_Instance;
            }
        }
        
        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (s_Instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Subscribe to an event
        /// </summary>
        public void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            Type eventType = typeof(T);
            
            if (!s_EventHandlers.ContainsKey(eventType))
            {
                s_EventHandlers[eventType] = new List<Delegate>();
            }
            
            s_EventHandlers[eventType].Add(handler);
        }
        
        /// <summary>
        /// Unsubscribe from an event
        /// </summary>
        public void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
        {
            Type eventType = typeof(T);
            
            if (s_EventHandlers.ContainsKey(eventType))
            {
                s_EventHandlers[eventType].Remove(handler);
                
                if (s_EventHandlers[eventType].Count == 0)
                {
                    s_EventHandlers.Remove(eventType);
                }
            }
        }
        
        /// <summary>
        /// Publish an event
        /// </summary>
        public void Publish<T>(T gameEvent) where T : IGameEvent
        {
            Type eventType = typeof(T);
            
            if (s_EventHandlers.ContainsKey(eventType))
            {
                foreach (var handler in s_EventHandlers[eventType])
                {
                    try
                    {
                        (handler as Action<T>)?.Invoke(gameEvent);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error handling event {eventType.Name}: {e.Message}");
                    }
                }
            }
        }
        
        /// <summary>
        /// Clear all event handlers
        /// </summary>
        public void Clear()
        {
            s_EventHandlers.Clear();
        }
    }

    /// <summary>
    /// Interface for game events
    /// </summary>
    public interface IGameEvent
    {
        DateTime Timestamp { get; }
    }

    /// <summary>
    /// Base class for game events
    /// </summary>
    public abstract class GameEvent : IGameEvent
    {
        public DateTime Timestamp { get; private set; }
        
        protected GameEvent()
        {
            Timestamp = DateTime.Now;
        }
    }

    /// <summary>
    /// Specific game events
    /// </summary>
    public class PlayerHealthChangedEvent : GameEvent
    {
        public int CurrentHealth { get; }
        public int MaxHealth { get; }
        public int DamageAmount { get; }
        
        public PlayerHealthChangedEvent(int currentHealth, int maxHealth, int damageAmount = 0)
        {
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            DamageAmount = damageAmount;
        }
    }

    public class AIHealthChangedEvent : GameEvent
    {
        public int CurrentHealth { get; }
        public int MaxHealth { get; }
        public int DamageAmount { get; }
        
        public AIHealthChangedEvent(int currentHealth, int maxHealth, int damageAmount = 0)
        {
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            DamageAmount = damageAmount;
        }
    }

    public class PlayerDeathEvent : GameEvent
    {
        public int LastDamage { get; }
        
        public PlayerDeathEvent(int lastDamage)
        {
            LastDamage = lastDamage;
        }
    }

    public class AIDeathEvent : GameEvent
    {
        public int LastDamage { get; }
        
        public AIDeathEvent(int lastDamage)
        {
            LastDamage = lastDamage;
        }
    }

    public class TurnChangedEvent : GameEvent
    {
        public bool IsPlayerTurn { get; }
        
        public TurnChangedEvent(bool isPlayerTurn)
        {
            IsPlayerTurn = isPlayerTurn;
        }
    }

    public class GameStateChangedEvent : GameEvent
    {
        public GameState NewState { get; }
        public GameState PreviousState { get; }
        
        public GameStateChangedEvent(GameState newState, GameState previousState)
        {
            NewState = newState;
            PreviousState = previousState;
        }
    }

    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver,
        Victory
    }

    /// <summary>
    /// Extension methods for easier event handling
    /// </summary>
    public static class GameEventExtensions
    {
        public static void SubscribeToEvent<T>(this MonoBehaviour component, Action<T> handler) where T : IGameEvent
        {
            GameEventSystem.Instance.Subscribe(handler);
        }
        
        public static void UnsubscribeFromEvent<T>(this MonoBehaviour component, Action<T> handler) where T : IGameEvent
        {
            GameEventSystem.Instance.Unsubscribe(handler);
        }
        
        public static void PublishEvent<T>(this MonoBehaviour component, T gameEvent) where T : IGameEvent
        {
            GameEventSystem.Instance.Publish(gameEvent);
        }
    }
}
