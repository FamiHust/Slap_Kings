using UnityEngine;
using TMPro;

namespace Duc
{
    public class PlayerSkinManager : MonoBehaviour
    {
        [Header("Skin Configuration")]
        [SerializeField] private PlayerSkinData m_SkinData;
        
        [Header("Skinned Mesh Renderer References")]
        [SerializeField] private SkinnedMeshRenderer m_HeadRenderer;
        [SerializeField] private SkinnedMeshRenderer m_BodyRenderer;
        
        [Header("Health Thresholds")]
        [SerializeField] private float m_HeadSlappedThreshold = 0.5f; 
        [SerializeField] private float m_HeadVerySlappedThreshold = 0.3f; 
        
        [Header("Settings")]
        [SerializeField] private bool m_EnableDebugLogs = true;
        
        [Header("UI Display (Optional)")]
        [SerializeField] private TextMeshProUGUI m_SkinNameText;
        [SerializeField] private TextMeshProUGUI m_UnlockCostText;
        [SerializeField] private UnityEngine.UI.Button m_BuySkinButton; 

        private PlayerSkinData.SkinSet m_CurrentSkin;
        private PlayerHealth m_PlayerHealth;
        private bool m_IsUsingSlappedMesh = false;
        private CoinManager m_CoinManager;
        
        public System.Action<PlayerSkinData.SkinSet> OnSkinChanged;
        public System.Action<int> OnSkinIdChanged;
        
        public PlayerSkinData SkinData => m_SkinData;
        public PlayerSkinData.SkinSet CurrentSkin => m_CurrentSkin;
        public int CurrentSkinId => m_SkinData != null ? m_SkinData.CurrentSkinId : 0;
        
        private void Awake()
        {   
            m_PlayerHealth = GetComponent<PlayerHealth>();
            if (m_PlayerHealth == null)
            {
                m_PlayerHealth = GetComponentInChildren<PlayerHealth>();
            }
            if (m_PlayerHealth == null)
            {
                m_PlayerHealth = GetComponentInParent<PlayerHealth>();
            }
            if (m_PlayerHealth == null)
            {
                m_PlayerHealth = FindObjectOfType<PlayerHealth>();
            }
            
            m_CoinManager = CoinManager.Get();
        }
        
        private void Start()
        {
            if (m_PlayerHealth != null)
            {
                m_PlayerHealth.OnHealthChanged += OnPlayerHealthChanged;
            }
            
            if (m_SkinData != null)
            {
                ApplySkin(m_SkinData.GetCurrentSkin());
            }
        }
        
        private SkinnedMeshRenderer FindSkinnedMeshRenderer(string name)
        {
            SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            
            foreach (var renderer in renderers)
            {
                if (renderer.name.ToLower().Contains(name.ToLower()))
                {
                    return renderer;
                }
            }
            
            if (renderers.Length > 0)
            {
                return renderers[0];
            }
            
            return null;
        }
        
        
        public void SetSkinData(PlayerSkinData skinData)
        {
            if (m_SkinData != skinData)
            {
                m_SkinData = skinData;
                if (m_SkinData != null)
                {
                    ApplySkin(m_SkinData.GetCurrentSkin());
                }
            }
        }
        
        public void ApplySkin(PlayerSkinData.SkinSet skin)
        {
            if (m_HeadRenderer != null)
            {
                bool useHeadVerySlappedMesh = ShouldUseHeadVerySlappedMesh();
                bool useHeadSlappedMesh = ShouldUseHeadSlappedMesh();
                
                if (useHeadVerySlappedMesh && m_SkinData?.SharedHeadVerySlappedMesh != null)
                {
                    m_HeadRenderer.sharedMesh = m_SkinData.SharedHeadVerySlappedMesh;
                }
                else if (useHeadSlappedMesh && m_SkinData?.SharedHeadSlappedMesh != null)
                {
                    m_HeadRenderer.sharedMesh = m_SkinData.SharedHeadSlappedMesh;
                }
                else
                {
                    if (m_SkinData?.SharedHeadMesh != null)
                    {
                        m_HeadRenderer.sharedMesh = m_SkinData.SharedHeadMesh;
                    }
                }
            }
            
            
            if (m_BodyRenderer != null)
            {
                if (skin.bodyMesh != null)
                {
                    m_BodyRenderer.sharedMesh = skin.bodyMesh;
                }
            }
            
            m_CurrentSkin = skin;
            OnSkinChanged?.Invoke(skin);
            
            UpdateCostDisplay();
        }
        
