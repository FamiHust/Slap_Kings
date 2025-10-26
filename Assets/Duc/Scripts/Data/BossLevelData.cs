using UnityEngine;
using System.Collections.Generic;

namespace Duc
{
    [CreateAssetMenu(fileName = "BossLevelData", menuName = "Game/Boss Level Data")]
    public class BossLevelData : ScriptableObject
    {
        [System.Serializable]
        public class BossLevel
        {
            [Header("Boss Level Info")]
            public int level;
            public string bossName;
            public bool isActive = true;
            
            [Header("Boss Multipliers")]
            [Tooltip("Health multiplier for boss level (2.0 = double health)")]
            public float healthMultiplier = 2.0f;
            [Tooltip("Damage multiplier for boss level (2.0 = double damage)")]
            public float damageMultiplier = 2.0f;
            [Tooltip("Speed multiplier for PowerMeter and CounterMeter (1.0 = normal, 1.5 = +50%)")]
            public float speedMultiplier = 1.0f;
            
            
            public bool IsLevelBoss(int checkLevel)
            {
                return isActive && level == checkLevel;
            }
        }
        
        [Header("Boss Level Configuration")]
        [SerializeField] private List<BossLevel> m_BossLevels = new List<BossLevel>();
        
        [Header("Default Settings")]
        [SerializeField] private bool m_EnableBossLevels = true;
        [SerializeField] private float m_DefaultHealthMultiplier = 2.0f;
        [SerializeField] private float m_DefaultDamageMultiplier = 2.0f;
        [SerializeField] private float m_DefaultSpeedMultiplier = 1.0f;
        
        public List<BossLevel> BossLevels => m_BossLevels;
        public bool EnableBossLevels => m_EnableBossLevels;
        public float DefaultHealthMultiplier => m_DefaultHealthMultiplier;
        public float DefaultDamageMultiplier => m_DefaultDamageMultiplier;
        public float DefaultSpeedMultiplier => m_DefaultSpeedMultiplier;

        public bool IsBossLevel(int level)
        {
            if (!m_EnableBossLevels) return false;
            
            foreach (var bossLevel in m_BossLevels)
            {
                if (bossLevel.IsLevelBoss(level))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        public BossLevel GetBossLevel(int level)
        {
            foreach (var bossLevel in m_BossLevels)
            {
                if (bossLevel.IsLevelBoss(level))
                {
                    return bossLevel;
                }
            }
            
            return null;
        }

        public float GetHealthMultiplier(int level)
        {
            if (IsBossLevel(level))
            {
                var bossLevel = GetBossLevel(level);
                return bossLevel != null ? bossLevel.healthMultiplier : m_DefaultHealthMultiplier;
            }
            
            return 1.0f;
        }

        public float GetDamageMultiplier(int level)
        {
            if (IsBossLevel(level))
            {
                var bossLevel = GetBossLevel(level);
                return bossLevel != null ? bossLevel.damageMultiplier : m_DefaultDamageMultiplier;
            }
            
            return 1.0f; 
        }

        public float GetSpeedMultiplier(int level)
        {
            if (IsBossLevel(level))
            {
                var bossLevel = GetBossLevel(level);
                return bossLevel != null ? Mathf.Max(0f, bossLevel.speedMultiplier) : Mathf.Max(0f, m_DefaultSpeedMultiplier);
            }
            return 1.0f;
        }
        
        public float GetSpeedBonus(int level)
        {
            return GetSpeedMultiplier(level) - 1.0f;
        }

        public string GetBossName(int level)
        {
            var bossLevel = GetBossLevel(level);
            return bossLevel != null ? bossLevel.bossName : "Normal Level";
        }

        [ContextMenu("Validate Boss Level Data")]
        public void ValidateBossLevelData()
        {
            for (int i = 0; i < m_BossLevels.Count; i++)
            {
                var bossLevel = m_BossLevels[i];
                
                if (string.IsNullOrEmpty(bossLevel.bossName) || 
                    bossLevel.level <= 0 || 
                    bossLevel.healthMultiplier <= 0 || 
                    bossLevel.damageMultiplier <= 0)
                {
                    // Invalid boss level data
                }
            }
        }
    }
}
