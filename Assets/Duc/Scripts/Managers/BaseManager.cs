using UnityEngine;
using System;
using System.Linq;

namespace Duc
{
    public interface IGameManager
    {
        void Initialize();
        void Cleanup();
        bool IsInitialized { get; }
    }

    public interface IDataManager
    {
        void LoadData();
        void SaveData();
        bool IsDataLoaded { get; }
    }

    public interface IPersistentManager
    {
        void SavePersistentData();
        void LoadPersistentData();
        void ResetData();
    }

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
        }
        
        public virtual void Cleanup()
        {
            if (!IsInitialized) return;
            
            OnCleanup();
            IsInitialized = false;
        }
        
        protected abstract void OnInitialize();
        protected abstract void OnCleanup();
    }

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

    public static class SingletonCleanup
    {
        public static void CleanupAllSingletons()
        {
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
