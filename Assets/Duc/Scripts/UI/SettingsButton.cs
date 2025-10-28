using UnityEngine;
using UnityEngine.UI;

namespace Duc
{
    public class SettingsButton : MonoBehaviour
    {
        [Header("Button Settings")]
        [SerializeField] private Button m_Button;
        [SerializeField] private Image m_ButtonImage;
        
        [Header("Colors")]
        [SerializeField] private Color m_ActiveColor = Color.white;
        [SerializeField] private Color m_InactiveColor = Color.gray;
        
        [Header("Settings Type")]
        [SerializeField] private SettingType m_SettingType = SettingType.Sound;
        
        public enum SettingType
        {
            Sound,
            Vibrate
        }
        
        private bool m_IsEnabled = true;
        
        private void Awake()
        {
            if (m_Button == null)
                m_Button = GetComponent<Button>();
            
            if (m_ButtonImage == null)
                m_ButtonImage = GetComponent<Image>();
            
            LoadSettings();
            UpdateButtonVisual();
        }
        
        private void Start()
        {
            if (m_Button != null)
            {
                m_Button.onClick.AddListener(OnButtonClick);
            }
        }
        
        private void OnDestroy()
        {
            if (m_Button != null)
            {
                m_Button.onClick.RemoveListener(OnButtonClick);
            }
        }
        
        private void LoadSettings()
        {
            string key = GetPlayerPrefsKey();
            m_IsEnabled = PlayerPrefs.GetInt(key, 1) == 1;
        }
        
        private void SaveSettings()
        {
            string key = GetPlayerPrefsKey();
            PlayerPrefs.SetInt(key, m_IsEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }
        
        private string GetPlayerPrefsKey()
        {
            switch (m_SettingType)
            {
                case SettingType.Sound:
                    return "SoundEnabled";
                case SettingType.Vibrate:
                    return "VibrateEnabled";
                default:
                    return "SettingEnabled";
            }
        }
        
        private void OnButtonClick()
        {
            ToggleSetting();
        }
        
        public void ToggleSetting()
        {
            m_IsEnabled = !m_IsEnabled;
            SaveSettings();
            UpdateButtonVisual();
            ApplySetting();
        }
        
        private void UpdateButtonVisual()
        {
            if (m_ButtonImage != null)
            {
                m_ButtonImage.color = m_IsEnabled ? m_ActiveColor : m_InactiveColor;
            }
        }
        
        private void ApplySetting()
        {
            switch (m_SettingType)
            {
                case SettingType.Sound:
                    ApplySoundSetting();
                    break;
                case SettingType.Vibrate:
                    ApplyVibrateSetting();
                    break;
            }
        }
        
        private void ApplySoundSetting()
        {
            var soundManager = SoundManager.Get();
            if (soundManager != null)
            {
                soundManager.SetEnabled(m_IsEnabled);
            }
        }
        
        private void ApplyVibrateSetting()
        {
            var vibrateManager = VibrationManager.Instance;
            if (vibrateManager != null)
            {
                vibrateManager.SetVibrationEnabled(m_IsEnabled);
            }
        }
        
        public bool IsEnabled => m_IsEnabled;
    }
}

