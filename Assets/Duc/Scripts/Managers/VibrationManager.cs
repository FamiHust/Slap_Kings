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
            }
            else if (m_Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }
        
        /// <summary>
        /// Vibrate device
        /// </summary>
        public void Vibrate()
        {
            if (!m_EnableVibration) return;
            
#if UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#elif UNITY_IOS && !UNITY_EDITOR
            HapticFeedback.Generate(HapticFeedback.HapticFeedbackType.Impact);
#endif
        }
        
        /// <summary>
        /// Enable/Disable vibration
        /// </summary>
        public void SetVibrationEnabled(bool enabled)
        {
            m_EnableVibration = enabled;
        }
        
        [ContextMenu("Test Vibration")]
        public void TestVibration()
        {
            Vibrate();
        }
    }
}
