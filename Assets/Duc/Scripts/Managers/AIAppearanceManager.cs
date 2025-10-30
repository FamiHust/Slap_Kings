using UnityEngine;
using UnityEngine.UI;
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
        
        [Header("Avatar Image Component")]
        [SerializeField] private Image m_AvatarImage;
        
        [Header("Settings")]
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
            
            if (m_AvatarImage == null)
            {
                m_AvatarImage = FindImageComponent("Avatar");
            }
            
            m_AIHealth = GetComponent<AIHealth>();
            if (m_AIHealth == null)
            {
                m_AIHealth = GetComponentInChildren<AIHealth>();
                if (m_AIHealth == null)
                {
                    m_AIHealth = GetComponentInParent<AIHealth>();
                    if (m_AIHealth == null)
                    {
                        m_AIHealth = FindObjectOfType<AIHealth>();
                    }
                }
            }
        }
        
        private void Start()
        {
            // Load all avatar sprites from Resources
            if (m_AppearanceData != null)
            {
                m_AppearanceData.LoadAllAvatarSprites();
            }
            
            if (m_AutoUpdateOnStart)
            {
                UpdateAppearanceForCurrentLevel();
            }
            
            if (m_AIHealth != null)
            {
                m_AIHealth.OnHealthChanged += OnAIHealthChanged;
            }
            else
            {
                Debug.LogWarning("AIAppearanceManager: AIHealth is null, cannot subscribe to health changes");
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
        
        private Image FindImageComponent(string name)
        {
            Image[] images = GetComponentsInChildren<Image>();
            
            foreach (var image in images)
            {
                if (image.name.ToLower().Contains(name.ToLower()))
                {
                    return image;
                }
            }
            
            if (images.Length > 0)
            {
                return images[0];
            }
            
            return null;
        }
        
        private bool IsMeshCompatible(SkinnedMeshRenderer renderer, Mesh mesh)
        {
            
            if (renderer == null || mesh == null) 
            {
                return false;
            }
            
            
            // Check if the mesh has the same vertex count and bone structure
            if (renderer.sharedMesh != null)
            {
                
                // Basic compatibility check - same vertex count
                // Note: Vertex count can be different for different mesh variants (e.g., normal vs slapped)
                if (renderer.sharedMesh.vertexCount != mesh.vertexCount)
                {
                    // Don't return false for vertex count mismatch - different meshes can have different vertex counts
                }
                
                // Check bone count if using bones
                if (renderer.bones != null && renderer.bones.Length > 0)
                {
                    
                    if (mesh.bindposes == null || mesh.bindposes.Length != renderer.bones.Length)
                    {
                        return false;
                    }
                }
                else
                {
                }
            }
            else
            {
            }
            
            
            return true;
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
                bool useSlappedMesh = false;
                if (m_AIHealth != null && m_AIHealth.GetCurrentHealth() > 0)
                {
                    useSlappedMesh = ShouldUseSlappedMesh();
                }
                
                
                if (useSlappedMesh)
                {
                    
                    if (appearance.headSlappedMesh != null)
                    {
                        
                        
                        if (IsMeshCompatible(m_HeadRenderer, appearance.headSlappedMesh))
                        {
                            m_HeadRenderer.sharedMesh = appearance.headSlappedMesh;
                            m_IsUsingSlappedMesh = true;
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                    }
                }
                else
                {
                    
                    if (appearance.headMesh != null && IsMeshCompatible(m_HeadRenderer, appearance.headMesh))
                    {
                        m_HeadRenderer.sharedMesh = appearance.headMesh;
                    }
                    else
                    {
                    }
                    m_IsUsingSlappedMesh = false;
                }
            }
            
            if (m_BodyRenderer != null)
            {
                if (appearance.bodyMesh != null && IsMeshCompatible(m_BodyRenderer, appearance.bodyMesh))
                {
                    m_BodyRenderer.sharedMesh = appearance.bodyMesh;
                }
            }
            
            if (m_AvatarImage != null)
            {
                if (appearance.avatarSprite != null)
                {
                    m_AvatarImage.sprite = appearance.avatarSprite;
                }
                else
                {
                    appearance.LoadAvatarSprite();
                    if (appearance.avatarSprite != null)
                    {
                        m_AvatarImage.sprite = appearance.avatarSprite;
                    }
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
        
        public void SetAvatarImage(Image image)
        {
            m_AvatarImage = image;
        }

        private void OnAIHealthChanged(int currentHealth, int maxHealth)
        {
            if (m_CurrentAppearance == null) 
            {
                return;
            }
            
            if (currentHealth < maxHealth)
            {
                bool shouldUseSlapped = ShouldUseSlappedMesh();
                
                if (shouldUseSlapped != m_IsUsingSlappedMesh)
                {
                    ApplyAppearance(m_CurrentAppearance);
                }
            }
            else
            {
                if (m_IsUsingSlappedMesh)
                {
                    m_IsUsingSlappedMesh = false;
                    ApplyAppearance(m_CurrentAppearance);
                }
            }
        }
        
        private bool ShouldUseSlappedMesh()
        {
            if (m_AIHealth == null) return false;
            
            float healthPercentage = m_AIHealth.GetHealthPercentage();
            bool shouldUse = healthPercentage < m_HealthThreshold;
            
            
            return shouldUse;
        }
        
        private void OnDestroy()
        {
            if (m_AIHealth != null)
            {
                m_AIHealth.OnHealthChanged -= OnAIHealthChanged;
            }
        }
    }
}
