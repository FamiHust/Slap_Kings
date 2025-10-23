using UnityEngine;
using System;
using System.Collections.Generic;

namespace Duc
{
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

        public void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            Type eventType = typeof(T);
            
            if (!s_EventHandlers.ContainsKey(eventType))
            {
                s_EventHandlers[eventType] = new List<Delegate>();
            }
            
            s_EventHandlers[eventType].Add(handler);
        }
        
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
                        
                    }
                }
            }
        }

        public void Clear()
        {
            s_EventHandlers.Clear();
        }
    }

    public interface IGameEvent
    {
        DateTime Timestamp { get; }
    }

    public abstract class GameEvent : IGameEvent
    {
        public DateTime Timestamp { get; private set; }
        
        protected GameEvent()
        {
            Timestamp = DateTime.Now;
        }
    }

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

    public class CameraShakeEvent : GameEvent
    {
        public float Intensity { get; }
        public float Duration { get; }
        public bool ShakeFOV { get; }
        
        public CameraShakeEvent(float intensity, float duration, bool shakeFOV = true)
        {
            Intensity = intensity;
            Duration = duration;
            ShakeFOV = shakeFOV;
        }
    }

    public class CameraFOVShakeEvent : GameEvent
    {
        public float FOVAmount { get; }
        public float Duration { get; }
        
        public CameraFOVShakeEvent(float fovAmount, float duration)
        {
            FOVAmount = fovAmount;
            Duration = duration;
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
