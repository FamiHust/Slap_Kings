using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Duc
{
    public class AIAppearanceManager : MonoBehaviour
    {
        [Header("Appearance Configuration")]
        [SerializeField] private AIAppearanceData m_AppearanceData;
        
        [Header("Skinned Mesh Renderer References")]
        [SerializeField] private SkinnedMeshRenderer m_HeadRenderer;
        [SerializeField] private SkinnedMeshRenderer m_BodyRenderer;
        
        [Header("Settings")]
        [SerializeField] private bool m_EnableDebugLogs = true;
        [SerializeField] private bool m_AutoUpdateOnStart = true;
        [SerializeField] private float m_HealthThreshold = 0.5f; // 50% health threshold
        
        private AIAppearanceData.AppearanceSet m_CurrentAppearance;
        private AIHealth m_AIHealth;
        private bool m_IsUsingSlappedMesh = false;
        
        public System.Action<AIAppearanceData.AppearanceSet> OnAppearanceChanged;
        public System.Action<int> OnLevelChanged;
        
        public AIAppearanceData AppearanceData => m_AppearanceData;
        public AIAppearanceData.AppearanceSet CurrentAppearance => m_CurrentAppearance;
        
        private void Awake()
        {
            if (m_HeadRenderer == null)
            {
                m_HeadRenderer = FindSkinnedMeshRenderer("Head");
            }
            
            if (m_BodyRenderer == null)
            {
                m_BodyRenderer = FindSkinnedMeshRenderer("Body");
            }
            
            // Find AIHealth component - try multiple ways
            m_AIHealth = GetComponent<AIHealth>();
            if (m_AIHealth != null)
            {
                Debug.Log("AIHealth found on same GameObject");
            }
            else
            {
                m_AIHealth = GetComponentInChildren<AIHealth>();
                if (m_AIHealth != null)
                {
                    Debug.Log("AIHealth found in children");
                }
                else
                {
                    m_AIHealth = GetComponentInParent<AIHealth>();
                    if (m_AIHealth != null)
                    {
                        Debug.Log("AIHealth found in parent");
                    }
                    else
                    {
                        // Try to find in the scene
                        m_AIHealth = FindObjectOfType<AIHealth>();
                        if (m_AIHealth != null)
                        {
                            Debug.Log("AIHealth found in scene");
                        }
                        else
                        {
                            Debug.LogWarning("AIHealth not found anywhere!");
                        }
                    }
                }
            }
        }
        
        private void Start()
        {
            Debug.Log("AIAppearanceManager Start() called");
            Debug.Log($"m_AIHealth: {m_AIHealth != null}");
            Debug.Log($"m_HeadRenderer: {m_HeadRenderer != null}");
            Debug.Log($"m_AppearanceData: {m_AppearanceData != null}");
            
            if (m_AutoUpdateOnStart)
            {
                UpdateAppearanceForCurrentLevel();
            }
            
            // Subscribe to health changes in Start to ensure AIHealth is ready
            if (m_AIHealth != null)
            {
                m_AIHealth.OnHealthChanged += OnAIHealthChanged;
                Debug.Log("AIAppearanceManager: Subscribed to AIHealth.OnHealthChanged");
            }
            else
            {
                Debug.LogWarning("AIAppearanceManager: m_AIHealth is null in Start!");
            }
        }
        
        private SkinnedMeshRenderer FindSkinnedMeshRenderer(string name)
        {
            SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            
            foreach (var renderer in renderers)
            {
                if (renderer.name.ToLower().Contains(name.ToLower()))
                {
                    return renderer;
                }
            }
            
            if (renderers.Length > 0)
            {
                return renderers[0];
            }
            
            return null;
        }
        
        public void SetAppearanceData(AIAppearanceData appearanceData)
        {
            if (m_AppearanceData != appearanceData)
            {
                m_AppearanceData = appearanceData;
                UpdateAppearanceForCurrentLevel();
            }
        }
        
        public void UpdateAppearanceForCurrentLevel()
        {
            if (m_AppearanceData == null)
            {
                return;
            }
            
            int currentLevel = GetCurrentLevelFromPersistentData();
            AIAppearanceData.AppearanceSet newAppearance = m_AppearanceData.GetAppearanceForLevel(currentLevel);
            
            if (newAppearance == null)
            {
                return;
            }
            
            if (m_CurrentAppearance == newAppearance)
            {
                return;
            }
            
            TransitionToAppearance(newAppearance);
        }
        
        private void TransitionToAppearance(AIAppearanceData.AppearanceSet newAppearance)
        {
            ApplyAppearance(newAppearance);
            
            m_CurrentAppearance = newAppearance;
            OnAppearanceChanged?.Invoke(newAppearance);
        }
        
        private void ApplyAppearance(AIAppearanceData.AppearanceSet appearance)
        {
            Debug.Log($"ApplyAppearance called. m_HeadRenderer: {m_HeadRenderer != null}");
            
            if (m_HeadRenderer != null)
            {
                bool useSlappedMesh = ShouldUseSlappedMesh();
                Debug.Log($"useSlappedMesh: {useSlappedMesh}, headSlappedMesh: {appearance.headSlappedMesh != null}");
                
                if (useSlappedMesh && appearance.headSlappedMesh != null)
                {
                    Debug.Log("Applying slapped mesh...");
                    m_HeadRenderer.sharedMesh = appearance.headSlappedMesh;
                    if (appearance.headSlappedMaterial != null)
                    {
                        m_HeadRenderer.material = appearance.headSlappedMaterial;
                    }
                    m_IsUsingSlappedMesh = true;
                    Debug.Log("Slapped mesh applied successfully");
                }
                else
                {
                    Debug.Log("Applying normal mesh...");
                    if (appearance.headMesh != null)
                    {
                        m_HeadRenderer.sharedMesh = appearance.headMesh;
                    }
                    if (appearance.headMaterial != null)
                    {
                        m_HeadRenderer.material = appearance.headMaterial;
                    }
                    m_IsUsingSlappedMesh = false;
                    Debug.Log("Normal mesh applied successfully");
                }
            }
            else
            {
                Debug.LogWarning("m_HeadRenderer is null!");
            }
            
            if (m_BodyRenderer != null)
            {
                if (appearance.bodyMesh != null)
                {
                    m_BodyRenderer.sharedMesh = appearance.bodyMesh;
                }
                if (appearance.bodyMaterial != null)
                {
                    m_BodyRenderer.material = appearance.bodyMaterial;
                }
            }
        }
        
        private int GetCurrentLevelFromPersistentData()
        {
            var persistentDataManager = PersistentDataManager.Instance;
            if (persistentDataManager != null)
            {
                return persistentDataManager.GetLevelCount();
            }
            return 1; 
        }
        
        public void ForceUpdateAppearance()
        {
            UpdateAppearanceForCurrentLevel();
        }
        
        public bool IsAppearanceActive(AIAppearanceData.AppearanceSet appearance)
        {
            return m_CurrentAppearance == appearance;
        }
        
        public void SetHeadRenderer(SkinnedMeshRenderer renderer)
        {
            m_HeadRenderer = renderer;
        }
        
        public void SetBodyRenderer(SkinnedMeshRenderer renderer)
        {
            m_BodyRenderer = renderer;
        }
        
        [ContextMenu("Test Appearance Update")]
        public void TestAppearanceUpdate()
        {
            UpdateAppearanceForCurrentLevel();
        }
        
        [ContextMenu("Test Health Check")]
        public void TestHealthCheck()
        {
            if (m_AIHealth != null)
            {
                float healthPercentage = m_AIHealth.GetHealthPercentage();
                bool shouldUseSlapped = ShouldUseSlappedMesh();
                Debug.Log($"Test Health Check - Health: {healthPercentage:P1}, Should use slapped: {shouldUseSlapped}, Current using slapped: {m_IsUsingSlappedMesh}");
            }
            else
            {
                Debug.LogWarning("AIHealth not found for test");
            }
        }
        
        [ContextMenu("Force Health Check")]
        public void ForceHealthCheck()
        {
            if (m_AIHealth != null)
            {
                int currentHealth = m_AIHealth.GetCurrentHealth();
                int maxHealth = m_AIHealth.GetMaxHealth();
                OnAIHealthChanged(currentHealth, maxHealth);
            }
        }
        
        [ContextMenu("Test Slapped Mesh")]
        public void TestSlappedMesh()
        {
            if (m_CurrentAppearance != null)
            {
                m_IsUsingSlappedMesh = !m_IsUsingSlappedMesh; // Toggle
                ApplyAppearance(m_CurrentAppearance);
                Debug.Log($"Test: Switched to {(m_IsUsingSlappedMesh ? "slapped" : "normal")} mesh");
            }
        }
        
        [ContextMenu("Force Check Health")]
        public void ForceCheckHealth()
        {
            if (m_AIHealth != null)
            {
                int currentHealth = m_AIHealth.GetCurrentHealth();
                int maxHealth = m_AIHealth.GetMaxHealth();
                float healthPercentage = m_AIHealth.GetHealthPercentage();
                
                Debug.Log($"Force Check Health: {currentHealth}/{maxHealth} ({healthPercentage:P1})");
                Debug.Log($"Should use slapped: {healthPercentage < m_HealthThreshold}");
                
                OnAIHealthChanged(currentHealth, maxHealth);
            }
            else
            {
                Debug.LogWarning("AIHealth not found for force check!");
            }
        }
        
        private void OnAIHealthChanged(int currentHealth, int maxHealth)
        {
            Debug.Log($"OnAIHealthChanged called: {currentHealth}/{maxHealth}");
            
            if (m_CurrentAppearance == null) 
            {
                Debug.LogWarning("m_CurrentAppearance is null!");
                return;
            }
            
            // Simple check: if health < 50%, switch to slapped mesh
            bool shouldUseSlapped = ShouldUseSlappedMesh();
            Debug.Log($"shouldUseSlapped: {shouldUseSlapped}, m_IsUsingSlappedMesh: {m_IsUsingSlappedMesh}");
            
            if (shouldUseSlapped != m_IsUsingSlappedMesh)
            {
                Debug.Log("Switching mesh...");
                ApplyAppearance(m_CurrentAppearance);
                Debug.Log($"AI Head switched to {(shouldUseSlapped ? "slapped" : "normal")} at {currentHealth}/{maxHealth} health");
            }
            else
            {
                Debug.Log("No mesh change needed");
            }
        }
        
        private bool ShouldUseSlappedMesh()
        {
            if (m_AIHealth == null) return false;
            
            float healthPercentage = m_AIHealth.GetHealthPercentage();
            return healthPercentage < m_HealthThreshold;
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (m_AIHealth != null)
            {
                m_AIHealth.OnHealthChanged -= OnAIHealthChanged;
            }
        }
    }
}
