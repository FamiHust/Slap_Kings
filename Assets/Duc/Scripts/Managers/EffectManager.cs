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
        [SerializeField] private int m_CombatEffectPoolSize = 10;
        
        [Header("Effect Spawn Positions")]
        [SerializeField] private Transform m_HealthUpgradeSpawnPoint;
        [SerializeField] private Transform m_PowerUpgradeSpawnPoint;
        [SerializeField] private Transform m_PlayerHitSpawnPoint;
        [SerializeField] private Transform m_AIHitSpawnPoint;
        
        [Header("Effect Settings")]
        [SerializeField] private float m_EffectDuration = 2f;
        [SerializeField] private Vector3 m_EffectOffset = Vector3.up * 2f;
        
        private Queue<ParticleSystem> m_HealthUpgradePool = new Queue<ParticleSystem>();
        private Queue<ParticleSystem> m_PowerUpgradePool = new Queue<ParticleSystem>();
        private Queue<ParticleSystem> m_PlayerHitPool = new Queue<ParticleSystem>();
        private Queue<ParticleSystem> m_AIHitPool = new Queue<ParticleSystem>();
        private List<ParticleSystem> m_ActiveEffects = new List<ParticleSystem>();
        
        protected override void OnInitialize()
        {
            InitializeEffectPools();
            SubscribeToEvents();
        }
        
        protected override void OnCleanup()
        {
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
            
            effect.Play();
            m_ActiveEffects.Add(effect);
            
            StartCoroutine(ReturnEffectToPool(effect, m_PowerUpgradePool));
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
            
            effect.Play();
            m_ActiveEffects.Add(effect);
            
            this.PublishEvent(new CameraShakeEvent(3f, 0.2f, true));
            
            StartCoroutine(ReturnEffectToPool(effect, m_PlayerHitPool));
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
            
            effect.Play();
            m_ActiveEffects.Add(effect);
            
            this.PublishEvent(new CameraShakeEvent(2.5f, 0.18f, true));
            
            StartCoroutine(ReturnEffectToPool(effect, m_AIHitPool));
        }
        
        public void PlayEffectAtPosition(ParticleSystem effectPrefab, Vector3 position)
        {
            if (effectPrefab == null)
            {
                return;
            }
            
            ParticleSystem effect = Instantiate(effectPrefab, position + m_EffectOffset, Quaternion.identity);
            effect.Play();
            m_ActiveEffects.Add(effect);
            
            StartCoroutine(DestroyEffectAfterDuration(effect));
        }
        
        private IEnumerator ReturnEffectToPool(ParticleSystem effect, Queue<ParticleSystem> pool)
        {
            yield return new WaitForSeconds(m_EffectDuration);
            
            effect.Stop();
            effect.gameObject.SetActive(false);
            m_ActiveEffects.Remove(effect);
            pool.Enqueue(effect);
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
        }
        
        public void SetEffectOffset(Vector3 offset)
        {
            m_EffectOffset = offset;
        }
        
        public void SetEffectDuration(float duration)
        {
            m_EffectDuration = duration;
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
    }
}
