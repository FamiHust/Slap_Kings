using UnityEngine;
using Duc.Managers;

namespace Duc
{
    public class AnimationEventReceiver : MonoBehaviour
    {
        public enum ActorType
        {
            Player,
            AI
        }

        [SerializeField] private ActorType m_ActorType = ActorType.Player;

        public void OnSlapHit()
        {
            // Play slap sound based on type
            PlaySlapSound();
            
            if (m_ActorType == ActorType.Player)
            {
                var playerSM = GetComponent<PlayerStateMachine>();
                if (playerSM == null) playerSM = GetComponentInParent<PlayerStateMachine>();
                if (playerSM != null)
                {
                    playerSM.OnSlapHit();
                    
                    if (ShouldPlayHitEffect())
                    {
                        var effectManager = EffectManager.Instance;
                        if (effectManager != null)
                        {
                            effectManager.PlayAIHitEffect();
                        }
                    }
                    return;
                }
            }
            else 
            {
                var aiSM = GetComponent<AIStateMachine>();
                if (aiSM == null) aiSM = GetComponentInParent<AIStateMachine>();
                if (aiSM != null)
                {
                    aiSM.OnSlapHit();
                    
                    if (ShouldPlayHitEffect())
                    {
                        var effectManager = EffectManager.Instance;
                        if (effectManager != null)
                        {
                            effectManager.PlayPlayerHitEffect();
                        }
                    }
                    return;
                }
            }

            var turnManager = FindObjectOfType<TurnManager>();
            if (turnManager != null)
            {
                if (m_ActorType == ActorType.Player)
                {
                    turnManager.ApplyPlayerDamage();
                    
                    if (ShouldPlayHitEffect())
                    {
                        var effectManager = EffectManager.Instance;
                        if (effectManager != null)
                        {
                            effectManager.PlayAIHitEffect();
                        }
                    }
                }
                else
                {
                    turnManager.ApplyAIDamage();
                    
                    if (ShouldPlayHitEffect())
                    {
                        var effectManager = EffectManager.Instance;
                        if (effectManager != null)
                        {
                            effectManager.PlayPlayerHitEffect();
                        }
                    }
                }
            }
        }
        
        private bool ShouldPlayHitEffect()
        {
            if (IsMegaSlap())
            {
                return true;
            }
            
            if (IsLastHit())
            {
                return true;
            }
            
            return false;
        }
        
        private bool IsMegaSlap()
        {
            var powerMeter = PowerMeter.Get();
            if (powerMeter != null)
            {
                int power = powerMeter.GetPowerValue();
                int maxPower = powerMeter.GetMaxPower();
                return power >= (maxPower / 2); 
            }
            return false;
        }
        
        private bool IsLastHit()
        {
            if (m_ActorType == ActorType.Player)
            {
                var aiHealth = FindObjectOfType<AIHealth>();
                if (aiHealth != null)
                {
                    var turnManager = FindObjectOfType<TurnManager>();
                    if (turnManager != null)
                    {
                        int damage = GetPlayerDamage();
                        return aiHealth.GetCurrentHealth() <= damage;
                    }
                }
            }
            else
            {
                var playerHealth = FindObjectOfType<PlayerHealth>();
                if (playerHealth != null)
                {
                    var turnManager = FindObjectOfType<TurnManager>();
                    if (turnManager != null)
                    {
                        int damage = GetAIDamage();
                        return playerHealth.GetCurrentHealth() <= damage;
                    }
                }
            }
            return false;
        }
        
        private int GetPlayerDamage()
        {
            var powerMeter = PowerMeter.Get();
            if (powerMeter != null)
            {
                return Mathf.Max(0, powerMeter.GetPowerValue());
            }
            return Random.Range(10, 31);
        }
        
        private int GetAIDamage()
        {
            var persistentData = PersistentDataManager.Instance;
            if (persistentData != null)
            {
                int minDmg = persistentData.GetCurrentAIMinDamage();
                int maxDmg = persistentData.GetCurrentAIMaxDamage();
                return Random.Range(minDmg, maxDmg + 1);
            }
            else
            {
                return Random.Range(10, 31);
            }
        }

        private void PlaySlapSound()
        {
            var soundManager = SoundManager.Get();
            if (soundManager == null) return;
            
            if (IsLastHit())
            {
                soundManager.PlaySound(SoundManager.SoundType.LastHit);
            }
            else if (IsMegaSlap())
            {
                soundManager.PlaySound(SoundManager.SoundType.MegaSlap);
            }
            else
            {
                soundManager.PlaySound(SoundManager.SoundType.NormalSlap);
            }
        }
    }
}


