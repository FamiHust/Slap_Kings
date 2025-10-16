using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUI : MonoBehaviour
{
    [Header("Player Health UI")]
    [SerializeField] private Slider m_PlayerHealthSlider;
    [SerializeField] private TextMeshProUGUI m_PlayerHealthText;

    [Header("AI Health UI")]
    [SerializeField] private Slider m_AIHealthSlider;
    [SerializeField] private TextMeshProUGUI m_AIHealthText;

    [Header("Health References")]
    [SerializeField] private PlayerHealth m_PlayerHealth;
    [SerializeField] private AIHealth m_AIHealth;
    
    [Header("Animation Settings")]
    [SerializeField] private float m_AnimationSpeed = 2f;
    [SerializeField] private bool m_EnableSmoothAnimation = true;
    
    private int m_TargetPlayerHealth;
    private int m_TargetAIHealth;

    private void Start()
    {
        SetupHealthUI();
        
        if (m_PlayerHealth != null)
        {
            m_TargetPlayerHealth = m_PlayerHealth.GetCurrentHealth();
        }
        if (m_AIHealth != null)
        {
            m_TargetAIHealth = m_AIHealth.GetCurrentHealth();
        }
    }

    private void Update()
    {
        UpdatePlayerHealthUI();
        UpdateAIHealthUI();
    }

    private void SetupHealthUI()
    {
        if (m_PlayerHealthSlider != null && m_PlayerHealth != null)
        {
            m_PlayerHealthSlider.maxValue = m_PlayerHealth.GetMaxHealth();
            m_PlayerHealthSlider.value = m_PlayerHealth.GetCurrentHealth();
        }

        if (m_AIHealthSlider != null && m_AIHealth != null)
        {
            m_AIHealthSlider.maxValue = m_AIHealth.GetMaxHealth();
            m_AIHealthSlider.value = m_AIHealth.GetCurrentHealth();
        }
    }

    private void UpdatePlayerHealthUI()
    {
        if (m_PlayerHealth == null) return;

        m_TargetPlayerHealth = m_PlayerHealth.GetCurrentHealth();

        if (m_PlayerHealthSlider != null)
        {
            if (m_EnableSmoothAnimation)
            {
                m_PlayerHealthSlider.value = Mathf.Lerp(m_PlayerHealthSlider.value, m_TargetPlayerHealth, m_AnimationSpeed * Time.deltaTime);
            }
            else
            {
                m_PlayerHealthSlider.value = m_TargetPlayerHealth;
            }
        }

        if (m_PlayerHealthText != null)
        {
            if (m_EnableSmoothAnimation)
            {
                m_PlayerHealthText.text = Mathf.RoundToInt(m_PlayerHealthSlider.value).ToString();
            }
            else
            {
                m_PlayerHealthText.text = m_PlayerHealth.GetCurrentHealth().ToString();
            }
        }
    }

    private void UpdateAIHealthUI()
    {
        if (m_AIHealth == null) return;

        m_TargetAIHealth = m_AIHealth.GetCurrentHealth();

        if (m_AIHealthSlider != null)
        {
            if (m_EnableSmoothAnimation)
            {
                m_AIHealthSlider.value = Mathf.Lerp(m_AIHealthSlider.value, m_TargetAIHealth, m_AnimationSpeed * Time.deltaTime);
            }
            else
            {
                m_AIHealthSlider.value = m_TargetAIHealth;
            }
        }

        if (m_AIHealthText != null)
        {
            if (m_EnableSmoothAnimation)
            {
                m_AIHealthText.text = Mathf.RoundToInt(m_AIHealthSlider.value).ToString();
            }
            else
            {
                m_AIHealthText.text = m_AIHealth.GetCurrentHealth().ToString();
            }
        }
    }

    public void SetPlayerHealth(PlayerHealth playerHealth)
    {
        m_PlayerHealth = playerHealth;
        SetupHealthUI();
    }

    public void SetAIHealth(AIHealth aiHealth)
    {
        m_AIHealth = aiHealth;
        SetupHealthUI();
    }
}
