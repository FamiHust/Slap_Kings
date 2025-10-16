using UnityEngine;
using TMPro;

public class PowerMeter : MonoBehaviour
{
    public static PowerMeter Instance;

    [Header("Animation")]
    [SerializeField] private Animation m_PowerBarAnim;
    [SerializeField] private string clipName = "PowerBarAnim3D";

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI m_PowerText;

    [Header("Settings")]
    [SerializeField] private bool isActive = false;
    [SerializeField] private bool loopPingPong = true;  
    [SerializeField] private float m_AnimSpeed = 1f;     
    [SerializeField] private int m_MinPower = 0;         // configurable min power
    [SerializeField] private int m_MaxPower = 90;        // configurable max power

    private AnimationState m_AnimState;
    private int m_Direction = 1;        
    private int m_PowerValue = 0;

    void Awake()
    {
        Instance = this;
        // Eagerly cache animation state to avoid Start order race
        if (m_PowerBarAnim != null)
            m_AnimState = m_PowerBarAnim[clipName];
    }

    void Start()
    {
        if (m_PowerText != null)
            m_PowerText.text = "0"; 
        if (m_PowerBarAnim != null)
            m_AnimState = m_PowerBarAnim[clipName];
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
        // Lazily resolve anim state in case Start hasn't run yet
        if (m_AnimState == null && m_PowerBarAnim != null)
            m_AnimState = m_PowerBarAnim[clipName];

        if (m_AnimState == null)
        {
            Debug.LogWarning("PowerMeter.StartMeter called but AnimationState is null. Check clipName and Animation reference.");
            return;
        }

        isActive = true;
        m_Direction = 1;
        m_AnimState.speed = 0f;  
        m_AnimState.time = 0f;
        m_PowerBarAnim.Play(clipName);

        // Force an immediate sample and UI update on start
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
}
