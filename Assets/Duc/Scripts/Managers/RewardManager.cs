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
            
            // Store original target position
            if (m_TargetTransform != null)
            {
                m_OriginalTargetPosition = m_TargetTransform.position;
            }
            
            // Ensure all reward coins are initially disabled
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
        
        /// <summary>
        /// Show reward coins and fly them to target
        /// </summary>
        /// <param name="coinCount">Number of coins to show (max is array length)</param>
        public void ShowRewardCoins(int coinCount = 10)
        {
            if (m_RewardCoins == null || m_RewardCoins.Length == 0)
            {
                Debug.LogWarning("RewardManager: No reward coins assigned!");
                return;
            }
            
            if (m_TargetTransform == null)
            {
                Debug.LogWarning("RewardManager: No target transform assigned!");
                return;
            }
            
            // Clear any active coins
            ClearActiveCoins();
            
            // Limit coin count to available coins
            int coinsToShow = Mathf.Min(coinCount, m_RewardCoins.Length);
            
            // Activate and animate coins
            for (int i = 0; i < coinsToShow; i++)
            {
                if (m_RewardCoins[i] != null)
                {
                    StartCoroutine(AnimateSingleCoin(m_RewardCoins[i], i * m_StaggerDelay));
                }
            }
        }
        
        /// <summary>
        /// Show victory reward coins
        /// </summary>
        public void ShowVictoryReward()
        {
            ShowRewardCoins(10);
        }
        
        /// <summary>
        /// Show defeat reward coins (fewer coins)
        /// </summary>
        public void ShowDefeatReward()
        {
            ShowRewardCoins(5);
        }
        
        /// <summary>
        /// Show reward coins from specific panel position
        /// </summary>
        /// <param name="coinCount">Number of coins to show</param>
        /// <param name="startPosition">Starting position for coins</param>
        public void ShowRewardCoinsFromPosition(int coinCount, Vector3 startPosition)
        {
            if (m_RewardCoins == null || m_RewardCoins.Length == 0)
            {
                Debug.LogWarning("RewardManager: No reward coins assigned!");
                return;
            }
            
            if (m_TargetTransform == null)
            {
                Debug.LogWarning("RewardManager: No target transform assigned!");
                return;
            }
            
            // Clear any active coins
            ClearActiveCoins();
            
            // Limit coin count to available coins
            int coinsToShow = Mathf.Min(coinCount, m_RewardCoins.Length);
            
            // Activate and animate coins from specific position
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
            // Wait for stagger delay
            yield return new WaitForSeconds(delay);
            
            // Add to active coins list
            m_ActiveCoins.Add(coin);
            
            // Activate coin
            coin.SetActive(true);
            
            // Reset scale and position
            coin.transform.localScale = Vector3.zero;
            
            // Scale up animation
            coin.transform.DOScale(Vector3.one, m_ScaleUpDuration)
                .SetEase(Ease.OutBack);
            
            // Wait a bit before flying
            yield return new WaitForSeconds(0.5f);
            
            // Fly to target
            Vector3 targetPos = m_TargetTransform.position;
            coin.transform.DOMove(targetPos, m_FlyDuration)
                .SetEase(m_FlyEase)
                .OnComplete(() => {
                    // Scale down and deactivate when reached target
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
            // Wait for stagger delay
            yield return new WaitForSeconds(delay);
            
            // Add to active coins list
            m_ActiveCoins.Add(coin);
            
            // Activate coin and set position with some random offset
            coin.SetActive(true);
            
            // Add random offset to spread coins around the panel position
            Vector3 randomOffset = new Vector3(
                Random.Range(-m_CoinSpreadX, m_CoinSpreadX),  // X offset
                Random.Range(-m_CoinSpreadY, m_CoinSpreadY),  // Y offset
                0f                                            // Z offset
            );
            coin.transform.position = startPosition + randomOffset;
            coin.transform.localScale = Vector3.zero;
            
            
            // Scale up animation
            coin.transform.DOScale(Vector3.one, m_ScaleUpDuration)
                .SetEase(Ease.OutBack);
            
            // Wait a bit before flying
            yield return new WaitForSeconds(0.5f);
            
            // Fly to target
            Vector3 targetPos = m_TargetTransform.position;
            coin.transform.DOMove(targetPos, m_FlyDuration)
                .SetEase(m_FlyEase)
                .OnComplete(() => {
                    // Scale down and deactivate when reached target
                    coin.transform.DOScale(Vector3.zero, m_ScaleDownDuration)
                        .SetEase(Ease.InBack)
                        .OnComplete(() => {
                            coin.SetActive(false);
                            m_ActiveCoins.Remove(coin);
                        });
                });
        }
        
        /// <summary>
        /// Clear all active coins immediately
        /// </summary>
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
        
        /// <summary>
        /// Set target transform for coins to fly to
        /// </summary>
        public void SetTargetTransform(Transform target)
        {
            m_TargetTransform = target;
            if (target != null)
            {
                m_OriginalTargetPosition = target.position;
            }
        }
        
        /// <summary>
        /// Update target position (useful if target moves)
        /// </summary>
        public void UpdateTargetPosition()
        {
            if (m_TargetTransform != null)
            {
                m_OriginalTargetPosition = m_TargetTransform.position;
            }
        }
        
        /// <summary>
        /// Get current active coin count
        /// </summary>
        public int GetActiveCoinCount()
        {
            return m_ActiveCoins.Count;
        }
        
        /// <summary>
        /// Check if any coins are currently animating
        /// </summary>
        public bool IsAnimating()
        {
            return m_ActiveCoins.Count > 0;
        }
        
        protected override void OnInitialize()
        {
            // Initialize reward manager
            if (m_TargetTransform == null)
            {
                // Try to find coin display as default target
                var coinDisplay = FindObjectOfType<CoinDisplay>();
                if (coinDisplay != null)
                {
                    m_TargetTransform = coinDisplay.transform;
                }
            }
        }
        
        protected override void OnCleanup()
        {
            // Cleanup reward manager
            ClearActiveCoins();
        }
        
        protected override void OnDestroy()
        {
            // Kill all DOTween animations
            foreach (var coin in m_RewardCoins)
            {
                if (coin != null)
                {
                    coin.transform.DOKill();
                }
            }
            
            base.OnDestroy();
        }
        
        [ContextMenu("Test Victory Reward")]
        public void TestVictoryReward()
        {
            ShowVictoryReward();
        }
        
        [ContextMenu("Test Defeat Reward")]
        public void TestDefeatReward()
        {
            ShowDefeatReward();
        }
        
        [ContextMenu("Clear All Coins")]
        public void TestClearCoins()
        {
            ClearActiveCoins();
        }
    }
}
