using UnityEngine;

namespace Duc
{
    [System.Serializable]
    public class SoundData
    {
        public SoundManager.SoundType soundType;
        public AudioClip audioClip;
    }

    public class SoundManager : SingletonManager<SoundManager>
    {
        public enum SoundType
        {
            Victory,
            Defeated,
            Button,
            NormalSlap,
            MegaSlap,
            LastHit,
            UpHealth,
            UpPower,
            GetNormalSlapped,
            GetMegaSlapped,
            CoinReward,
            Tap,
            SkinChange
        }

        [Header("Audio Source")]
        [SerializeField] private AudioSource m_SoundSource;

        [Header("Sound Clips Array")]
        [SerializeField] private SoundData[] m_SoundClips;

        [Header("Settings")]
        [SerializeField] private float m_Volume = 1f;
        [SerializeField] private bool m_SoundEnabled = true;
        [SerializeField] private bool m_AutoSetupButtonSounds = true;

        protected override void Awake()
        {
            base.Awake();

            if (m_SoundSource == null)
            {
                GameObject soundObject = new GameObject("SoundSource");
                soundObject.transform.SetParent(transform);
                m_SoundSource = soundObject.AddComponent<AudioSource>();
                m_SoundSource.loop = false;
                m_SoundSource.playOnAwake = false;
            }
            
            LoadSettings();
        }
        
        private void LoadSettings()
        {
            m_SoundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        }

        private AudioClip GetSoundClip(SoundType soundType)
        {
            if (m_SoundClips == null || m_SoundClips.Length == 0) return null;

            foreach (var soundData in m_SoundClips)
            {
                if (soundData != null && soundData.soundType == soundType)
                {
                    return soundData.audioClip;
                }
            }

            return null;
        }

        public void PlaySound(SoundType soundType)
        {
            if (!m_SoundEnabled) return;

            AudioClip soundClip = GetSoundClip(soundType);
            if (soundClip != null && m_SoundSource != null)
            {
                m_SoundSource.PlayOneShot(soundClip, m_Volume);
            }
        }

        public void PlaySound(SoundType soundType, float volume)
        {
            if (!m_SoundEnabled) return;

            AudioClip soundClip = GetSoundClip(soundType);
            if (soundClip != null && m_SoundSource != null)
            {
                m_SoundSource.PlayOneShot(soundClip, m_Volume * Mathf.Clamp01(volume));
            }
        }

        public void PlayCustom(AudioClip clip)
        {
            if (!m_SoundEnabled || clip == null || m_SoundSource == null) return;

            m_SoundSource.PlayOneShot(clip, m_Volume);
        }

        public void SetVolume(float volume)
        {
            m_Volume = Mathf.Clamp01(volume);
        }

        public void SetEnabled(bool enabled)
        {
            m_SoundEnabled = enabled;
            PlayerPrefs.SetInt("SoundEnabled", m_SoundEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public float Volume => m_Volume;
        public bool IsEnabled => m_SoundEnabled;

        protected override void OnInitialize()
        {
            if (m_AutoSetupButtonSounds)
            {
                AutoSetupButtonSounds();
            }
            
            var persistentData = PersistentDataManager.Instance;
            if (persistentData != null)
            {
                persistentData.OnHealthUpgradePurchased += OnHealthUpgrade;
                persistentData.OnPowerUpgradePurchased += OnPowerUpgrade;
            }
        }
        
        private void OnHealthUpgrade()
        {
            PlaySound(SoundType.UpHealth);
        }
        
        private void OnPowerUpgrade()
        {
            PlaySound(SoundType.UpPower);
        }

        private void AutoSetupButtonSounds()
        {
            UnityEngine.UI.Button[] buttons = FindObjectsOfType<UnityEngine.UI.Button>(true);
            
            int addedCount = 0;
            int skippedCount = 0;
            
            foreach (UnityEngine.UI.Button button in buttons)
            {
                if (button == null) continue;
                
                ButtonSoundHelper helper = button.GetComponent<ButtonSoundHelper>();
                if (helper != null)
                {
                    skippedCount++;
                    continue;
                }
                
                button.gameObject.AddComponent<ButtonSoundHelper>();
                addedCount++;
            }
        }

        protected override void OnCleanup()
        {
            if (m_SoundSource != null)
            {
                m_SoundSource.Stop();
            }
            
            var persistentData = PersistentDataManager.Instance;
            if (persistentData != null)
            {
                persistentData.OnHealthUpgradePurchased -= OnHealthUpgrade;
                persistentData.OnPowerUpgradePurchased -= OnPowerUpgrade;
            }
        }

        public void PlayButtonSound()
        {
            PlaySound(SoundType.Button);
        }
    }
}
