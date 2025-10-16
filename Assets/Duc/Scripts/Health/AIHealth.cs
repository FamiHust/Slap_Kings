using UnityEngine;

public class AIHealth : HealthManager
{
    [Header("UI Reference")]
    [SerializeField] private HealthUI m_HealthUI;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        
        if (m_HealthUI == null)
        {
            m_HealthUI = FindObjectOfType<HealthUI>();
        }
        
        if (m_HealthUI != null)
        {
            m_HealthUI.SetAIHealth(this);
        }
    }

    public void TakeDamageFromPowerMeter()
    {
        if (PowerMeter.Instance != null)
        {
            int powerValue = PowerMeter.Instance.GetPowerValue();
            TakeDamage(powerValue);
        }
    }

    protected override void HandleDeath()
    {
        Debug.Log("AI has died!");
    }
}