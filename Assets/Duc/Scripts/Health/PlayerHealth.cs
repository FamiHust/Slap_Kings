using UnityEngine;

public class PlayerHealth : HealthManager
{
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
        else
        {
            m_HealthUI.SetPlayerHealth(this);
        }
    }

    protected override void HandleDeath()
    {
        Debug.Log("Player has died! Game Over!");
        
        var gameplayInput = GetComponent<GameplayInput>();
        if (gameplayInput != null)
        {
            gameplayInput.enabled = false;
        }
    }
}
