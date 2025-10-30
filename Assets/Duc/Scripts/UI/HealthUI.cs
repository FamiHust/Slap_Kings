using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Duc
{
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
        
        [Header("Damage Text Settings")]
        [SerializeField] private GameObject m_DamageTextPrefab;
        [SerializeField] private RectTransform m_PlayerDamageTextContainer;
        [SerializeField] private RectTransform m_AIDamageTextContainer;
        [SerializeField] private float m_DamageTextDuration = 2f;
        [SerializeField] private float m_DamageTextMoveSpeed = 2f;
        [SerializeField] private Color m_DamageTextColor = Color.red;
        [SerializeField] private int m_DamageTextPoolSize = 10;
        
        private int m_TargetPlayerHealth;
        private int m_TargetAIHealth;
        private int m_LastPlayerHealth;
        private int m_LastAIHealth;
        
        private System.Collections.Generic.Queue<GameObject> m_PlayerDamageTextPool = new System.Collections.Generic.Queue<GameObject>();
        private System.Collections.Generic.Queue<GameObject> m_AIDamageTextPool = new System.Collections.Generic.Queue<GameObject>();

        private void Start()
        {
            SetupHealthUI();
            SetupDamageTextContainers();
            InitializeDamageTextPools();
            
            if (m_PlayerHealth == null)
            {
                m_PlayerHealth = FindObjectOfType<PlayerHealth>();
            }
            if (m_AIHealth == null)
            {
                m_AIHealth = FindObjectOfType<AIHealth>();
            }
            
            if (m_PlayerHealth != null)
            {
                m_TargetPlayerHealth = m_PlayerHealth.GetCurrentHealth();
                m_LastPlayerHealth = m_PlayerHealth.GetCurrentHealth();
            }
            if (m_AIHealth != null)
            {
                m_TargetAIHealth = m_AIHealth.GetCurrentHealth();
                m_LastAIHealth = m_AIHealth.GetCurrentHealth();
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

            int currentHealth = m_PlayerHealth.GetCurrentHealth();
            m_TargetPlayerHealth = currentHealth;

            if (currentHealth < m_LastPlayerHealth)
            {
                int damage = m_LastPlayerHealth - currentHealth;
                ShowPlayerDamageText(damage);
            }
            m_LastPlayerHealth = currentHealth;

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

            int currentHealth = m_AIHealth.GetCurrentHealth();
            m_TargetAIHealth = currentHealth;

            if (currentHealth < m_LastAIHealth)
            {
                int damage = m_LastAIHealth - currentHealth;
                ShowAIDamageText(damage);
            }
            m_LastAIHealth = currentHealth;

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

        #region Damage Text Methods

        private void SetupDamageTextContainers()
        {
            if (m_PlayerDamageTextContainer == null)
            {
                GameObject playerContainer = new GameObject("PlayerDamageTextContainer");
                m_PlayerDamageTextContainer = playerContainer.AddComponent<RectTransform>();
                m_PlayerDamageTextContainer.SetParent(transform, false);
                
                m_PlayerDamageTextContainer.anchorMin = Vector2.zero;
                m_PlayerDamageTextContainer.anchorMax = Vector2.one;
                m_PlayerDamageTextContainer.offsetMin = Vector2.zero;
                m_PlayerDamageTextContainer.offsetMax = Vector2.zero;
                m_PlayerDamageTextContainer.anchoredPosition = Vector2.zero;
                m_PlayerDamageTextContainer.localScale = Vector3.one;
            }

            if (m_AIDamageTextContainer == null)
            {
                GameObject aiContainer = new GameObject("AIDamageTextContainer");
                m_AIDamageTextContainer = aiContainer.AddComponent<RectTransform>();
                m_AIDamageTextContainer.SetParent(transform, false);
                
                m_AIDamageTextContainer.anchorMin = Vector2.zero;
                m_AIDamageTextContainer.anchorMax = Vector2.one;
                m_AIDamageTextContainer.offsetMin = Vector2.zero;
                m_AIDamageTextContainer.offsetMax = Vector2.zero;
                m_AIDamageTextContainer.anchoredPosition = Vector2.zero;
                m_AIDamageTextContainer.localScale = Vector3.one;
            }
        }

        private void InitializeDamageTextPools()
        {
            if (m_DamageTextPrefab == null)
            {
                CreateDefaultDamageTextPrefab();
            }

            for (int i = 0; i < m_DamageTextPoolSize; i++)
            {
                GameObject playerText = Instantiate(m_DamageTextPrefab, m_PlayerDamageTextContainer, false);
                playerText.SetActive(false);
                m_PlayerDamageTextPool.Enqueue(playerText);
            }

            for (int i = 0; i < m_DamageTextPoolSize; i++)
            {
                GameObject aiText = Instantiate(m_DamageTextPrefab, m_AIDamageTextContainer, false);
                aiText.SetActive(false);
                m_AIDamageTextPool.Enqueue(aiText);
            }
        }

        private void CreateDefaultDamageTextPrefab()
        {
            GameObject prefab = new GameObject("DamageText");
            
            RectTransform rectTransform = prefab.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(100, 50);
            
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            
            TextMeshProUGUI textComponent = prefab.AddComponent<TextMeshProUGUI>();
            textComponent.text = "0";
            textComponent.fontSize = 24f;
            textComponent.color = m_DamageTextColor;
            textComponent.alignment = TextAlignmentOptions.Center;

            m_DamageTextPrefab = prefab;
        }

        private void ShowPlayerDamageText(int damage)
        {
            if (m_PlayerHealth == null) return;

            GameObject damageText = GetPlayerDamageTextFromPool();
            if (damageText == null) return;

            RectTransform damageRect = damageText.GetComponent<RectTransform>();
            damageRect.anchoredPosition = new Vector2(0f, 20f);
            damageRect.localScale = Vector3.one;
            damageText.SetActive(true);

            TextMeshProUGUI textComponent = damageText.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = "-" + damage.ToString();
                textComponent.color = m_DamageTextColor;
            }

            StartCoroutine(AnimateDamageText(damageText, true));
        }

        private void ShowAIDamageText(int damage)
        {
            if (m_AIHealth == null) return;

            GameObject damageText = GetAIDamageTextFromPool();
            if (damageText == null) return;

            RectTransform damageRect = damageText.GetComponent<RectTransform>();
            damageRect.anchoredPosition = new Vector2(0f, 20f); 
            damageRect.localScale = Vector3.one;
            damageText.SetActive(true);

            TextMeshProUGUI textComponent = damageText.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = "-" + damage.ToString();
                textComponent.color = m_DamageTextColor;
            }

            StartCoroutine(AnimateDamageText(damageText, false));
        }

        private GameObject GetPlayerDamageTextFromPool()
        {
            if (m_PlayerDamageTextPool.Count > 0)
            {
                return m_PlayerDamageTextPool.Dequeue();
            }
            return null;
        }

        private GameObject GetAIDamageTextFromPool()
        {
            if (m_AIDamageTextPool.Count > 0)
            {
                return m_AIDamageTextPool.Dequeue();
            }
            return null;
        }

        private IEnumerator AnimateDamageText(GameObject damageText, bool isPlayer)
        {
            RectTransform damageRect = damageText.GetComponent<RectTransform>();
            Vector2 startPos = damageRect.anchoredPosition;
            Vector2 targetPos = startPos + Vector2.up * m_DamageTextMoveSpeed * 50f;
            TextMeshProUGUI textComponent = damageText.GetComponent<TextMeshProUGUI>();
            Color startColor = m_DamageTextColor;

            float elapsed = 0f;
            while (elapsed < m_DamageTextDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / m_DamageTextDuration;

                damageRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, progress);

                if (textComponent != null)
                {
                    Color currentColor = startColor;
                    currentColor.a = Mathf.Lerp(1f, 0f, progress);
                    textComponent.color = currentColor;
                }

                yield return null;
            }

            damageText.SetActive(false);
            if (isPlayer)
            {
                m_PlayerDamageTextPool.Enqueue(damageText);
            }
            else
            {
                m_AIDamageTextPool.Enqueue(damageText);
            }
        }

        #endregion
    }
}
