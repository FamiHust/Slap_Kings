using UnityEngine;
using System;
using System.Collections.Generic;

namespace Duc
{
    /// <summary>
    /// Service Locator pattern for managing dependencies
    /// </summary>
    public class ServiceLocator : MonoBehaviour
    {
        private static ServiceLocator s_Instance;
        private Dictionary<Type, object> s_Services = new Dictionary<Type, object>();
        
        public static ServiceLocator Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    GameObject go = new GameObject("ServiceLocator");
                    s_Instance = go.AddComponent<ServiceLocator>();
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
        /// Register a service
        /// </summary>
        public void Register<T>(T service) where T : class
        {
            s_Services[typeof(T)] = service;
        }
        
        /// <summary>
        /// Get a service
        /// </summary>
        public T Get<T>() where T : class
        {
            if (s_Services.TryGetValue(typeof(T), out object service))
            {
                return service as T;
            }
            
            Debug.LogWarning($"Service of type {typeof(T)} not found");
            return null;
        }
        
        /// <summary>
        /// Check if a service is registered
        /// </summary>
        public bool IsRegistered<T>() where T : class
        {
            return s_Services.ContainsKey(typeof(T));
        }
        
        /// <summary>
        /// Unregister a service
        /// </summary>
        public void Unregister<T>() where T : class
        {
            s_Services.Remove(typeof(T));
        }
        
        /// <summary>
        /// Clear all services
        /// </summary>
        public void Clear()
        {
            s_Services.Clear();
        }
    }

    /// <summary>
    /// Extension methods for ServiceLocator
    /// </summary>
    public static class ServiceLocatorExtensions
    {
        /// <summary>
        /// Register a service with ServiceLocator
        /// </summary>
        public static void RegisterService<T>(this MonoBehaviour component, T service) where T : class
        {
            ServiceLocator.Instance.Register(service);
        }
        
        /// <summary>
        /// Get a service from ServiceLocator
        /// </summary>
        public static T GetService<T>(this MonoBehaviour component) where T : class
        {
            return ServiceLocator.Instance.Get<T>();
        }
    }
}
