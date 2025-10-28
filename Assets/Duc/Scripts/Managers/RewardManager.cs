using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace Duc
{
    public class RewardManager : SingletonManager<RewardManager>
    {
        [Header("Reward Settings")]
        [SerializeField] private GameObject[] m_RewardCoins;
        [SerializeField] private Transform m_TargetTransform;
        [SerializeField] private float m_FlyDuration = 1.5f;
        [SerializeField] private float m_StaggerDelay = 0.1f;
        [SerializeField] private Ease m_FlyEase = Ease.OutQuad;
        
        [Header("Animation Settings")]
        [SerializeField] private float m_ScaleUpDuration = 0.3f;
        [SerializeField] private float m_ScaleDownDuration = 0.2f;
        [SerializeField] private Vector3 m_ScaleUpSize = Vector3.one * 1.2f;
        [SerializeField] private Vector3 m_ScaleDownSize = Vector3.one * 0.8f;
        
        [Header("Coin Spread Settings")]
        [SerializeField] private float m_CoinSpreadX = 50f;
        [SerializeField] private float m_CoinSpreadY = 30f;
        
        private List<GameObject> m_ActiveCoins = new List<GameObject>();
        private Vector3 m_OriginalTargetPosition;
        
        protected override void Awake()
        {
            base.Awake();
            
            if (m_TargetTransform != null)
            {
                m_OriginalTargetPosition = m_TargetTransform.position;
            }
            
            if (m_RewardCoins != null)
            {
                foreach (var coin in m_RewardCoins)
                {
                    if (coin != null)
                    {
                        coin.SetActive(false);
                    }
                }
            }
        }
        
        public void ShowRewardCoins(int coinCount = 10)
        {
            if (m_RewardCoins == null || m_RewardCoins.Length == 0)
            {
                return;
            }
            
            if (m_TargetTransform == null)
            {
                return;
            }
            
            ClearActiveCoins();
            
            int coinsToShow = Mathf.Min(coinCount, m_RewardCoins.Length);
            
            for (int i = 0; i < coinsToShow; i++)
            {
                if (m_RewardCoins[i] != null)
                {
                    StartCoroutine(AnimateSingleCoin(m_RewardCoins[i], i * m_StaggerDelay));
                }
            }
        }

        public void ShowVictoryReward()
        {
            ShowRewardCoins(15);
        }
        
        public void ShowDefeatReward()
        {
            ShowRewardCoins(5);
        }

        public void ShowRewardCoinsFromPosition(int coinCount, Vector3 startPosition)
        {
            if (m_RewardCoins == null || m_RewardCoins.Length == 0)
            {
                return;
            }
            
            if (m_TargetTransform == null)
            {
                return;
            }
            
            ClearActiveCoins();
            
            int coinsToShow = Mathf.Min(coinCount, m_RewardCoins.Length);
            
            for (int i = 0; i < coinsToShow; i++)
            {
                if (m_RewardCoins[i] != null)
                {
                    StartCoroutine(AnimateSingleCoinFromPosition(m_RewardCoins[i], startPosition, i * m_StaggerDelay));
                }
            }
        }
        
        private IEnumerator AnimateSingleCoin(GameObject coin, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            m_ActiveCoins.Add(coin);
            
            coin.SetActive(true);
            var soundManager = SoundManager.Get();
            if (soundManager != null)
            {
                soundManager.PlaySound(SoundManager.SoundType.CoinReward);
            }
            
            coin.transform.localScale = Vector3.zero;
            
            coin.transform.DOScale(Vector3.one, m_ScaleUpDuration)
                .SetEase(Ease.OutBack);
            
            yield return new WaitForSeconds(0.5f);
            
            Vector3 targetPos = m_TargetTransform.position;
            coin.transform.DOMove(targetPos, m_FlyDuration)
                .SetEase(m_FlyEase)
                .OnComplete(() => {
                    var sm = SoundManager.Get();
                    if (sm != null)
                    {
                        sm.PlaySound(SoundManager.SoundType.CoinReward);
                    }
                    coin.transform.DOScale(Vector3.zero, m_ScaleDownDuration)
                        .SetEase(Ease.InBack)
                        .OnComplete(() => {
                            coin.SetActive(false);
                            m_ActiveCoins.Remove(coin);
                        });
                });
        }
        
        private IEnumerator AnimateSingleCoinFromPosition(GameObject coin, Vector3 startPosition, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            m_ActiveCoins.Add(coin);
            
            coin.SetActive(true);
            var soundManager = SoundManager.Get();
            if (soundManager != null)
            {
                soundManager.PlaySound(SoundManager.SoundType.CoinReward);
            }
            
            Vector3 randomOffset = new Vector3(
                Random.Range(-m_CoinSpreadX, m_CoinSpreadX), 
                Random.Range(-m_CoinSpreadY, m_CoinSpreadY),  
                0f                                         
            );
            coin.transform.position = startPosition + randomOffset;
            coin.transform.localScale = Vector3.zero;
            
            
            coin.transform.DOScale(Vector3.one, m_ScaleUpDuration)
                .SetEase(Ease.OutBack);
            
            yield return new WaitForSeconds(0.5f);
            
            Vector3 targetPos = m_TargetTransform.position;
            coin.transform.DOMove(targetPos, m_FlyDuration)
                .SetEase(m_FlyEase)
                .OnComplete(() => {
                    var sm = SoundManager.Get();
                    if (sm != null)
                    {
                        sm.PlaySound(SoundManager.SoundType.CoinReward);
                    }
                    coin.transform.DOScale(Vector3.zero, m_ScaleDownDuration)
                        .SetEase(Ease.InBack)
                        .OnComplete(() => {
                            coin.SetActive(false);
                            m_ActiveCoins.Remove(coin);
                        });
                });
        }

        public void ClearActiveCoins()
        {
            foreach (var coin in m_ActiveCoins)
            {
                if (coin != null)
                {
                    coin.transform.DOKill();
                    coin.SetActive(false);
                }
            }
            m_ActiveCoins.Clear();
        }

        public void SetTargetTransform(Transform target)
        {
            m_TargetTransform = target;
            if (target != null)
            {
                m_OriginalTargetPosition = target.position;
            }
        }

        public void UpdateTargetPosition()
        {
            if (m_TargetTransform != null)
            {
                m_OriginalTargetPosition = m_TargetTransform.position;
            }
        }
 
        public int GetActiveCoinCount()
        {
            return m_ActiveCoins.Count;
        }

        public bool IsAnimating()
        {
            return m_ActiveCoins.Count > 0;
        }
        
        protected override void OnInitialize()
        {
            if (m_TargetTransform == null)
            {
                var coinDisplay = FindObjectOfType<CoinDisplay>();
                if (coinDisplay != null)
                {
                    m_TargetTransform = coinDisplay.transform;
                }
            }
        }
        
        protected override void OnCleanup()
        {
            ClearActiveCoins();
        }
        
        protected override void OnDestroy()
        {
            foreach (var coin in m_RewardCoins)
            {
                if (coin != null)
                {
                    coin.transform.DOKill();
                }
            }
            
            base.OnDestroy();
        }
    }
}
