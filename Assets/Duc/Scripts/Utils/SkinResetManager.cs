using UnityEngine;

namespace Duc
{
    /// <summary>
    /// Manager để reset skin khi clear PlayerPrefs
    /// </summary>
    public class SkinResetManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerSkinManager m_SkinManager;
        
        private void Start()
        {
            if (m_SkinManager == null)
                m_SkinManager = FindObjectOfType<PlayerSkinManager>();
        }
        
        /// <summary>
        /// Reset tất cả skin về trạng thái ban đầu
        /// Gọi method này sau khi clear PlayerPrefs
        /// </summary>
        [ContextMenu("Reset All Skins")]
        public void ResetAllSkins()
        {
            if (m_SkinManager != null)
            {
                m_SkinManager.ResetAllSkins();
            }
        }
        
        /// <summary>
        /// Reset skin hiện tại
        /// </summary>
        [ContextMenu("Reset Current Skin")]
        public void ResetCurrentSkin()
        {
            if (m_SkinManager != null)
            {
                m_SkinManager.ResetCurrentSkin();
            }
        }
        
        /// <summary>
        /// Clear PlayerPrefs và reset skin
        /// </summary>
        [ContextMenu("Clear PlayerPrefs and Reset Skins")]
        public void ClearPlayerPrefsAndResetSkins()
        {
            // Clear PlayerPrefs
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            
            // Reset skin
            ResetAllSkins();
            
            // Cập nhật coin display nếu có
            var coinDisplay = FindObjectOfType<CoinDisplay>();
            if (coinDisplay != null)
            {
                coinDisplay.UpdateDisplay();
            }
        }
    }
}
