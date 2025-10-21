using UnityEngine;

namespace Duc
{
    [CreateAssetMenu(fileName = "PlayerStatsData", menuName = "Game/Player Stats Data")]
    public class PlayerStatsData : BaseCharacterStats
    {
        [System.Serializable]
        public class PlayerHealthSettings : HealthSettings
        {
            [Header("Upgrade System")]
            public int healthPerUpgrade = 10;
            
            protected override int GetHealthBonus(int upgradeCount)
            {
                return upgradeCount * healthPerUpgrade;
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
        public PlayerHealthSettings playerHealth = new PlayerHealthSettings();
        public PowerSettings power = new PowerSettings();
        public ControlSettings controls = new ControlSettings();

        // Override health settings to use PlayerHealthSettings
        public new PlayerHealthSettings health => playerHealth;

        // Implement abstract methods
        public override int GetMaxHealthWithUpgrades(int upgradeCount)
        {
            return playerHealth.GetMaxHealth(upgradeCount);
        }

        public override int GetScaledHealth(int level)
        {
            return playerHealth.GetMaxHealth(level);
        }

        // Convenience getters for backward compatibility
        public int HealthPerUpgrade => playerHealth.healthPerUpgrade;
        public int BaseMinPower => power.baseMinPower;
        public int BaseMaxPower => power.baseMaxPower;
        public int MinPowerPerUpgrade => power.minPowerPerUpgrade;
        public int MaxPowerPerUpgrade => power.maxPowerPerUpgrade;
        public int DamagePerPowerUpgrade => power.damagePerPowerUpgrade;
        public float PowerMeterAnimSpeed => power.animSpeed;
        public bool LoopPingPong => power.loopPingPong;
        public bool EnableControlsInIdle => controls.enableInIdle;
        public bool EnableControlsInWaiting => controls.enableInWaiting;
        public bool EnableControlsInAttacking => controls.enableInAttacking;
        public bool EnableControlsInDead => controls.enableInDead;

        // Legacy methods for backward compatibility
        public int GetMinPowerWithUpgrades(int upgradeCount) => power.GetMinPowerWithUpgrades(upgradeCount);
        public int GetMaxPowerWithUpgrades(int upgradeCount) => power.GetMaxPowerWithUpgrades(upgradeCount);
        public int GetPowerBonusDamage(int upgradeCount) => power.GetPowerBonusDamage(upgradeCount);
    }
}