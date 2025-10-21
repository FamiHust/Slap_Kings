using UnityEngine;
using System;
using System.Linq;

namespace Duc
{
    /// <summary>
    /// Interface for game managers
    /// </summary>
    public interface IGameManager
    {
        void Initialize();
        void Cleanup();
        bool IsInitialized { get; }
    }

    /// <summary>
    /// Interface for data managers
    /// </summary>
    public interface IDataManager
    {
        void LoadData();
        void SaveData();
        bool IsDataLoaded { get; }
    }

    /// <summary>
    /// Interface for persistent managers
    /// </summary>
    public interface IPersistentManager
    {
        void SavePersistentData();
        void LoadPersistentData();
        void ResetData();
    }

    /// <summary>
    /// Base class for all managers with common functionality
    /// </summary>
    public abstract class BaseManager : MonoBehaviour, IGameManager
    {
        [Header("Manager Settings")]
        [SerializeField] protected bool m_EnableDebugLogs = true;
        [SerializeField] protected bool m_AutoInitialize = true;
        
        public bool IsInitialized { get; protected set; } = false;
        
        protected virtual void Awake()
        {
            if (m_AutoInitialize)
            {
                Initialize();
            }
        }
        
        protected virtual void OnDestroy()
        {
            Cleanup();
        }
        
        public virtual void Initialize()
        {
            if (IsInitialized) return;
            
            OnInitialize();
            IsInitialized = true;
            
            if (m_EnableDebugLogs)
            {
                Debug.Log($"{GetType().Name} initialized successfully");
            }
        }
        
        public virtual void Cleanup()
        {
            if (!IsInitialized) return;
            
            OnCleanup();
            IsInitialized = false;
            
            if (m_EnableDebugLogs)
            {
                Debug.Log($"{GetType().Name} cleaned up");
            }
        }
        
        protected abstract void OnInitialize();
        protected abstract void OnCleanup();
    }

    /// <summary>
    /// Enhanced singleton manager with better error handling
    /// </summary>
    public abstract class EnhancedSingletonManager<T> : BaseManager where T : MonoBehaviour
    {
        private static T s_Instance;
        private static readonly object s_Lock = new object();
        private static bool s_IsApplicationQuitting = false;
        
        public static T Instance
        {
            get
            {
                if (s_IsApplicationQuitting)
                {
                    Debug.LogWarning($"Instance of {typeof(T)} was requested during application quit. Returning null.");
                    return null;
                }
                
                lock (s_Lock)
                {
                    if (s_Instance == null)
                    {
                        s_Instance = FindObjectOfType<T>();
                        
                        if (s_Instance == null)
                        {
                            GameObject singletonObject = new GameObject($"{typeof(T).Name}");
                            s_Instance = singletonObject.AddComponent<T>();
                            DontDestroyOnLoad(singletonObject);
                            
                            Debug.Log($"Created new singleton instance of {typeof(T).Name}");
                        }
                    }
                    
                    return s_Instance;
                }
            }
        }
        
        protected override void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Debug.LogWarning($"Multiple instances of {typeof(T).Name} detected. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }
            
            s_Instance = this as T;
            DontDestroyOnLoad(gameObject);
            
            base.Awake();
        }
        
        protected override void OnDestroy()
        {
            if (s_Instance == this)
            {
                s_Instance = null;
            }
            
            base.OnDestroy();
        }
        
        private void OnApplicationQuit()
        {
            s_IsApplicationQuitting = true;
        }

    }

    /// <summary>
    /// Static utility class for singleton management
    /// </summary>
    public static class SingletonCleanup
    {
        /// <summary>
        /// Cleanup all singleton instances
        /// Call this when application is quitting
        /// </summary>
        public static void CleanupAllSingletons()
        {
            Debug.Log("Cleaning up all singleton instances...");
            
            // Find and cleanup all singleton managers
            var allSingletons = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>()
                .Where(mb => mb is IGameManager)
                .Cast<IGameManager>()
                .ToArray();
                
            foreach (var singleton in allSingletons)
            {
                if (singleton is MonoBehaviour mb && mb != null)
                {
                    singleton.Cleanup();
                }
            }
        }
    }
}
