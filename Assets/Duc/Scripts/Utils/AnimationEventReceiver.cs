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
            PlaySlapSound();
            
            if (m_ActorType == ActorType.Player)
            {
                var playerSM = GetComponent<PlayerStateMachine>();
                if (playerSM == null) playerSM = GetComponentInParent<PlayerStateMachine>();
                if (playerSM != null)
                {
                    playerSM.OnSlapHit();
                    
                    var effectManager = EffectManager.Instance;
                    if (effectManager != null)
                    {
                        bool isLastHit = IsLastHit();
                        bool isMegaSlap = IsMegaSlap();
                        
                        Debug.Log($"Player Attack - AI Health: {FindObjectOfType<AIHealth>()?.GetCurrentHealth()}, Player Damage: {GetPlayerDamage()}, IsLastHit: {isLastHit}, IsMegaSlap: {isMegaSlap}");
                        
                        if (isLastHit)
                        {
                            Debug.Log("Playing AI LastHit Effect Combined");
                            effectManager.PlayAILastHitEffectCombined();
                        }
                        else if (isMegaSlap)
                        {
                            Debug.Log("Playing AI Hit Effect");
                            effectManager.PlayAIHitEffect();
                        }
                        else
                        {
                            Debug.Log("Playing AI Normal Hit Effect");
                            effectManager.PlayAINormalHitEffect();
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
                    
                    var effectManager = EffectManager.Instance;
                    if (effectManager != null)
                    {
                        if (IsLastHit())
                        {
                            effectManager.PlayPlayerLastHitEffectCombined();
                        }
                        else if (IsMegaSlap())
                        {
                            effectManager.PlayPlayerHitEffect();
                        }
                        else
                        {
                            effectManager.PlayPlayerNormalHitEffect();
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
                    var effectManager = EffectManager.Instance;
                    bool isLastHit = IsLastHit();
                    bool isMegaSlap = IsMegaSlap();
                    
                    turnManager.ApplyPlayerDamage();
                    
                    if (effectManager != null)
                    {
                        Debug.Log($"TurnManager Player Attack - AI Health: {FindObjectOfType<AIHealth>()?.GetCurrentHealth()}, Player Damage: {GetPlayerDamage()}, IsLastHit: {isLastHit}, IsMegaSlap: {isMegaSlap}");
                        
                        if (isLastHit)
                        {
                            Debug.Log("TurnManager Playing AI LastHit Effect Combined");
                            effectManager.PlayAILastHitEffectCombined();
                        }
                        else if (isMegaSlap)
                        {
                            Debug.Log("TurnManager Playing AI Hit Effect");
                            effectManager.PlayAIHitEffect();
                        }
                        else
                        {
                            Debug.Log("TurnManager Playing AI Normal Hit Effect");
                            effectManager.PlayAINormalHitEffect();
                        }
                    }
                }
                else
                {
                    var effectManager2 = EffectManager.Instance;
                    bool isLastHit = IsLastHit();
                    bool isMegaSlap = IsMegaSlap();
                    
                    turnManager.ApplyAIDamage();
                    
                    if (effectManager2 != null)
                    {
                        if (isLastHit)
                        {
                            effectManager2.PlayPlayerLastHitEffectCombined();
                        }
                        else if (isMegaSlap)
                        {
                            effectManager2.PlayPlayerHitEffect();
                        }
                        else
                        {
                            effectManager2.PlayPlayerNormalHitEffect();
                        }
                    }
                }
            }
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
                        int currentHealth = aiHealth.GetCurrentHealth();
                        int damage = GetPlayerDamage();
                        return currentHealth > 0 && currentHealth <= damage;
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
                        int currentHealth = playerHealth.GetCurrentHealth();
                        int damage = GetAIDamage();
                        return currentHealth > 0 && currentHealth <= damage;
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
                int maxDmg = persistentData.GetCurrentAIMaxDamage();
                return maxDmg;
            }
            else
            {
                return 30;
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




