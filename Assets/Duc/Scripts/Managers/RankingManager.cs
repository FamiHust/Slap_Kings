using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Duc
{
    public class RankingManager : SingletonManager<RankingManager>
    {
        [Header("Rank Data")]
        [SerializeField] private RankItemData m_RankData;
        
        [Header("Rank Item UI Prefab")]
        [SerializeField] private GameObject m_RankItemPrefab;
        [SerializeField] private Transform m_RankListContainer;
        [SerializeField] private ScrollRect m_RankScrollRect;
        
        [Header("Medal Icons")]
        [SerializeField] private Sprite[] m_MedalIcons = new Sprite[5];
        [SerializeField] private Image[] m_MedalDisplayImages;
        
        [Header("Settings")]
        private int m_BaseScore = 0;
        private int m_MinVictoryScore = 10;
        private int m_MaxVictoryScore = 50;
        
        private int m_CurrentScore = 0;
        private int m_CurrentVictories = 0;
        private RankItemData.RankInfo m_CurrentRank;
        private int m_PlayerRankPosition = 51; 
        
        public int CurrentScore => m_CurrentScore;
        public int CurrentVictories => m_CurrentVictories;
        public RankItemData.RankInfo CurrentRank => m_CurrentRank;
        
        public System.Action<RankItemData.RankInfo> OnRankChanged;
        public System.Action<int> OnScoreChanged;
        public System.Action<int> OnVictoryCountChanged;
        
        protected override void OnInitialize()
        {
            LoadRankingData();
            InitializeRanking();
            var persistentData = PersistentDataManager.Instance;
            if (persistentData != null)
            {
                persistentData.OnPowerUpgradePurchased -= OnPowerUpgraded;
                persistentData.OnPowerUpgradePurchased += OnPowerUpgraded;
            }
        }
        
        private void Start()
        {
            UpdateMedalDisplay();
            
            if (m_RankData != null && m_RankItemPrefab != null && m_RankListContainer != null)
            {
                CreateRankListUI();
            }
        }
        
        protected override void OnCleanup()
        {
            var persistentData = PersistentDataManager.Instance;
            if (persistentData != null)
            {
                persistentData.OnPowerUpgradePurchased -= OnPowerUpgraded;
            }
            SaveRankingData();
        }

        private void OnPowerUpgraded()
        {
            UpdateScoreFromPowerMeter();
            OnScoreChanged?.Invoke(m_CurrentScore);
            if (m_RankData != null && m_RankItemPrefab != null && m_RankListContainer != null)
            {
                UpdateRankListUI();
            }
            SaveRankingData();
        }
        
        private void LoadRankingData()
        {
            m_CurrentScore = PlayerPrefs.GetInt("PlayerScore", m_BaseScore);
            m_CurrentVictories = PlayerPrefs.GetInt("PlayerVictories", 0);
            m_PlayerRankPosition = PlayerPrefs.GetInt("PlayerRankPosition", 51);
            
            UpdateScoreFromPowerMeter();
        }
        
        private void UpdateScoreFromPowerMeter()
        {
            var powerMeter = PowerMeter.Get();
            if (powerMeter != null)
            {
                int maxPower = powerMeter.GetMaxPower();
                m_CurrentScore = maxPower;
            }
        }
        
        private void SaveRankingData()
        {
            PlayerPrefs.SetInt("PlayerScore", m_CurrentScore);
            PlayerPrefs.SetInt("PlayerVictories", m_CurrentVictories);
            PlayerPrefs.SetInt("PlayerRankPosition", m_PlayerRankPosition);
            PlayerPrefs.Save();
        }
        
        private void InitializeRanking()
        {
            if (m_RankData == null)
            {
                return;
            }
            
            m_CurrentRank = m_RankData.GetCurrentRank(m_CurrentVictories, m_CurrentScore);
        }
        
        public void OnPlayerVictory()
        {
            var powerMeter = PowerMeter.Get();
            if (powerMeter != null)
            {
                int maxPower = powerMeter.GetMaxPower();
                SetScore(maxPower);
            }
            
            m_CurrentVictories++;
            OnVictoryCountChanged?.Invoke(m_CurrentVictories);
            
            if (m_PlayerRankPosition > 1)
            {
                m_PlayerRankPosition--;
            }
            
            CheckRankAdvancement();
            
            SaveRankingData();
        }
        
        public void AddScore(int score)
        {
            m_CurrentScore += score;
            OnScoreChanged?.Invoke(m_CurrentScore);
            
            if (m_RankData != null && m_RankItemPrefab != null && m_RankListContainer != null)
            {
                UpdateRankListUI();
            }
            
            SaveRankingData();
        }
        
        public void SetScore(int score)
        {
            m_CurrentScore = score;
            OnScoreChanged?.Invoke(m_CurrentScore);
            CheckRankAdvancement();
            
            if (m_RankData != null && m_RankItemPrefab != null && m_RankListContainer != null)
            {
                UpdateRankListUI();
            }
            
            SaveRankingData();
        }
        
        public void SetVictoryCount(int victories)
        {
            m_CurrentVictories = victories;
            OnVictoryCountChanged?.Invoke(m_CurrentVictories);
            CheckRankAdvancement();
            SaveRankingData();
        }
        
        private void CheckRankAdvancement()
        {
            if (m_RankData == null) return;
            
            RankItemData.RankInfo newRank = m_RankData.GetCurrentRank(m_CurrentVictories, m_CurrentScore);
            
            if (newRank != null && (m_CurrentRank == null || newRank.rankOrder != m_CurrentRank.rankOrder))
            {
                m_CurrentRank = newRank;
                OnRankChanged?.Invoke(m_CurrentRank);
                
                UpdateMedalDisplay();
                UpdateRankListUI();
            }
        }
        
        private void UpdateRankListUI()
        {
            CreateRankListUI();
        }
        
        public RankItemData.RankInfo GetRankInfo(int order)
        {
            return m_RankData != null ? m_RankData.GetRankByOrder(order) : null;
        }
        
        public RankItemData.RankInfo GetNextRank()
        {
            return m_RankData != null ? m_RankData.GetNextRank(m_CurrentVictories, m_CurrentScore) : null;
        }
        
        public void ResetRanking()
        {
            m_CurrentScore = m_BaseScore;
            m_CurrentVictories = 0;
            m_CurrentRank = null;
            
            InitializeRanking();
            
            OnScoreChanged?.Invoke(m_CurrentScore);
            OnVictoryCountChanged?.Invoke(m_CurrentVictories);
            OnRankChanged?.Invoke(m_CurrentRank);
            
            SaveRankingData();
        }
        
        public void CreateRankListUI()
        {
            if (m_RankData == null || m_RankItemPrefab == null || m_RankListContainer == null) return;
            
            foreach (Transform child in m_RankListContainer)
            {
                Destroy(child.gameObject);
            }
            
            var dataManager = DataManager.Get();
            if (dataManager == null || dataManager.AIStats == null || dataManager.AIStats.appearanceData == null)
            {
                return;
            }
            
            var allAIs = dataManager.AIStats.appearanceData.AppearanceSets;
            
            List<RankingItem> rankingItems = new List<RankingItem>();
            
            for (int i = 0; i < allAIs.Count; i++)
            {
                var ai = allAIs[i];
                var rankInfo = GetRankInfoForAI(i + 1);
                
                string displayName = !string.IsNullOrEmpty(ai.rankDisplayName) ? ai.rankDisplayName : ai.setName;
                int aiLevel = ai.startLevel;
                int aiScore = dataManager != null ? dataManager.GetAIMaxDamage(aiLevel) : 1000;
                
                rankingItems.Add(new RankingItem
                {
                    ai = ai,
                    rankInfo = rankInfo,
                    originalIndex = i,
                    score = aiScore,
                    displayName = displayName,
                    isPlayer = false
                });
            }
            
            rankingItems.Sort((a, b) => b.ai.startLevel.CompareTo(a.ai.startLevel));
            
            if (m_CurrentRank != null)
            {
                int playerIndex = m_PlayerRankPosition - 1;
                if (playerIndex >= 0 && playerIndex <= rankingItems.Count)
                {
                    rankingItems.Insert(playerIndex, new RankingItem
                    {
                        ai = null,
                        rankInfo = m_CurrentRank,
                        originalIndex = -1,
                        score = m_CurrentScore,
                        displayName = "YOU",
                        isPlayer = true
                    });
                }
            }

            for (int i = 0; i < rankingItems.Count; i++)
            {
                var item = rankingItems[i];
                GameObject rankItemObj = Instantiate(m_RankItemPrefab, m_RankListContainer);
                RankItemUI rankItemUI = rankItemObj.GetComponent<RankItemUI>();
                if (rankItemUI != null)
                {
                    int displayPosition = i + 1; 
                    int aiLevel = item.originalIndex + 1;
                    bool isBossLevel = dataManager != null && dataManager.IsBossLevel(aiLevel);
                    rankItemUI.Initialize(item.rankInfo, item.displayName, item.score, false, item.isPlayer, displayPosition, m_CurrentScore, isBossLevel, m_PlayerRankPosition);
                }
            }
            
            StartCoroutine(ScrollToPlayerRank());
        }
        
        private IEnumerator ScrollToBottomAfterLayout()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            
            if (m_RankScrollRect != null)
            {
                m_RankScrollRect.verticalNormalizedPosition = 0f;
            }
        }
        
        private IEnumerator ScrollToPlayerRank()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            
            if (m_RankScrollRect != null && m_RankListContainer != null)
            {
                int totalItems = m_RankListContainer.childCount;
                if (totalItems > 0 && m_PlayerRankPosition > 0)
                {
                    float normalizedPosition = 1f - ((float)m_PlayerRankPosition / totalItems);
                    normalizedPosition = Mathf.Clamp01(normalizedPosition);
                    
                    m_RankScrollRect.verticalNormalizedPosition = normalizedPosition;
                }
                else
                {
                    m_RankScrollRect.verticalNormalizedPosition = 0f;
                }
            }
        }
        
        private class RankingItem
        {
            public AIAppearanceData.AppearanceSet ai;
            public RankItemData.RankInfo rankInfo;
            public int originalIndex;
            public int score;
            public string displayName;
            public bool isPlayer;
        }
        
        private string GetAINameForRank(int rankOrder)
        {
            var dataManager = DataManager.Get();
            if (dataManager != null && dataManager.AIStats != null && dataManager.AIStats.appearanceData != null)
            {
                int levelForRank = rankOrder;
                
                var appearance = dataManager.AIStats.appearanceData.GetAppearanceForLevel(levelForRank);
                if (appearance != null && !string.IsNullOrEmpty(appearance.setName))
                {
                    return appearance.setName;
                }
            }
            
            var aiAppearanceManager = FindObjectOfType<AIAppearanceManager>();
            if (aiAppearanceManager != null && aiAppearanceManager.CurrentAppearance != null)
            {
                return aiAppearanceManager.CurrentAppearance.setName;
            }
            return $"AI Rank {rankOrder}";
        }
        
        private string GetCurrentAIName()
        {
            var aiAppearanceManager = FindObjectOfType<AIAppearanceManager>();
            if (aiAppearanceManager != null && aiAppearanceManager.CurrentAppearance != null)
            {
                return aiAppearanceManager.CurrentAppearance.setName;
            }
            
            var dataManager = DataManager.Get();
            if (dataManager != null && dataManager.AIStats != null && dataManager.AIStats.appearanceData != null)
            {
                int currentLevel = GetCurrentLevel();
                var appearance = dataManager.AIStats.appearanceData.GetAppearanceForLevel(currentLevel);
                if (appearance != null && !string.IsNullOrEmpty(appearance.setName))
                {
                    return appearance.setName;
                }
            }
            return "Unknown AI";
        }
        
        private int GetCurrentLevel()
        {
            var persistentData = PersistentDataManager.Instance;
            return persistentData != null ? persistentData.GetLevelCount() : 1;
        }
        
        private RankItemData.RankInfo GetRankInfoForAI(int aiPosition)
        {
            if (aiPosition <= m_RankData.Ranks.Length)
            {
                return m_RankData.GetRankByOrder(aiPosition);
            }
            
            return m_RankData.GetRankByOrder(m_RankData.Ranks.Length);
        }
        
        private int GetAIScoreForRank(int rankOrder)
        {
            var dataManager = DataManager.Get();
            if (dataManager != null && dataManager.AIStats != null && dataManager.AIStats.appearanceData != null)
            {
                int aiIndex = rankOrder - 1;
                var allAIs = dataManager.AIStats.appearanceData.AppearanceSets;
                
                if (aiIndex >= 0 && aiIndex < allAIs.Count)
                {
                    var ai = allAIs[aiIndex];
                    return dataManager != null ? dataManager.GetAIMaxDamage(ai.startLevel) : 1000;
                }
            }
            
            return 1000;
        }
        
        public void ScrollRankingToBottom()
        {
            StartCoroutine(ScrollToBottomAfterLayout());
        }

        public void ScrollRankingToPlayer()
        {
            StartCoroutine(ScrollToPlayerRank());
        }
        
        public Sprite GetMedalIcon(int rankOrder)
        {
            if (m_MedalIcons != null && rankOrder >= 1 && rankOrder <= m_MedalIcons.Length)
            {
                return m_MedalIcons[rankOrder - 1];
            }
            return null;
        }
        
        private void UpdateMedalDisplay()
        {
            if (m_MedalDisplayImages != null && m_MedalDisplayImages.Length > 0 && m_CurrentRank != null)
            {
                Sprite medalIcon = GetMedalIcon(m_CurrentRank.rankOrder);
                
                foreach (Image medalImage in m_MedalDisplayImages)
                {
                    if (medalImage != null)
                    {
                        if (medalIcon != null)
                        {
                            medalImage.sprite = medalIcon;
                            medalImage.enabled = true;
                        }
                        else
                        {
                            medalImage.enabled = false;
                        }
                    }
                }
            }
        }
    }
}
