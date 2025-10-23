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
            public Material headMaterial;
            
            [Header("Head Slapped Mesh (for low health)")]
            public Mesh headSlappedMesh;
            public Material headSlappedMaterial;
            
            [Header("Body Mesh")]
            public Mesh bodyMesh;
            public Material bodyMaterial;
            
            [Header("Additional Settings")]
            public bool allowMultipleInstances = false;
            public float transitionDuration = 0.5f;
            
            public bool IsLevelInRange(int level)
            {
                return level >= startLevel && level <= endLevel;
            }
            
            public int GetLevelRange()
            {
                return endLevel - startLevel + 1;
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

        [ContextMenu("Validate Appearance Data")]
        public void ValidateAppearanceData()
        {
            for (int i = 0; i < m_AppearanceSets.Count; i++)
            {
                var appearance = m_AppearanceSets[i];
                
                if (string.IsNullOrEmpty(appearance.setName))
                {
                    Debug.LogWarning($"Appearance at index {i} has no name!");
                }
                
                if (appearance.headMesh == null)
                {
                    Debug.LogWarning($"Appearance '{appearance.setName}' has no head mesh assigned!");
                }
                
                if (appearance.headSlappedMesh == null)
                {
                    Debug.LogWarning($"Appearance '{appearance.setName}' has no head slapped mesh assigned!");
                }
                
                if (appearance.bodyMesh == null)
                {
                    Debug.LogWarning($"Appearance '{appearance.setName}' has no body mesh assigned!");
                }
                
                if (appearance.startLevel > appearance.endLevel)
                {
                    Debug.LogWarning($"Appearance '{appearance.setName}' has invalid level range: {appearance.startLevel}-{appearance.endLevel}");
                }
            }
        }
    }
}
