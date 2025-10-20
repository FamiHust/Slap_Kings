using UnityEngine;

[CreateAssetMenu(fileName = "AIStatsData", menuName = "Game/AI Stats Data")]
public class AIStatsData : ScriptableObject
{
    [System.Serializable]
    public class HealthSettings
    {
        [Header("Base Health")]
        public int baseMaxHealth = 100;
        
        [Header("Level Scaling")]
        public int healthPerLevel = 20;
        public float healthScalingMultiplier = 1.0f;
        
        public int GetScaledHealth(int level)
        {
            return baseMaxHealth + (level - 1) * healthPerLevel;
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
    public class KnockbackSettings
    {
        [Header("Knockback Force")]
        public float force = 500f;
        public float multiplier = 5.0f;
        public Vector3 direction = new Vector3(0, 0.3f, -1f);
        
        [Header("Ragdoll Settings")]
        public bool useAllRigidbodies = true;
        public float rigidbodyForceMultiplier = 2.0f;
        public float duration = 2.0f;
        
        public float CalculateForce(float damage)
        {
            return force + (damage * multiplier);
        }
    }

    [System.Serializable]
    public class AnimationSettings
    {
        [Header("Animation Variants")]
        public int slapVariantCount = 20;
        public float attackDuration = 1f;
        
        [Header("Victory Animation")]
        public int victoryVariantCount = 12;
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
    public HealthSettings health = new HealthSettings();
    public DamageSettings damage = new DamageSettings();
    public KnockbackSettings knockback = new KnockbackSettings();
    public AnimationSettings animation = new AnimationSettings();
    public BehaviorSettings behavior = new BehaviorSettings();

    // Convenience getters for backward compatibility
    public int BaseMaxHealth => health.baseMaxHealth;
    public int MinDamage => damage.minDamage;
    public int MaxDamage => damage.maxDamage;
    public float KnockbackForce => knockback.force;
    public float KnockbackMultiplier => knockback.multiplier;
    public Vector3 KnockbackDirection => knockback.direction;
    public bool UseAllRigidbodies => knockback.useAllRigidbodies;
    public float RigidbodyForceMultiplier => knockback.rigidbodyForceMultiplier;
    public float KnockbackDuration => knockback.duration;
    public int SlapVariantCount => animation.slapVariantCount;
    public float AttackDuration => animation.attackDuration;
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
    public int GetScaledHealth(int level) => health.GetScaledHealth(level);
    public int GetScaledRandomDamage(int level) => damage.GetRandomDamage(level);
    public int GetScaledAverageDamage(int level) => damage.GetAverageDamage(level);
    public int GetScaledMinDamage(int level) => damage.GetScaledMinDamage(level);
    public int GetScaledMaxDamage(int level) => damage.GetScaledMaxDamage(level);
    public float GetScaledNormalizedDamage(int damage, int level) => this.damage.GetNormalizedDamage(damage, level);
}