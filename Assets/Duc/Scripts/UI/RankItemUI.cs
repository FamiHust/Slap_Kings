using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Duc
{
    public class RankItemUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI m_RankOrderText;
        [SerializeField] private TextMeshProUGUI m_RankNameText;
        [SerializeField] private TextMeshProUGUI m_ScoreText;
        [SerializeField] private Image m_BackgroundImage;
        [SerializeField] private Image m_BossIconImage;
        
        [Header("Background Colors")]
        [SerializeField] private Color m_ActiveRankBgColor = new Color(1f, 0.8f, 0f, 0.3f);
        [SerializeField] private Color m_InactiveRankBgColor = new Color(1f, 1f, 1f, 0.1f);
        [SerializeField] private Color m_LockedRankBgColor = new Color(0.5f, 0.5f, 0.5f, 0.2f);
        [SerializeField] private Color m_BossLevelBgColor = new Color(1f, 0f, 0f, 0.3f);
        
        private RankItemData.RankInfo m_RankInfo;
        private bool m_IsLocked = false;
        private bool m_IsCurrentRank = false;
        private string m_DisplayName = "";
        private int m_CurrentScore = 0;
        private int m_DisplayPosition = 0;
        private int m_PlayerScore = 0;
        private bool m_IsBossLevel = false;
        private int m_PlayerRankPosition = 0;
        
        public void Initialize(RankItemData.RankInfo rankInfo, string displayName, int currentScore, bool isLocked = false, bool isCurrentRank = false, int displayPosition = 0, int playerScore = 0, bool isBossLevel = false, int playerRankPosition = 0)
        {
            m_RankInfo = rankInfo;
            m_DisplayName = displayName;
            m_CurrentScore = currentScore;
            m_IsLocked = isLocked;
            m_IsCurrentRank = isCurrentRank;
            m_DisplayPosition = displayPosition;
            m_PlayerScore = playerScore;
            m_IsBossLevel = isBossLevel;
            m_PlayerRankPosition = playerRankPosition;
            
            UpdateDisplay();
        }
        
        private void UpdateDisplay()
        {
            if (m_RankInfo == null) return;
            
            if (m_RankOrderText != null)
            {
                int orderToShow = m_DisplayPosition > 0 ? m_DisplayPosition : m_RankInfo.rankOrder;
                m_RankOrderText.text = orderToShow.ToString();
            }
            
            if (m_RankNameText != null)
            {
                m_RankNameText.text = m_DisplayName;
            }
            
            if (m_ScoreText != null)
            {
                m_ScoreText.text = m_CurrentScore.ToString();
            }
            
            // Update boss icon visibility
            if (m_BossIconImage != null)
            {
                m_BossIconImage.enabled = m_IsBossLevel;
            }

            UpdateColors();
        }
        
        private void UpdateColors()
        {
            Color targetBgColor = m_InactiveRankBgColor;
            
            if (m_IsCurrentRank)
            {
                targetBgColor = m_ActiveRankBgColor;
            }
            // Lock ranks below player's rank (higher display position number = lower rank)
            else if (m_PlayerRankPosition > 0 && m_DisplayPosition > m_PlayerRankPosition)
            {
                targetBgColor = m_LockedRankBgColor;
            }
            else if (m_IsBossLevel)
            {
                targetBgColor = m_BossLevelBgColor;
            }
            
            if (m_BackgroundImage != null)
            {
                m_BackgroundImage.color = targetBgColor;
            }
        }
        
        public void SetCurrentRank(bool isCurrent)
        {
            m_IsCurrentRank = isCurrent;
            m_IsLocked = false;
            UpdateColors();
        }
        
        public void SetLocked(bool isLocked)
        {
            m_IsLocked = isLocked;
            if (isLocked)
            {
                m_IsCurrentRank = false;
            }
            UpdateColors();
        }
        
        public int GetRankOrder()
        {
            return m_RankInfo != null ? m_RankInfo.rankOrder : 0;
        }
    }
}


