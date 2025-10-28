using UnityEngine;

namespace Duc
{
    public class SkinResetManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerSkinManager m_SkinManager;
        
        private void Start()
        {
            if (m_SkinManager == null)
                m_SkinManager = FindObjectOfType<PlayerSkinManager>();
        }

        [ContextMenu("Reset All Skins")]
        public void ResetAllSkins()
        {
            if (m_SkinManager != null)
            {
                m_SkinManager.ResetAllSkins();
            }
        }

        [ContextMenu("Reset Current Skin")]
        public void ResetCurrentSkin()
        {
            if (m_SkinManager != null)
            {
                m_SkinManager.ResetCurrentSkin();
            }
        }

        [ContextMenu("Clear PlayerPrefs and Reset Skins")]
        public void ClearPlayerPrefsAndResetSkins()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            
            ResetAllSkins();
            
            var coinDisplay = FindObjectOfType<CoinDisplay>();
            if (coinDisplay != null)
            {
                coinDisplay.UpdateDisplay();
            }
        }
    }
}
