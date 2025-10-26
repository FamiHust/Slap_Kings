using UnityEngine;
using System.Collections.Generic;

namespace Duc
{
    [CreateAssetMenu(fileName = "AIAppearanceData", menuName = "Game/AI Appearance Data")]
    public class AIAppearanceData : ScriptableObject
    {
        [System.Serializable]
        public class AppearanceSet
        {
            [Header("Appearance Info")]
            public string setName;
            public int startLevel = 1;
            public int endLevel = 5;
            
            [Header("Head Mesh")]
            public Mesh headMesh;
            
            [Header("Head Slapped Mesh (for low health)")]
            public Mesh headSlappedMesh;
            
            [Header("Body Mesh")]
            public Mesh bodyMesh;
            
            [Header("Avatar Sprite")]
            [HideInInspector] public Sprite avatarSprite;
            
            [Header("Additional Settings")]
            [HideInInspector] public bool allowMultipleInstances = false;
            [HideInInspector] public float transitionDuration = 0.5f;
            
            public bool IsLevelInRange(int level)
            {
                return level >= startLevel && level <= endLevel;
            }
            
            public int GetLevelRange()
            {
                return endLevel - startLevel + 1;
            }
            
            public void LoadAvatarSprite()
            {
                if (string.IsNullOrEmpty(setName))
                {
                    return;
                }
                
                string resourcePath = $"SO/AIAvatar/{setName.ToLower()}";
                avatarSprite = Resources.Load<Sprite>(resourcePath);
            }
        }
        
        [Header("Appearance Configuration")]
        [SerializeField] private List<AppearanceSet> m_AppearanceSets = new List<AppearanceSet>();
        
        [Header("Default Settings")]
        [SerializeField] private AppearanceSet m_DefaultAppearance;
        [SerializeField] private bool m_EnableSmoothTransitions = true;
        
        public List<AppearanceSet> AppearanceSets => m_AppearanceSets;
        public AppearanceSet DefaultAppearance => m_DefaultAppearance;
        public bool EnableSmoothTransitions => m_EnableSmoothTransitions;

        public AppearanceSet GetAppearanceForLevel(int level)
        {
            foreach (var appearance in m_AppearanceSets)
            {
                if (appearance.IsLevelInRange(level))
                {
                    return appearance;
                }
            }
            
            return m_DefaultAppearance; 
        }
        
        public List<AppearanceSet> GetAllAppearancesForLevel(int level)
        {
            List<AppearanceSet> result = new List<AppearanceSet>();
            
            foreach (var appearance in m_AppearanceSets)
            {
                if (appearance.IsLevelInRange(level))
                {
                    result.Add(appearance);
                }
            }
            
            return result;
        }

        public bool HasAppearanceForLevel(int level)
        {
            return GetAppearanceForLevel(level) != null;
        }
 
        public int GetAppearanceCount()
        {
            return m_AppearanceSets.Count;
        }
        
        public void LoadAllAvatarSprites()
        {
            foreach (var appearance in m_AppearanceSets)
            {
                appearance.LoadAvatarSprite();
            }
            
            if (m_DefaultAppearance != null)
            {
                m_DefaultAppearance.LoadAvatarSprite();
            }
        }

        [ContextMenu("Validate Appearance Data")]
        public void ValidateAppearanceData()
        {
            for (int i = 0; i < m_AppearanceSets.Count; i++)
            {
                var appearance = m_AppearanceSets[i];
                
                if (string.IsNullOrEmpty(appearance.setName) || 
                    appearance.headMesh == null || 
                    appearance.headSlappedMesh == null || 
                    appearance.bodyMesh == null || 
                    appearance.avatarSprite == null || 
                    appearance.startLevel > appearance.endLevel)
                {
                    // Invalid appearance data
                }
            }
        }
    }
}