        public void SetSkinById(int skinId)
        {
            if (m_SkinData != null)
            {
                var skin = m_SkinData.GetSkinById(skinId);
                if (skin != null)
                {
                    m_SkinData.SetCurrentSkin(skinId);
                    ApplySkin(skin);
                    OnSkinIdChanged?.Invoke(skinId);
                }
            }
        }
        
        public void NextSkin()
        {
            if (m_SkinData != null)
            {
                var nextSkin = m_SkinData.GetNextSkin();
                SetSkinById(nextSkin.skinId);
            }
        }
        
        public void PreviousSkin()
        {
            if (m_SkinData != null)
            {
                var prevSkin = m_SkinData.GetPreviousSkin();
                SetSkinById(prevSkin.skinId);
            }
        }
        
        private void OnPlayerHealthChanged(int currentHealth, int maxHealth)
        {
            if (m_CurrentSkin == null) return;
            
            bool shouldUseHeadVerySlapped = ShouldUseHeadVerySlappedMesh();
            bool shouldUseHeadSlapped = ShouldUseHeadSlappedMesh();
            
            ApplySkin(m_CurrentSkin);
            
            if (m_EnableDebugLogs)
            {
                string headState = shouldUseHeadVerySlapped ? "Very Slapped" : 
                                 shouldUseHeadSlapped ? "Slapped" : "Normal";
            }
        }
        
        private bool ShouldUseHeadSlappedMesh()
        {
            if (m_PlayerHealth == null) return false;
            
            float healthPercentage = m_PlayerHealth.GetHealthPercentage();
            return healthPercentage < m_HeadSlappedThreshold && healthPercentage >= m_HeadVerySlappedThreshold;
        }
        
        private bool ShouldUseHeadVerySlappedMesh()
        {
            if (m_PlayerHealth == null) return false;
            
            float healthPercentage = m_PlayerHealth.GetHealthPercentage();
            return healthPercentage < m_HeadVerySlappedThreshold;
        }
        
        public void OnNextSkinButtonClicked()
        {
            NextSkin();

        }
        
        public void OnPreviousSkinButtonClicked()
        {
            PreviousSkin();
        }
        
        public void OnTestSlappedMeshButtonClicked()
        {
            if (m_CurrentSkin != null)
            {
                m_IsUsingSlappedMesh = !m_IsUsingSlappedMesh;
                ApplySkin(m_CurrentSkin);
            }
        }
        
        public void OnBuySkinButtonClicked()
        {
            if (m_CurrentSkin == null)
            {
                return;
            }
            
            if (m_CurrentSkin.isUnlocked)
            {
                OnUseUnlockedSkin();
            }
            else
            {
                UnlockCurrentSkinWithCoins();
            }
        }

        public bool IsSkinUnlocked(int skinId)
        {
            if (m_SkinData == null) return false;
            var skin = m_SkinData.GetSkinById(skinId);
            return skin != null && skin.isUnlocked;
        }

        public bool IsCurrentSkinUnlocked()
        {
            return m_CurrentSkin != null && m_CurrentSkin.isUnlocked;
        }
 
        public bool UnlockSkin(int skinId)
        {
            if (m_SkinData == null) return false;
            var skin = m_SkinData.GetSkinById(skinId);
            if (skin == null || skin.isUnlocked) return false;
            
            skin.isUnlocked = true;
            return true;
        }

        public bool UnlockCurrentSkin()
        {
            if (m_CurrentSkin == null || m_CurrentSkin.isUnlocked) return false;
            
            m_CurrentSkin.isUnlocked = true;
            return true;
        }

        public int GetSkinUnlockCost(int skinId)
        {
            if (m_SkinData == null) return 0;
            var skin = m_SkinData.GetSkinById(skinId);
            return skin?.unlockCost ?? 0;
        }

