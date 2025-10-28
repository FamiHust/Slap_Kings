using UnityEngine;

namespace Duc
{
    public class VibrationManager : MonoBehaviour
    {
        [Header("Vibration Settings")]
        [SerializeField] private bool m_EnableVibration = true;
        
        private static VibrationManager m_Instance;
        
        public static VibrationManager Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = FindObjectOfType<VibrationManager>();
                    if (m_Instance == null)
                    {
                        GameObject go = new GameObject("VibrationManager");
                        m_Instance = go.AddComponent<VibrationManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return m_Instance;
            }
        }
        
        private void Awake()
        {
            if (m_Instance == null)
            {
                m_Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadSettings();
            }
            else if (m_Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }
        
        private void LoadSettings()
        {
            m_EnableVibration = PlayerPrefs.GetInt("VibrateEnabled", 1) == 1;
        }
        
        private void SaveSettings()
        {
            PlayerPrefs.SetInt("VibrateEnabled", m_EnableVibration ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void Vibrate()
        {
            if (!m_EnableVibration) return;
            
#if UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#elif UNITY_IOS && !UNITY_EDITOR
            HapticFeedback.Generate(HapticFeedback.HapticFeedbackType.Impact);
#endif
        }
        
        public void SetVibrationEnabled(bool enabled)
        {
            m_EnableVibration = enabled;
            SaveSettings();
        }
        
        public bool IsVibrationEnabled => m_EnableVibration;
        
        [ContextMenu("Test Vibration")]
        public void TestVibration()
        {
            Vibrate();
        }
    }
}
