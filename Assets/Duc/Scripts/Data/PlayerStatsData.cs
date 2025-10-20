using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatsData", menuName = "Game/Player Stats Data")]
public class PlayerStatsData : ScriptableObject
{
    [System.Serializable]
    public class HealthSettings
    {
        [Header("Base Health")]
        public int baseMaxHealth = 100;
        public int healthPerUpgrade = 10;
        
        public int GetMaxHealthWithUpgrades(int upgradeCount)
        {
            return baseMaxHealth + (upgradeCount * healthPerUpgrade);
        }
    }

    [System.Serializable]
    public class PowerSettings
    {
        [Header("Power Range")]
        public int baseMinPower = 0;
        public int baseMaxPower = 90;
        public int minPowerPerUpgrade = 5;
        public int maxPowerPerUpgrade = 10;
        public int damagePerPowerUpgrade = 1;
        
        [Header("Power Meter Animation")]
        public float animSpeed = 2f;
        public bool loopPingPong = true;
        
        public int GetMinPowerWithUpgrades(int upgradeCount)
        {
            return baseMinPower + (upgradeCount * minPowerPerUpgrade);
        }
        
        public int GetMaxPowerWithUpgrades(int upgradeCount)
        {
            return baseMaxPower + (upgradeCount * maxPowerPerUpgrade);
        }
        
        public int GetPowerBonusDamage(int upgradeCount)
        {
            return upgradeCount * damagePerPowerUpgrade;
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
    public class ControlSettings
    {
        [Header("State Controls")]
        public bool enableInIdle = true;
        public bool enableInWaiting = false;
        public bool enableInAttacking = false;
        public bool enableInDead = false;
        
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

    [Header("Player Configuration")]
    public HealthSettings health = new HealthSettings();
    public PowerSettings power = new PowerSettings();
    public KnockbackSettings knockback = new KnockbackSettings();
    public AnimationSettings animation = new AnimationSettings();
    public ControlSettings controls = new ControlSettings();

    // Convenience getters for backward compatibility
    public int BaseMaxHealth => health.baseMaxHealth;
    public int HealthPerUpgrade => health.healthPerUpgrade;
    public int BaseMinPower => power.baseMinPower;
    public int BaseMaxPower => power.baseMaxPower;
    public int MinPowerPerUpgrade => power.minPowerPerUpgrade;
    public int MaxPowerPerUpgrade => power.maxPowerPerUpgrade;
    public int DamagePerPowerUpgrade => power.damagePerPowerUpgrade;
    public float PowerMeterAnimSpeed => power.animSpeed;
    public bool LoopPingPong => power.loopPingPong;
    public float KnockbackForce => knockback.force;
    public float KnockbackMultiplier => knockback.multiplier;
    public Vector3 KnockbackDirection => knockback.direction;
    public bool UseAllRigidbodies => knockback.useAllRigidbodies;
    public float RigidbodyForceMultiplier => knockback.rigidbodyForceMultiplier;
    public float KnockbackDuration => knockback.duration;
    public int SlapVariantCount => animation.slapVariantCount;
    public float AttackDuration => animation.attackDuration;
    public bool EnableControlsInIdle => controls.enableInIdle;
    public bool EnableControlsInWaiting => controls.enableInWaiting;
    public bool EnableControlsInAttacking => controls.enableInAttacking;
    public bool EnableControlsInDead => controls.enableInDead;

    // Legacy methods for backward compatibility
    public int GetMaxHealthWithUpgrades(int upgradeCount) => health.GetMaxHealthWithUpgrades(upgradeCount);
    public int GetMinPowerWithUpgrades(int upgradeCount) => power.GetMinPowerWithUpgrades(upgradeCount);
    public int GetMaxPowerWithUpgrades(int upgradeCount) => power.GetMaxPowerWithUpgrades(upgradeCount);
    public int GetPowerBonusDamage(int upgradeCount) => power.GetPowerBonusDamage(upgradeCount);
}