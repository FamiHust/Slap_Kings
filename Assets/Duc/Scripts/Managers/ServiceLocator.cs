using UnityEngine;
using System;
using System.Collections.Generic;

namespace Duc
{
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

        public void Register<T>(T service) where T : class
        {
            s_Services[typeof(T)] = service;
        }

        public T Get<T>() where T : class
        {
            if (s_Services.TryGetValue(typeof(T), out object service))
            {
                return service as T;
            }
            
            return null;
        }

        public bool IsRegistered<T>() where T : class
        {
            return s_Services.ContainsKey(typeof(T));
        }

        public void Unregister<T>() where T : class
        {
            s_Services.Remove(typeof(T));
        }

        public void Clear()
        {
            s_Services.Clear();
        }
    }

    public static class ServiceLocatorExtensions
    {
        public static void RegisterService<T>(this MonoBehaviour component, T service) where T : class
        {
            ServiceLocator.Instance.Register(service);
        }

        public static T GetService<T>(this MonoBehaviour component) where T : class
        {
            return ServiceLocator.Instance.Get<T>();
        }
    }
}
