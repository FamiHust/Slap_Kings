using UnityEngine;
using System.Collections.Generic;

namespace Duc
{
    [CreateAssetMenu(fileName = "PlayerSkinData", menuName = "Game/Player Skin Data")]
    public class PlayerSkinData : ScriptableObject
    {
        [System.Serializable]
        public class SkinSet
        {
            [Header("Skin Info")]
            public int skinId;
            public string skinName;
            
            [Header("Body Mesh")]
            public Mesh bodyMesh; 
            
            [Header("Additional Settings")]
            public bool isUnlocked = true;
            public int unlockCost = 0;
        }
        
        [Header("Skin Configuration")]
        [SerializeField] private List<SkinSet> m_SkinSets = new List<SkinSet>();
        
        [Header("Shared Head Meshes (used by all skins)")]
        [SerializeField] private Mesh m_SharedHeadMesh; 
        [SerializeField] private Mesh m_SharedHeadSlappedMesh; 
        [SerializeField] private Mesh m_SharedHeadVerySlappedMesh;
        
        [Header("Shared Hair Mesh (used by all skins)")]
        [SerializeField] private Mesh m_SharedHairMesh; 
        
        [Header("Default Settings")]
        [SerializeField] private SkinSet m_DefaultSkin;
        [SerializeField] private int m_CurrentSkinId = 0;
        
        public List<SkinSet> SkinSets => m_SkinSets;
        public SkinSet DefaultSkin => m_DefaultSkin;
        public int CurrentSkinId => m_CurrentSkinId;
        
        public Mesh SharedHeadMesh => m_SharedHeadMesh;
        public Mesh SharedHeadSlappedMesh => m_SharedHeadSlappedMesh;
        public Mesh SharedHeadVerySlappedMesh => m_SharedHeadVerySlappedMesh;
        public Mesh SharedHairMesh => m_SharedHairMesh;

        public SkinSet GetSkinById(int skinId)
        {
            foreach (var skin in m_SkinSets)
            {
                if (skin.skinId == skinId)
                {
                    return skin;
                }
            }
            
            return m_DefaultSkin;
        }
        
        public SkinSet GetCurrentSkin()
        {
            return GetSkinById(m_CurrentSkinId);
        }
        
        public void SetCurrentSkin(int skinId)
        {
            m_CurrentSkinId = skinId;
        }
        
        public SkinSet GetNextSkin()
        {
            int currentIndex = GetSkinIndex(m_CurrentSkinId);
            int nextIndex = (currentIndex + 1) % m_SkinSets.Count;
            return m_SkinSets[nextIndex];
        }
        
        public SkinSet GetPreviousSkin()
        {
            int currentIndex = GetSkinIndex(m_CurrentSkinId);
            int prevIndex = currentIndex - 1;
            if (prevIndex < 0) prevIndex = m_SkinSets.Count - 1;
            return m_SkinSets[prevIndex];
        }
        
        private int GetSkinIndex(int skinId)
        {
            for (int i = 0; i < m_SkinSets.Count; i++)
            {
                if (m_SkinSets[i].skinId == skinId)
                {
                    return i;
                }
            }
            return 0;
        }

        public bool HasSkinForId(int skinId)
        {
            return GetSkinById(skinId) != null;
        }

        public int GetSkinCount()
        {
            return m_SkinSets.Count;
        }

        public bool UnlockSkin(int skinId)
        {
            var skin = GetSkinById(skinId);
            if (skin == null || skin.isUnlocked) return false;
            
            skin.isUnlocked = true;
            return true;
        }

        public bool IsSkinUnlocked(int skinId)
        {
            var skin = GetSkinById(skinId);
            return skin != null && skin.isUnlocked;
        }

        public List<SkinSet> GetUnlockedSkins()
        {
            List<SkinSet> unlockedSkins = new List<SkinSet>();
            foreach (var skin in m_SkinSets)
            {
                if (skin.isUnlocked)
                {
                    unlockedSkins.Add(skin);
                }
            }
            return unlockedSkins;
        }

        public List<SkinSet> GetLockedSkins()
        {
            List<SkinSet> lockedSkins = new List<SkinSet>();
            foreach (var skin in m_SkinSets)
            {
                if (!skin.isUnlocked)
                {
                    lockedSkins.Add(skin);
                }
            }
            return lockedSkins;
        }

        [ContextMenu("Validate Skin Data")]
        public void ValidateSkinData()
        {
            for (int i = 0; i < m_SkinSets.Count; i++)
            {
                var skin = m_SkinSets[i];
                
                if (string.IsNullOrEmpty(skin.skinName) || 
                    m_SharedHeadMesh == null || 
                    m_SharedHeadSlappedMesh == null || 
                    m_SharedHeadVerySlappedMesh == null || 
                    skin.bodyMesh == null)
                {
                    // Invalid skin data
                }
            }
        }
    }
}
