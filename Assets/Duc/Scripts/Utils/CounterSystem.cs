using UnityEngine;
using TMPro;
using DG.Tweening;

namespace Duc
{
    public class CounterSystem : SingletonManager<CounterSystem>
    {
        [Header("Animation")]
        [SerializeField] private Animation m_CounterBarAnim;
        [SerializeField] private string clipName = "CounterBarAnim3D";

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI m_CounterText;

        [Header("Settings")]
        [SerializeField] private bool isActive = false;
        [SerializeField] private bool loopPingPong = true;  
        [SerializeField] private float m_AnimSpeed;     
        [SerializeField] private float m_MinCounter;        
        [SerializeField] private float m_MaxCounter;
        
        [Header("DOTween Scale Animation")]
        [SerializeField] private float m_ScaleInDuration = 0.3f;
        [SerializeField] private float m_ScaleOutDuration = 0.2f;
        [SerializeField] private Ease m_ScaleInEase = Ease.OutBack;
        [SerializeField] private Ease m_ScaleOutEase = Ease.InBack;
        
        public bool IsActive => isActive;
        public bool IsCounterCaptured => !isActive && m_CounterValue > 0f;        

        private AnimationState m_AnimState;
        private int m_Direction = 1;        
        private float m_CounterValue = 0f;
        private Tween m_ScaleTween;
        
        public System.Action<float> OnCounterAttempted; 
        public System.Action OnCounterStarted;
        public System.Action OnCounterEnded;

        protected override void Awake()
        {
            base.Awake();
            if (m_CounterBarAnim != null)
                m_AnimState = m_CounterBarAnim[clipName];
        }

        void Start()
        {
            ApplyCounterSpeed();
            
            if (m_CounterText != null)
                m_CounterText.text = "0.0"; 
            if (m_CounterBarAnim != null)
                m_AnimState = m_CounterBarAnim[clipName];
                
            HideCounterBar();
        }
        
        private void ApplyCounterSpeed()
        {
            var persistentData = PersistentDataManager.Instance;
            var dataManager = DataManager.Get();
            if (persistentData == null || dataManager == null) return;
            
            int currentLevel = persistentData.GetLevelCount();
            m_AnimSpeed = dataManager.GetCounterSpeedWithBossBonus(currentLevel);
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

            m_CounterBarAnim.Sample();

            float normalized = m_AnimState.time / m_AnimState.length;
            float triangle = 1f - Mathf.Abs(normalized * 2f - 1f);
            float minC = Mathf.Min(m_MinCounter, m_MaxCounter);
            float maxC = Mathf.Max(m_MinCounter, m_MaxCounter);
            m_CounterValue = Mathf.Clamp(triangle * (maxC - minC) + minC, minC, maxC);

            if (m_CounterText != null)
                m_CounterText.text = m_CounterValue.ToString("F1");

            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                AttemptCounter();
            }
        }

        public void StartCounter()
        {
            if (m_AnimState == null && m_CounterBarAnim != null)
                m_AnimState = m_CounterBarAnim[clipName];

            if (m_AnimState == null)
            {
                return;
            }

            ShowCounterBar();

            if (m_ScaleTween != null)
            {
                m_ScaleTween.Kill();
            }

            transform.localScale = Vector3.zero;
            
            m_ScaleTween = transform.DOScale(Vector3.one, m_ScaleInDuration)
                .SetEase(m_ScaleInEase)
                .OnComplete(() => {
                    isActive = true;
                    m_Direction = 1;
                    m_AnimState.speed = 0f;  
                    m_AnimState.time = 0f;
                    m_CounterBarAnim.Play(clipName);

                    m_CounterBarAnim.Sample();
                    float normalized = m_AnimState.time / m_AnimState.length;
                    float triangle = 1f - Mathf.Abs(normalized * 2f - 1f);
                    float minC = Mathf.Min(m_MinCounter, m_MaxCounter);
                    float maxC = Mathf.Max(m_MinCounter, m_MaxCounter);
                    m_CounterValue = Mathf.Clamp(triangle * (maxC - minC) + minC, minC, maxC);
                    if (m_CounterText != null)
                        m_CounterText.text = m_CounterValue.ToString();

                    OnCounterStarted?.Invoke();
                });
        }

        public void StopCounter()
        {            
            isActive = false;

            if (m_CounterBarAnim != null)
                m_CounterBarAnim.Sample();

            HideCounterBar();
            OnCounterEnded?.Invoke();
        }

        public void EndTurnHide()
        {
            if (m_ScaleTween != null)
            {
                m_ScaleTween.Kill();
            }

            m_ScaleTween = transform.DOScale(Vector3.zero, m_ScaleOutDuration)
                .SetEase(m_ScaleOutEase)
                .OnComplete(() => {
                    HideCounterBar();
                });
        }

        private void AttemptCounter()
        {
            if (!isActive) return;

            isActive = false;
            
            var sound = Duc.SoundManager.Get();
            if (sound != null)
            {
                sound.PlaySound(Duc.SoundManager.SoundType.Tap);
            }

            var effectManager = Duc.Managers.EffectManager.Instance;
            if (effectManager != null)
            {
                effectManager.StartPlayerShield();
            }

            OnCounterAttempted?.Invoke(m_CounterValue);
        }

        public float GetCounterValue()
        {
            return m_CounterValue;
        }

        public float GetMaxCounter()
        {
            return Mathf.Max(m_MinCounter, m_MaxCounter);
        }

        public int ApplyCounterReduction(int originalDamage, float counterValue)
        {
            int reducedDamage = Mathf.RoundToInt(originalDamage * (1f - counterValue));
            return Mathf.Max(1, reducedDamage); 
        }

        protected override void OnInitialize()
        {
            
        }

        private void ShowCounterBar()
        {
            if (m_CounterBarAnim != null)
            {
                m_CounterBarAnim.gameObject.SetActive(true);
            }
            
            if (m_CounterText != null)
            {
                m_CounterText.gameObject.SetActive(true);
            }
        }

        private void HideCounterBar()
        {
            if (m_CounterBarAnim != null)
            {
                m_CounterBarAnim.gameObject.SetActive(false);
            }
            
            if (m_CounterText != null)
            {
                m_CounterText.gameObject.SetActive(false);
            }
        }

        protected override void OnCleanup()
        {
            if (m_ScaleTween != null)
            {
                m_ScaleTween.Kill();
            }
        }
    }
}