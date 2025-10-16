using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private CinemachineVirtualCamera m_Cam1;
    [SerializeField] private CinemachineVirtualCamera m_Cam2;

    [SerializeField] private GameplayInput m_GameplayInput;
    // AITurnManager removed - now handled by TurnManager

    private bool isCam1Active = true;

    private void Start()
    {
        m_Cam1.Priority = 20; // Start with Player camera active
        m_Cam2.Priority = 0;
        Debug.Log("CameraSwitcher: Initialized - Cam1 (Player) active, Cam2 (AI) inactive");
    }

    public void SwitchCamera()
    {
        bool previousState = isCam1Active;
        isCam1Active = !isCam1Active;
        
        Debug.Log($"CameraSwitcher: Previous state = {(previousState ? "Player (Cam1)" : "AI (Cam2)")}, Switching to {(isCam1Active ? "Player (Cam1)" : "AI (Cam2)")}");

        if (isCam1Active)
        {
            // Switch to Player turn (vcam 1)
            m_Cam1.Priority = 20; // Higher priority for smoother transition
            m_Cam2.Priority = 0;
            m_GameplayInput.enabled = true;
            Debug.Log($"CameraSwitcher: Set Cam1 Priority = 20, Cam2 Priority = 0 (Player Turn)");
        }
        else
        {
            // Switch to AI turn (vcam 2)
            m_Cam1.Priority = 0;
            m_Cam2.Priority = 20; // Higher priority for smoother transition
            m_GameplayInput.enabled = false;
            Debug.Log($"CameraSwitcher: Set Cam1 Priority = 0, Cam2 Priority = 20 (AI Turn)");
        }
    }
    
    /// <summary>
    /// Force switch to Player camera (Cam1)
    /// </summary>
    public void SwitchToPlayerCamera()
    {
        isCam1Active = true;
        m_Cam1.Priority = 20;
        m_Cam2.Priority = 0;
        m_GameplayInput.enabled = true;
        Debug.Log("CameraSwitcher: Force switched to Player (Cam1)");
    }
    
    /// <summary>
    /// Force switch to AI camera (Cam2)
    /// </summary>
    public void SwitchToAICamera()
    {
        isCam1Active = false;
        m_Cam1.Priority = 0;
        m_Cam2.Priority = 20;
        m_GameplayInput.enabled = false;
        Debug.Log("CameraSwitcher: Force switched to AI (Cam2)");
    }
}
