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
        
        private AIAppearanceData.AppearanceSet m_CurrentAppearance;
        
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
        }
        
        private void Start()
        {
            if (m_AutoUpdateOnStart)
            {
                UpdateAppearanceForCurrentLevel();
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
            if (m_HeadRenderer != null)
            {
                if (appearance.headMesh != null)
                {
                    m_HeadRenderer.sharedMesh = appearance.headMesh;
                }
                
                if (appearance.headMaterial != null)
                {
                    m_HeadRenderer.material = appearance.headMaterial;
                }
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
        
        private void OnDestroy()
        {
            
        }
    }
}
