using UnityEngine;

namespace Duc
{
    public interface ICharacterStats
    {
        int BaseMaxHealth { get; }
        float KnockbackForce { get; }
        float KnockbackMultiplier { get; }
        Vector3 KnockbackDirection { get; }
        bool UseAllRigidbodies { get; }
        float RigidbodyForceMultiplier { get; }
        float KnockbackDuration { get; }
        int SlapVariantCount { get; }
        float AttackDuration { get; }
    }

    public abstract class BaseCharacterStats : ScriptableObject, ICharacterStats
    {
        [System.Serializable]
        public class HealthSettings
        {
            [Header("Base Health")]
            public int baseMaxHealth = 200;
            
            public int GetMaxHealth(int level = 1)
            {
                return baseMaxHealth + GetHealthBonus(level);
            }
            
            protected virtual int GetHealthBonus(int level)
            {
                return 0;
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

        [Header("Character Configuration")]
        public HealthSettings health = new HealthSettings();
        public KnockbackSettings knockback = new KnockbackSettings();
        public AnimationSettings animation = new AnimationSettings();

        public int BaseMaxHealth => health.baseMaxHealth;
        public float KnockbackForce => knockback.force;
        public float KnockbackMultiplier => knockback.multiplier;
        public Vector3 KnockbackDirection => knockback.direction;
        public bool UseAllRigidbodies => knockback.useAllRigidbodies;
        public float RigidbodyForceMultiplier => knockback.rigidbodyForceMultiplier;
        public float KnockbackDuration => knockback.duration;
        public int SlapVariantCount => animation.slapVariantCount;
        public float AttackDuration => animation.attackDuration;

        public abstract int GetMaxHealthWithUpgrades(int upgradeCount);
        public abstract int GetScaledHealth(int level);
    }
}
