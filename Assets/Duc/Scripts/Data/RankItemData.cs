using UnityEngine;

namespace Duc
{
    [CreateAssetMenu(fileName = "RankItemData", menuName = "Game/Rank Item Data")]
    public class RankItemData : ScriptableObject
    {
        [System.Serializable]
        public class RankInfo
        {
            [Header("Rank Information")]
            public int rankOrder;
            public string rankName;
            
            [Header("Requirements")]
            public int requiredVictories; 
        }
        
        [Header("Rank Configuration")]
        [SerializeField] private RankInfo[] m_Ranks = new RankInfo[]
        {
            new RankInfo { rankOrder = 1, rankName = "Metal", requiredVictories = 0 },
            new RankInfo { rankOrder = 2, rankName = "Bronze", requiredVictories = 5 },
            new RankInfo { rankOrder = 3, rankName = "Silver", requiredVictories = 15 },
            new RankInfo { rankOrder = 4, rankName = "Gold", requiredVictories = 30 },
            new RankInfo { rankOrder = 5, rankName = "Diamond", requiredVictories = 50 }
        };
        
        public RankInfo[] Ranks => m_Ranks;
        
        public RankInfo GetRankByOrder(int order)
        {
            foreach (var rank in m_Ranks)
            {
                if (rank.rankOrder == order)
                {
                    return rank;
                }
            }
            return null;
        }
        
        public RankInfo GetCurrentRank(int victories, int score)
        {
            RankInfo currentRank = m_Ranks[0];
            
            foreach (var rank in m_Ranks)
            {
                if (victories >= rank.requiredVictories)
                {
                    currentRank = rank;
                }
                else
                {
                    break;
                }
            }
            
            return currentRank;
        }
        
        public RankInfo GetNextRank(int victories, int score)
        {
            RankInfo currentRank = GetCurrentRank(victories, score);
            
            foreach (var rank in m_Ranks)
            {
                if (rank.rankOrder > currentRank.rankOrder)
                {
                    return rank;
                }
            }
            
            return currentRank;
        }
        
        public int GetTotalVictoriesNeeded()
        {
            int maxVictories = 0;
            foreach (var rank in m_Ranks)
            {
                if (rank.requiredVictories > maxVictories)
                {
                    maxVictories = rank.requiredVictories;
                }
            }
            return maxVictories;
        }
    }
}


