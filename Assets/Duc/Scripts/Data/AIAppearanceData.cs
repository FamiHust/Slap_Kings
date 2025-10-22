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
            
            return m_DefaultAppearance; // Fallback to default
        }
        
        /// <summary>
        /// Lấy tất cả appearance sets cho level hiện tại
        /// </summary>
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
        
        /// <summary>
        /// Kiểm tra xem có appearance nào cho level này không
        /// </summary>
        public bool HasAppearanceForLevel(int level)
        {
            return GetAppearanceForLevel(level) != null;
        }
        
        /// <summary>
        /// Lấy tổng số appearance sets được cấu hình
        /// </summary>
        public int GetAppearanceCount()
        {
            return m_AppearanceSets.Count;
        }
        
        /// <summary>
        /// Validate dữ liệu appearance
        /// </summary>
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
                
                if (appearance.bodyMesh == null)
                {
                    Debug.LogWarning($"Appearance '{appearance.setName}' has no body mesh assigned!");
                }
                
                if (appearance.startLevel > appearance.endLevel)
                {
                    Debug.LogWarning($"Appearance '{appearance.setName}' has invalid level range: {appearance.startLevel}-{appearance.endLevel}");
                }
            }
            
            Debug.Log($"Appearance data validated. Total sets: {m_AppearanceSets.Count}");
        }
    }
}
