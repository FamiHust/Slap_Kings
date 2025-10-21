using UnityEngine;

namespace Duc
{
    [CreateAssetMenu(fileName = "AIStatsData", menuName = "Game/AI Stats Data")]
    public class AIStatsData : BaseCharacterStats
    {
        [System.Serializable]
        public class AIHealthSettings : HealthSettings
        {
            [Header("Level Scaling")]
            public int healthPerLevel = 20;
            public float healthScalingMultiplier = 1.0f;
            
            protected override int GetHealthBonus(int level)
            {
                return (level - 1) * healthPerLevel;
            }
        }

        [System.Serializable]
        public class DamageSettings
        {
            [Header("Damage Range")]
            public int minDamage = 10;
            public int maxDamage = 30;
            
            [Header("Level Scaling")]
            public int damagePerLevel = 5;
            public float damageScalingMultiplier = 1.0f;
            
            public int GetRandomDamage()
            {
                return Random.Range(minDamage, maxDamage + 1);
            }
            
            public int GetRandomDamage(int level)
            {
                int scaledMin = GetScaledMinDamage(level);
                int scaledMax = GetScaledMaxDamage(level);
                return Random.Range(scaledMin, scaledMax + 1);
            }
            
            public int GetAverageDamage()
            {
                return (minDamage + maxDamage) / 2;
            }
            
            public int GetAverageDamage(int level)
            {
                int scaledMin = GetScaledMinDamage(level);
                int scaledMax = GetScaledMaxDamage(level);
                return (scaledMin + scaledMax) / 2;
            }
            
            public int GetScaledMinDamage(int level)
            {
                return minDamage + (level - 1) * damagePerLevel;
            }
            
            public int GetScaledMaxDamage(int level)
            {
                return maxDamage + (level - 1) * damagePerLevel;
            }
            
            public float GetNormalizedDamage(int damage)
            {
                return Mathf.Clamp01((float)(damage - minDamage) / (maxDamage - minDamage));
            }
            
            public float GetNormalizedDamage(int damage, int level)
            {
                int scaledMin = GetScaledMinDamage(level);
                int scaledMax = GetScaledMaxDamage(level);
                return Mathf.Clamp01((float)(damage - scaledMin) / (scaledMax - scaledMin));
            }
        }


        [System.Serializable]
        public class BehaviorSettings
        {
            [Header("State Controls")]
            public bool enableInIdle = true;
            public bool enableInWaiting = false;
            public bool enableInAttacking = false;
            public bool enableInDead = false;
            
            [Header("Timing Settings")]
            public float waitTime = 1.5f;
            public float attackTime = 2.0f;
            public float getSlappedDelay = 0.5f;
            
            public bool GetControlEnabled(CharacterState state)
            {
                switch (state)
                {
                    case CharacterState.Idle: return enableInIdle;
                    case CharacterState.Waiting: return enableInWaiting;
                    case CharacterState.Attacking: return enableInAttacking;
                    case CharacterState.Dead: return enableInDead;
                    default: return false;
                }
            }
        }

        [Header("AI Configuration")]
        public AIHealthSettings aiHealth = new AIHealthSettings();
        public DamageSettings damage = new DamageSettings();
        public BehaviorSettings behavior = new BehaviorSettings();

        // Override health settings to use AIHealthSettings
        public new AIHealthSettings health => aiHealth;

        // Implement abstract methods
        public override int GetMaxHealthWithUpgrades(int level)
        {
            return aiHealth.GetMaxHealth(level);
        }

        public override int GetScaledHealth(int level)
        {
            return aiHealth.GetMaxHealth(level);
        }

        // Convenience getters for backward compatibility
        public int MinDamage => damage.minDamage;
        public int MaxDamage => damage.maxDamage;
        public bool EnableAIInIdle => behavior.enableInIdle;
        public bool EnableAIInWaiting => behavior.enableInWaiting;
        public bool EnableAIInAttacking => behavior.enableInAttacking;
        public bool EnableAIInDead => behavior.enableInDead;
        public float AIWaitTime => behavior.waitTime;
        public float AIAttackTime => behavior.attackTime;
        public float AIGetSlappedDelay => behavior.getSlappedDelay;

        // Legacy methods for backward compatibility
        public int GetRandomDamage() => damage.GetRandomDamage();
        public int GetAverageDamage() => damage.GetAverageDamage();
        public float GetNormalizedDamage(int damage) => this.damage.GetNormalizedDamage(damage);
        
        // Level-scaled methods
        public int GetScaledRandomDamage(int level) => damage.GetRandomDamage(level);
        public int GetScaledAverageDamage(int level) => damage.GetAverageDamage(level);
        public int GetScaledMinDamage(int level) => damage.GetScaledMinDamage(level);
        public int GetScaledMaxDamage(int level) => damage.GetScaledMaxDamage(level);
        public float GetScaledNormalizedDamage(int damage, int level) => this.damage.GetNormalizedDamage(damage, level);
    }
}