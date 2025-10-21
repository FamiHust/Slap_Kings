using UnityEngine;
using TMPro;

namespace Duc
{
    public class PowerMeter : SingletonManager<PowerMeter>
    {

        [Header("Animation")]
        [SerializeField] private Animation m_PowerBarAnim;
        [SerializeField] private string clipName = "PowerBarAnim3D";

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI m_PowerText;

        [Header("Settings")]
        [SerializeField] private bool isActive = false;
        [SerializeField] private bool loopPingPong = true;  
        private float m_AnimSpeed = 1f;     
        private int m_MinPower = 0;        
        private int m_MaxPower = 90;        

        private AnimationState m_AnimState;
        private int m_Direction = 1;        
        private int m_PowerValue = 0;

        protected override void Awake()
        {
            base.Awake();
            if (m_PowerBarAnim != null)
                m_AnimState = m_PowerBarAnim[clipName];
        }

        void Start()
        {
            if (m_PowerText != null)
                m_PowerText.text = "100"; 
            if (m_PowerBarAnim != null)
                m_AnimState = m_PowerBarAnim[clipName];

            ApplyPowerUpgradeRange();

            var persistentData = PersistentDataManager.Instance;
            if (persistentData != null)
            {
                persistentData.OnPowerUpgradePurchased -= OnPowerUpgradePurchased;
                persistentData.OnPowerUpgradePurchased += OnPowerUpgradePurchased;
                persistentData.OnProgressReset -= OnPowerUpgradePurchased;
                persistentData.OnProgressReset += OnPowerUpgradePurchased;
            }
        }

        private void OnDestroy()
        {
            var persistentData = PersistentDataManager.Instance;
            if (persistentData != null)
            {
                persistentData.OnPowerUpgradePurchased -= OnPowerUpgradePurchased;
                persistentData.OnProgressReset -= OnPowerUpgradePurchased;
            }
        }

        private void OnPowerUpgradePurchased()
        {
            bool wasActive = isActive;
            if (wasActive) StopMeter();
            ApplyPowerUpgradeRange();
            if (wasActive) StartMeter();
        }

        private void ApplyPowerUpgradeRange()
        {
            var persistentData = PersistentDataManager.Instance;
            var dataManager = DataManager.Get();
            if (persistentData == null || dataManager == null) return;
            int upgrades = persistentData.GetPowerUpgradeCount();
            var playerStats = DataManager.Get()?.PlayerStats;

            if (playerStats != null)
            {
                m_MinPower = playerStats.power.GetMinPowerWithUpgrades(upgrades);
                m_MaxPower = playerStats.power.GetMaxPowerWithUpgrades(upgrades);
                // Apply animation settings from PlayerStatsData
                m_AnimSpeed = playerStats.power.animSpeed;
                loopPingPong = playerStats.power.loopPingPong;
            }
        }

        void Update()
        {
            if (!isActive || m_AnimState == null) return;

            m_AnimState.time += Time.unscaledDeltaTime * m_AnimSpeed * m_Direction;

            if (loopPingPong)
            {
                if (m_AnimState.time >= m_AnimState.length)
                {
                    m_AnimState.time = m_AnimState.length;
                    m_Direction = -1;
                }
                else if (m_AnimState.time <= 0f)
                {
                    m_AnimState.time = 0f;
                    m_Direction = 1;
                }
            }
            else
            {
                if (m_AnimState.time > m_AnimState.length)
                    m_AnimState.time = 0f;
            }

            m_PowerBarAnim.Sample();

            float normalized = m_AnimState.time / m_AnimState.length;
            float triangle = 1f - Mathf.Abs(normalized * 2f - 1f);
            int minP = Mathf.Min(m_MinPower, m_MaxPower);
            int maxP = Mathf.Max(m_MinPower, m_MaxPower);
            m_PowerValue = Mathf.Clamp(Mathf.RoundToInt(triangle * (maxP - minP) + minP), minP, maxP);

            if (m_PowerText != null)
                m_PowerText.text = m_PowerValue.ToString();
        }

        public void StartMeter()
        {
            if (m_AnimState == null && m_PowerBarAnim != null)
                m_AnimState = m_PowerBarAnim[clipName];

            if (m_AnimState == null)
            {
                return;
            }

            isActive = true;
            m_Direction = 1;
            m_AnimState.speed = 0f;  
            m_AnimState.time = 0f;
            m_PowerBarAnim.Play(clipName);

            m_PowerBarAnim.Sample();
            float normalized = m_AnimState.time / m_AnimState.length;
            float triangle = 1f - Mathf.Abs(normalized * 2f - 1f);
            int minP = Mathf.Min(m_MinPower, m_MaxPower);
            int maxP = Mathf.Max(m_MinPower, m_MaxPower);
            m_PowerValue = Mathf.Clamp(Mathf.RoundToInt(triangle * (maxP - minP) + minP), minP, maxP);
            if (m_PowerText != null)
                m_PowerText.text = m_PowerValue.ToString();
        }

        public void StopMeter()
        {
            if (!isActive) return;
            isActive = false;

            if (m_PowerBarAnim != null)
                m_PowerBarAnim.Sample();

        }

        public int GetPowerValue()
        {
            return m_PowerValue;
        }

        public int GetMaxPower()
        {
            return Mathf.Max(m_MinPower, m_MaxPower);
        }

        protected override void OnInitialize()
        {
            // PowerMeter specific initialization
            if (m_PowerBarAnim != null)
                m_AnimState = m_PowerBarAnim[clipName];
                
            ApplyPowerUpgradeRange();
            
            var persistentData = PersistentDataManager.Instance;
            if (persistentData != null)
            {
                persistentData.OnPowerUpgradePurchased -= OnPowerUpgradePurchased;
                persistentData.OnPowerUpgradePurchased += OnPowerUpgradePurchased;
                persistentData.OnProgressReset -= OnPowerUpgradePurchased;
                persistentData.OnProgressReset += OnPowerUpgradePurchased;
            }
        }

        protected override void OnCleanup()
        {
            // PowerMeter specific cleanup
            var persistentData = PersistentDataManager.Instance;
            if (persistentData != null)
            {
                persistentData.OnPowerUpgradePurchased -= OnPowerUpgradePurchased;
                persistentData.OnProgressReset -= OnPowerUpgradePurchased;
            }
        }
    }
}
