using UnityEngine;
using TMPro;

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
        
        public bool IsActive => isActive;
        public bool IsCounterCaptured => !isActive && m_CounterValue > 0f;        

        private AnimationState m_AnimState;
        private int m_Direction = 1;        
        private float m_CounterValue = 0f;
        
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
            if (m_CounterText != null)
                m_CounterText.text = "0.0"; 
            if (m_CounterBarAnim != null)
                m_AnimState = m_CounterBarAnim[clipName];
                
            HideCounterBar();
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
        }

        public void StopCounter()
        {            
            isActive = false;

            if (m_CounterBarAnim != null)
                m_CounterBarAnim.Sample();

            // Hide counter bar UI
            HideCounterBar();

            Debug.Log("Counter system stopped and hidden");
            OnCounterEnded?.Invoke();
        }

        private void AttemptCounter()
        {
            if (!isActive) return;

            isActive = false;
            
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

        /// <summary>
        /// Apply counter damage reduction to AI damage
        /// </summary>
        public int ApplyCounterReduction(int originalDamage, float counterValue)
        {
            int reducedDamage = Mathf.RoundToInt(originalDamage * (1f - counterValue));
            return Mathf.Max(1, reducedDamage); // Minimum 1 damage
        }

        protected override void OnInitialize()
        {
            // CounterSystem specific initialization
        }

        private void ShowCounterBar()
        {
            Debug.Log("ShowCounterBar() called");
            
            if (m_CounterBarAnim != null)
            {
                Debug.Log("Activating counter bar animation");
                m_CounterBarAnim.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError("Counter bar animation is null!");
            }
            
            if (m_CounterText != null)
            {
                Debug.Log("Activating counter text");
                m_CounterText.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError("Counter text is null!");
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
            // CounterSystem specific cleanup
        }
    }
}