        public int GetCurrentSkinUnlockCost()
        {
            return m_CurrentSkin?.unlockCost ?? 0;
        }

        public void UpdateCostDisplay()
        {
            if (m_CurrentSkin == null) return;
            
            if (m_SkinNameText != null)
            {
                m_SkinNameText.text = m_CurrentSkin.skinName.ToString();
            }
            
            UpdateBuyButtonState();
        }

        private void UpdateBuyButtonState()
        {
            if (m_BuySkinButton == null) return;
            
            if (m_CurrentSkin == null)
            {
                m_BuySkinButton.interactable = false;
                return;
            }
            
            if (m_CurrentSkin.isUnlocked)
            {
                m_BuySkinButton.interactable = true;
                var buttonText = m_BuySkinButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "FIGHT!";
                }
            }
            else
            {
                bool canAfford = CanAffordCurrentSkin();
                m_BuySkinButton.interactable = canAfford;
                
                var buttonText = m_BuySkinButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "$" + m_CurrentSkin.unlockCost.ToString();
                }
            }
        }

        public bool CanAffordCurrentSkin()
        {
            if (m_CurrentSkin == null || m_CurrentSkin.isUnlocked) return true;
            
            var persistentData = PersistentDataManager.Instance;
            if (persistentData != null)
            {
                int currentCoins = persistentData.GetCurrentCoins();
                int requiredCoins = m_CurrentSkin.unlockCost;
                bool canAfford = persistentData.CanAffordSkin(requiredCoins);
                
                return canAfford;
            }
            
            if (m_CoinManager != null)
            {
                int currentCoins = m_CoinManager.GetCurrentCoins();
                int requiredCoins = m_CurrentSkin.unlockCost;
                bool canAfford = currentCoins >= requiredCoins;
                
                return canAfford;
            }

            return false;
        }

        public bool UnlockCurrentSkinWithCoins()
        {
            if (m_CurrentSkin == null || m_CurrentSkin.isUnlocked) 
            {
                return false;
            }
            
            if (!CanAffordCurrentSkin()) 
            {
                return false;
            }
            
            var persistentData = PersistentDataManager.Instance;
            if (persistentData != null)
            {
                if (persistentData.SpendCoins(m_CurrentSkin.unlockCost))
                {
                    m_CurrentSkin.isUnlocked = true;
                    
                    UpdateCostDisplay();
                    
                    var coinDisplay = FindObjectOfType<CoinDisplay>();
                    if (coinDisplay != null)
                    {
                        coinDisplay.UpdateDisplay();
                    }
                    
                    return true;
                }
            }

            return false;
        }

        public void OnCoinsChanged()
        {
            UpdateCostDisplay();
        }

        public void UpdateBuyButton()
        {
            UpdateBuyButtonState();
        }

        public void OnUseUnlockedSkin()
        {
            if (m_CurrentSkin == null || !m_CurrentSkin.isUnlocked)
            {
                return;
            }
        }

        public void BuySkinButton()
        {
            OnBuySkinButtonClicked();
        }

        public void ResetAllSkins()
        {
            if (m_SkinData == null) return;
            
            for (int i = 0; i < m_SkinData.SkinSets.Count; i++)
            {
                var skin = m_SkinData.SkinSets[i];
                if (i == 0)
                {
                    skin.isUnlocked = true;
                }
                else
                {
                    skin.isUnlocked = false;
                }
            }
            
            UpdateCostDisplay();
        }

        public void ResetCurrentSkin()
        {
            if (m_CurrentSkin == null) return;
            
            if (m_CurrentSkin.skinId == 0)
            {
                return;
            }
            
            m_CurrentSkin.isUnlocked = false;
            UpdateCostDisplay();
        }
        
        [ContextMenu("Test Reset All Skins")]
        public void TestResetAllSkins()
        {
            ResetAllSkins();
        }
        
        private void OnDestroy()
        {
            if (m_PlayerHealth != null)
            {
                m_PlayerHealth.OnHealthChanged -= OnPlayerHealthChanged;
            }
        }
    }
}
