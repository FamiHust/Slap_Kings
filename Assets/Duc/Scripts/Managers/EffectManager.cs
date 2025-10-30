using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Duc.Managers
{
    public class EffectManager : SingletonManager<EffectManager>
    {
        [Header("Upgrade Effects")]
        [SerializeField] private ParticleSystem m_HealthUpgradeEffect;
        [SerializeField] private ParticleSystem m_PowerUpgradeEffect;
        [SerializeField] private int m_EffectPoolSize = 5;
        
        [Header("Combat Effects")]
        [SerializeField] private ParticleSystem m_PlayerHitEffect;
        [SerializeField] private ParticleSystem m_AIHitEffect;
        [SerializeField] private ParticleSystem m_PlayerLastHitEffect;
        [SerializeField] private ParticleSystem m_AILastHitEffect;
        [SerializeField] private ParticleSystem m_PlayerNormalHitEffect;
        [SerializeField] private ParticleSystem m_AINormalHitEffect;
        [SerializeField] private ParticleSystem m_PlayerShieldEffect;
        [SerializeField] private ParticleSystem m_PlayerShieldDeathEffect;
        [SerializeField] private int m_CombatEffectPoolSize = 10;
        
        [Header("Skin Effects")]
        [SerializeField] private ParticleSystem m_SkinChangeEffect;
        [SerializeField] private int m_SkinEffectPoolSize = 5;
        
        [Header("Effect Spawn Positions")]
        [SerializeField] private Transform m_HealthUpgradeSpawnPoint;
        [SerializeField] private Transform m_PowerUpgradeSpawnPoint;
        [SerializeField] private Transform m_PlayerHitSpawnPoint;
        [SerializeField] private Transform m_AIHitSpawnPoint;
        [SerializeField] private Transform m_PlayerShieldSpawnPoint;
        [SerializeField] private Transform m_SkinChangeSpawnPoint;
        
        [Header("Effect Settings")]
        [SerializeField] private float m_EffectDuration = 2f;
        [SerializeField] private Vector3 m_EffectOffset = Vector3.up * 2f;
        [Header("Shield Effect Settings")]
        [SerializeField] private float m_ShieldScaleInDuration = 0.2f;
        [SerializeField] private float m_ShieldScaleOutDuration = 0.2f;
        [SerializeField] private float m_ShieldTargetScale = 1f;
        [SerializeField] private Ease m_ShieldEaseIn = Ease.OutBack;
        [SerializeField] private Ease m_ShieldEaseOut = Ease.InBack;
        
        private Queue<ParticleSystem> m_HealthUpgradePool = new Queue<ParticleSystem>();
        private Queue<ParticleSystem> m_PowerUpgradePool = new Queue<ParticleSystem>();
        private Queue<ParticleSystem> m_PlayerHitPool = new Queue<ParticleSystem>();
        private Queue<ParticleSystem> m_AIHitPool = new Queue<ParticleSystem>();
        private Queue<ParticleSystem> m_PlayerLastHitPool = new Queue<ParticleSystem>();
        private Queue<ParticleSystem> m_AILastHitPool = new Queue<ParticleSystem>();
        private Queue<ParticleSystem> m_PlayerNormalHitPool = new Queue<ParticleSystem>();
        private Queue<ParticleSystem> m_AINormalHitPool = new Queue<ParticleSystem>();
        private Queue<ParticleSystem> m_PlayerShieldPool = new Queue<ParticleSystem>();
        private Queue<ParticleSystem> m_PlayerShieldDeathPool = new Queue<ParticleSystem>();
        private Queue<ParticleSystem> m_SkinChangePool = new Queue<ParticleSystem>();
        private List<ParticleSystem> m_ActiveEffects = new List<ParticleSystem>();
        private ParticleSystem m_ActivePlayerShieldEffect;
        
        protected override void OnInitialize()
        {
            InitializeEffectPools();
            SubscribeToEvents();
        }
        
        protected override void OnCleanup()
        {
            StopAllCoroutines();
            UnsubscribeFromEvents();
            ClearAllEffects();
        }
        
        private void InitializeEffectPools()
        {
            if (m_HealthUpgradeEffect != null)
            {
                for (int i = 0; i < m_EffectPoolSize; i++)
                {
                    ParticleSystem effect = Instantiate(m_HealthUpgradeEffect, transform);
                    effect.gameObject.SetActive(false);
                    m_HealthUpgradePool.Enqueue(effect);
                }
            }

            if (m_PlayerShieldEffect != null)
            {
                for (int i = 0; i < m_CombatEffectPoolSize; i++)
                {
                    var instance = Instantiate(m_PlayerShieldEffect, transform);
                    instance.gameObject.SetActive(false);
                    m_PlayerShieldPool.Enqueue(instance);
                }
            }

            if (m_PlayerShieldDeathEffect != null)
            {
                for (int i = 0; i < m_CombatEffectPoolSize; i++)
                {
                    var instance = Instantiate(m_PlayerShieldDeathEffect, transform);
                    instance.gameObject.SetActive(false);
                    m_PlayerShieldDeathPool.Enqueue(instance);
                }
            }
            
            if (m_PowerUpgradeEffect != null)
            {
                for (int i = 0; i < m_EffectPoolSize; i++)
                {
                    ParticleSystem effect = Instantiate(m_PowerUpgradeEffect, transform);
                    effect.gameObject.SetActive(false);
                    m_PowerUpgradePool.Enqueue(effect);
                }
            }
            
            if (m_PlayerHitEffect != null)
            {
                for (int i = 0; i < m_CombatEffectPoolSize; i++)
                {
                    ParticleSystem effect = Instantiate(m_PlayerHitEffect, transform);
                    effect.gameObject.SetActive(false);
                    m_PlayerHitPool.Enqueue(effect);
                }
            }
            
            if (m_AIHitEffect != null)
            {
                for (int i = 0; i < m_CombatEffectPoolSize; i++)
                {
                    ParticleSystem effect = Instantiate(m_AIHitEffect, transform);
                    effect.gameObject.SetActive(false);
                    m_AIHitPool.Enqueue(effect);
                }
            }
            
            if (m_PlayerLastHitEffect != null)
            {
                for (int i = 0; i < m_CombatEffectPoolSize; i++)
                {
                    ParticleSystem effect = Instantiate(m_PlayerLastHitEffect, transform);
                    effect.gameObject.SetActive(false);
                    m_PlayerLastHitPool.Enqueue(effect);
                }
            }
            
            if (m_PlayerNormalHitEffect != null)
            {
                for (int i = 0; i < m_CombatEffectPoolSize; i++)
                {
                    ParticleSystem effect = Instantiate(m_PlayerNormalHitEffect, transform);
                    effect.gameObject.SetActive(false);
                    m_PlayerNormalHitPool.Enqueue(effect);
                }
            }
            
            if (m_AINormalHitEffect != null)
            {
                for (int i = 0; i < m_CombatEffectPoolSize; i++)
                {
                    ParticleSystem effect = Instantiate(m_AINormalHitEffect, transform);
                    effect.gameObject.SetActive(false);
                    m_AINormalHitPool.Enqueue(effect);
                }
            }
            
            if (m_AILastHitEffect != null)
            {
                for (int i = 0; i < m_CombatEffectPoolSize; i++)
                {
                    ParticleSystem effect = Instantiate(m_AILastHitEffect, transform);
                    effect.gameObject.SetActive(false);
                    m_AILastHitPool.Enqueue(effect);
                }
            }

            if (m_PlayerShieldEffect != null)
            {
                for (int i = 0; i < m_CombatEffectPoolSize; i++)
                {
                    ParticleSystem effect = Instantiate(m_PlayerShieldEffect, transform);
                    effect.gameObject.SetActive(false);
                    m_PlayerShieldPool.Enqueue(effect);
                }
            }

            if (m_SkinChangeEffect != null)
            {
                for (int i = 0; i < m_SkinEffectPoolSize; i++)
                {
                    ParticleSystem effect = Instantiate(m_SkinChangeEffect, transform);
                    effect.gameObject.SetActive(false);
                    m_SkinChangePool.Enqueue(effect);
                }
            }
        }
        
        private void SubscribeToEvents()
        {
            if (PersistentGameManager.Instance != null)
            {
                PersistentGameManager.Instance.OnHealthUpgradePurchased += OnHealthUpgrade;
                PersistentGameManager.Instance.OnPowerUpgradePurchased += OnPowerUpgrade;
            }
        }
        
        private void UnsubscribeFromEvents()
        {
            if (PersistentGameManager.Instance != null)
            {
                PersistentGameManager.Instance.OnHealthUpgradePurchased -= OnHealthUpgrade;
                PersistentGameManager.Instance.OnPowerUpgradePurchased -= OnPowerUpgrade;
            }
        }
        
        private void OnHealthUpgrade()
        {
            PlayHealthUpgradeEffect();
        }
        
        private void OnPowerUpgrade()
        {
            PlayPowerUpgradeEffect();
        }
        
        public void PlayHealthUpgradeEffect()
        {
            if (m_HealthUpgradePool.Count == 0)
            {
                return;
            }
            
            ParticleSystem effect = m_HealthUpgradePool.Dequeue();
            effect.gameObject.SetActive(true);
            
            Vector3 spawnPosition = GetHealthUpgradeSpawnPosition();
            effect.transform.position = spawnPosition;
            
            effect.Play();
            m_ActiveEffects.Add(effect);
            
            StartCoroutine(ReturnEffectToPool(effect, m_HealthUpgradePool));
        }
        
        public void PlayPowerUpgradeEffect()
        {
            if (m_PowerUpgradePool.Count == 0)
            {
                return;
            }
            
            ParticleSystem effect = m_PowerUpgradePool.Dequeue();
            effect.gameObject.SetActive(true);
            
            Vector3 spawnPosition = GetPowerUpgradeSpawnPosition();
            effect.transform.position = spawnPosition;
            
            if (effect != null)
            {
                effect.Play();
                m_ActiveEffects.Add(effect);
                StartCoroutine(ReturnEffectToPool(effect, m_PowerUpgradePool));
            }
        }
        
        public void PlayPlayerHitEffect()
        {
            if (m_PlayerHitPool.Count == 0)
            {
                return;
            }
            
            ParticleSystem effect = m_PlayerHitPool.Dequeue();
            effect.gameObject.SetActive(true);
            
            Vector3 spawnPosition = GetPlayerHitSpawnPosition();
            effect.transform.position = spawnPosition;
            
            if (effect != null)
            {
                effect.Play();
                m_ActiveEffects.Add(effect);
                
                this.PublishEvent(new CameraShakeEvent(3f, 0.2f, true));
                StartCoroutine(ReturnEffectToPool(effect, m_PlayerHitPool));
            }
        }
        
        public void PlayAIHitEffect()
        {
            if (m_AIHitPool.Count == 0)
            {
                return;
            }
            
            ParticleSystem effect = m_AIHitPool.Dequeue();
            effect.gameObject.SetActive(true);
            
            Vector3 spawnPosition = GetAIHitSpawnPosition();
            effect.transform.position = spawnPosition;
            
            if (effect != null)
            {
                effect.Play();
                m_ActiveEffects.Add(effect);
                
                this.PublishEvent(new CameraShakeEvent(2.5f, 0.18f, true));
                
                StartCoroutine(ReturnEffectToPool(effect, m_AIHitPool));
            }
        }
        
        public void PlayPlayerLastHitEffect()
        {
            if (m_PlayerLastHitPool.Count == 0)
            {
                return;
            }
            
            ParticleSystem effect = m_PlayerLastHitPool.Dequeue();
            effect.gameObject.SetActive(true);
            
            Vector3 spawnPosition = GetPlayerHitSpawnPosition();
            effect.transform.position = spawnPosition;
            
            if (effect != null)
            {
                effect.Play();
                m_ActiveEffects.Add(effect);
                
                StartCoroutine(ReturnEffectToPool(effect, m_PlayerLastHitPool));
            }
        }
        
        public void PlayAILastHitEffect()
        {
            if (m_AILastHitPool.Count == 0)
            {
                Debug.Log("AILastHitPool is empty!");
                return;
            }
            
            ParticleSystem effect = m_AILastHitPool.Dequeue();
            effect.gameObject.SetActive(true);
            
            Vector3 spawnPosition = GetAIHitSpawnPosition();
            effect.transform.position = spawnPosition;
            
            Debug.Log($"AILastHitEffect spawned at position: {spawnPosition}");
            
            if (effect != null)
            {
                effect.Play();
                m_ActiveEffects.Add(effect);
                
                StartCoroutine(ReturnEffectToPool(effect, m_AILastHitPool));
            }
        }
        
        public void PlayPlayerLastHitEffectCombined()
        {
            PlayPlayerHitEffect();
            PlayPlayerLastHitEffect();
        }
        
        public void PlayAILastHitEffectCombined()
        {
            Debug.Log("PlayAILastHitEffectCombined called");
            PlayAIHitEffect();
            PlayAILastHitEffect();
        }
        
        public void PlayPlayerNormalHitEffect()
        {
            if (m_PlayerNormalHitPool.Count == 0)
            {
                return;
            }
            
            ParticleSystem effect = m_PlayerNormalHitPool.Dequeue();
            effect.gameObject.SetActive(true);
            
            Vector3 spawnPosition = GetPlayerHitSpawnPosition();
            effect.transform.position = spawnPosition;
            
            if (effect != null)
            {
                effect.Play();
                m_ActiveEffects.Add(effect);
                
                StartCoroutine(ReturnEffectToPool(effect, m_PlayerNormalHitPool));
            }
        }
        
        public void PlayAINormalHitEffect()
        {
            if (m_AINormalHitPool.Count == 0)
            {
                return;
            }
            
            ParticleSystem effect = m_AINormalHitPool.Dequeue();
            effect.gameObject.SetActive(true);
            
            Vector3 spawnPosition = GetAIHitSpawnPosition();
            effect.transform.position = spawnPosition;
            
            if (effect != null)
            {
                effect.Play();
                m_ActiveEffects.Add(effect);
                
                StartCoroutine(ReturnEffectToPool(effect, m_AINormalHitPool));
            }
        }

        public void PlaySkinChangeEffect()
        {
            if (m_SkinChangePool.Count == 0)
            {
                return;
            }
            
            ParticleSystem effect = m_SkinChangePool.Dequeue();
            effect.gameObject.SetActive(true);
            
            Vector3 spawnPosition = GetSkinChangeSpawnPosition();
            effect.transform.position = spawnPosition;
            
            if (effect != null)
            {
                effect.Play();
                m_ActiveEffects.Add(effect);
                StartCoroutine(ReturnEffectToPool(effect, m_SkinChangePool));
            }
        }

        public void PlayPlayerShieldEffect()
        {
            if (m_PlayerShieldPool.Count == 0)
            {
                return;
            }

            ParticleSystem effect = m_PlayerShieldPool.Dequeue();
            effect.gameObject.SetActive(true);

            Vector3 spawnPosition = GetPlayerShieldSpawnPosition();
            effect.transform.position = spawnPosition;
            effect.transform.localScale = Vector3.zero;

            if (effect != null)
            {
                effect.Play();
                m_ActiveEffects.Add(effect);
                effect.transform.DOScale(Vector3.one * m_ShieldTargetScale, m_ShieldScaleInDuration)
                    .SetEase(m_ShieldEaseIn);
                StartCoroutine(AutoReturnShield(effect));
            }
        }

        private IEnumerator AutoReturnShield(ParticleSystem effect)
        {
            float waitTime = Mathf.Max(0f, m_EffectDuration - m_ShieldScaleOutDuration);
            yield return new WaitForSeconds(waitTime);
            
            if (effect != null)
            {
                Tween t = effect.transform.DOScale(Vector3.zero, m_ShieldScaleOutDuration)
                    .SetEase(m_ShieldEaseOut);
                yield return t.WaitForCompletion();
            }
            
            if (effect != null)
            {
                effect.Stop();
                effect.gameObject.SetActive(false);
                m_ActiveEffects.Remove(effect);
                m_PlayerShieldPool.Enqueue(effect);
            }
        }

        public void StartPlayerShield()
        {
            if (m_ActivePlayerShieldEffect != null)
            {
                return;
            }

            if (m_PlayerShieldPool.Count == 0)
            {
                return;
            }

            var effect = m_PlayerShieldPool.Dequeue();
            m_ActivePlayerShieldEffect = effect;
            effect.gameObject.SetActive(true);
            effect.transform.position = GetPlayerShieldSpawnPosition();
            effect.transform.localScale = Vector3.zero;
            effect.Play();
            effect.transform.DOScale(Vector3.one * m_ShieldTargetScale, m_ShieldScaleInDuration)
                .SetEase(m_ShieldEaseIn);
        }

        public void StopPlayerShield()
        {
            if (m_ActivePlayerShieldEffect == null)
            {
                return;
            }

            var effect = m_ActivePlayerShieldEffect;
            m_ActivePlayerShieldEffect = null;

            if (effect != null)
            {
                Tween t = effect.transform.DOScale(Vector3.zero, m_ShieldScaleOutDuration)
                    .SetEase(m_ShieldEaseOut);
                t.OnComplete(() =>
                {
                    if (effect != null)
                    {
                        effect.Stop();
                        effect.gameObject.SetActive(false);
                        m_PlayerShieldPool.Enqueue(effect);
                    }
                });
            }
        }

        public bool HasActivePlayerShield()
        {
            return m_ActivePlayerShieldEffect != null;
        }

        public void BreakPlayerShield()
        {
            // Stop the persistent shield instantly and return it to pool
            if (m_ActivePlayerShieldEffect != null)
            {
                var shield = m_ActivePlayerShieldEffect;
                m_ActivePlayerShieldEffect = null;
                if (shield != null)
                {
                    DOTween.Kill(shield.transform);
                    shield.Stop();
                    shield.gameObject.SetActive(false);
                    m_PlayerShieldPool.Enqueue(shield);
                }
            }

            // Play shield-death burst at the same spawn position
            if (m_PlayerShieldDeathPool.Count > 0)
            {
                var deathFx = m_PlayerShieldDeathPool.Dequeue();
                if (deathFx != null)
                {
                    deathFx.gameObject.SetActive(true);
                    deathFx.transform.position = GetPlayerShieldSpawnPosition();
                    deathFx.transform.localScale = Vector3.one; 
                    deathFx.Play();
                    StartCoroutine(ReturnEffectToPool(deathFx, m_PlayerShieldDeathPool));
                }
            }
        }
        
        public void PlayEffectAtPosition(ParticleSystem effectPrefab, Vector3 position)
        {
            if (effectPrefab == null)
            {
                return;
            }
            
            ParticleSystem effect = Instantiate(effectPrefab, position + m_EffectOffset, Quaternion.identity);
            if (effect != null)
            {
                effect.Play();
                m_ActiveEffects.Add(effect);
                
                StartCoroutine(DestroyEffectAfterDuration(effect));
            }
        }
        
        private IEnumerator ReturnEffectToPool(ParticleSystem effect, Queue<ParticleSystem> pool)
        {
            yield return new WaitForSeconds(m_EffectDuration);
            
            if (effect != null)
            {
                effect.Stop();
                effect.gameObject.SetActive(false);
                m_ActiveEffects.Remove(effect);
                pool.Enqueue(effect);
            }
        }
        
        private IEnumerator DestroyEffectAfterDuration(ParticleSystem effect)
        {
            yield return new WaitForSeconds(m_EffectDuration);
            
            if (effect != null)
            {
                m_ActiveEffects.Remove(effect);
                Destroy(effect.gameObject);
            }
        }
        
        public void ClearAllEffects()
        {
            foreach (var effect in m_ActiveEffects)
            {
                if (effect != null)
                {
                    effect.Stop();
                    effect.gameObject.SetActive(false);
                }
            }
            
            m_ActiveEffects.Clear();
            
            while (m_HealthUpgradePool.Count > 0)
            {
                var effect = m_HealthUpgradePool.Dequeue();
                if (effect != null) Destroy(effect.gameObject);
            }
            
            while (m_PowerUpgradePool.Count > 0)
            {
                var effect = m_PowerUpgradePool.Dequeue();
                if (effect != null) Destroy(effect.gameObject);
            }
            
            while (m_PlayerHitPool.Count > 0)
            {
                var effect = m_PlayerHitPool.Dequeue();
                if (effect != null) Destroy(effect.gameObject);
            }
            
            while (m_AIHitPool.Count > 0)
            {
                var effect = m_AIHitPool.Dequeue();
                if (effect != null) Destroy(effect.gameObject);
            }
            
            while (m_PlayerLastHitPool.Count > 0)
            {
                var effect = m_PlayerLastHitPool.Dequeue();
                if (effect != null) Destroy(effect.gameObject);
            }
            
            while (m_AILastHitPool.Count > 0)
            {
                var effect = m_AILastHitPool.Dequeue();
                if (effect != null) Destroy(effect.gameObject);
            }
            
            while (m_PlayerNormalHitPool.Count > 0)
            {
                var effect = m_PlayerNormalHitPool.Dequeue();
                if (effect != null) Destroy(effect.gameObject);
            }
            
            while (m_AINormalHitPool.Count > 0)
            {
                var effect = m_AINormalHitPool.Dequeue();
                if (effect != null) Destroy(effect.gameObject);
            }
            
            while (m_PlayerShieldPool.Count > 0)
            {
                var effect = m_PlayerShieldPool.Dequeue();
                if (effect != null) Destroy(effect.gameObject);
            }

            while (m_PlayerShieldDeathPool.Count > 0)
            {
                var effect = m_PlayerShieldDeathPool.Dequeue();
                if (effect != null) Destroy(effect.gameObject);
            }

            while (m_SkinChangePool.Count > 0)
            {
                var effect = m_SkinChangePool.Dequeue();
                if (effect != null) Destroy(effect.gameObject);
            }
        }
        
        public void SetEffectOffset(Vector3 offset)
        {
            m_EffectOffset = offset;
        }
        
        public void SetEffectDuration(float duration)
        {
            m_EffectDuration = duration;
        }
        
        private Vector3 GetSkinChangeSpawnPosition()
        {
            if (m_SkinChangeSpawnPoint != null)
            {
                return m_SkinChangeSpawnPoint.position + m_EffectOffset;
            }
            
            // Fallback to player hit spawn if available
            if (m_PlayerHitSpawnPoint != null)
            {
                return m_PlayerHitSpawnPoint.position + m_EffectOffset;
            }
            
            return Vector3.zero + m_EffectOffset;
        }

        private Vector3 GetHealthUpgradeSpawnPosition()
        {
            if (m_HealthUpgradeSpawnPoint != null)
            {
                return m_HealthUpgradeSpawnPoint.position + m_EffectOffset;
            }
            
            return Vector3.zero + m_EffectOffset;
        }
        
        private Vector3 GetPowerUpgradeSpawnPosition()
        {
            if (m_PowerUpgradeSpawnPoint != null)
            {
                return m_PowerUpgradeSpawnPoint.position + m_EffectOffset;
            }
            
            return Vector3.zero + m_EffectOffset;
        }
        
        private Vector3 GetPlayerHitSpawnPosition()
        {
            if (m_PlayerHitSpawnPoint != null)
            {
                return m_PlayerHitSpawnPoint.position + m_EffectOffset;
            }
            
            return Vector3.zero + m_EffectOffset;
        }
        
        private Vector3 GetAIHitSpawnPosition()
        {
            if (m_AIHitSpawnPoint != null)
            {
                return m_AIHitSpawnPoint.position + m_EffectOffset;
            }
            
            return Vector3.zero + m_EffectOffset;
        }
        
        private Vector3 GetPlayerShieldSpawnPosition()
        {
            if (m_PlayerShieldSpawnPoint != null)
            {
                return m_PlayerShieldSpawnPoint.position + m_EffectOffset;
            }
            return Vector3.zero + m_EffectOffset;
        }
        
        public void SetHealthUpgradeSpawnPoint(Transform spawnPoint)
        {
            m_HealthUpgradeSpawnPoint = spawnPoint;
        }
        
        public void SetPowerUpgradeSpawnPoint(Transform spawnPoint)
        {
            m_PowerUpgradeSpawnPoint = spawnPoint;
        }
        
        public void SetPlayerHitSpawnPoint(Transform spawnPoint)
        {
            m_PlayerHitSpawnPoint = spawnPoint;
        }
        
        public void SetAIHitSpawnPoint(Transform spawnPoint)
        {
            m_AIHitSpawnPoint = spawnPoint;
        }

        public void SetSkinChangeSpawnPoint(Transform spawnPoint)
        {
            m_SkinChangeSpawnPoint = spawnPoint;
        }
    }
}
