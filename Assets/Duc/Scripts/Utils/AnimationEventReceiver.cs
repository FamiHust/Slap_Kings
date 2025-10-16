using UnityEngine;

public class AnimationEventReceiver : MonoBehaviour
{
    public enum ActorType
    {
        Player,
        AI
    }

    [SerializeField] private ActorType m_ActorType = ActorType.Player;
    [SerializeField] private bool m_EnableDebugLogs = true;

    public void OnSlapHit()
    {
        if (m_ActorType == ActorType.Player)
        {
            var playerSM = GetComponent<PlayerStateMachine>();
            if (playerSM == null) playerSM = GetComponentInParent<PlayerStateMachine>();
            if (playerSM != null)
            {
                if (m_EnableDebugLogs) Debug.Log("AnimationEventReceiver: Forwarding OnSlapHit to PlayerStateMachine");
                playerSM.OnSlapHit();
                return;
            }
        }
        else 
        {
            var aiSM = GetComponent<AIStateMachine>();
            if (aiSM == null) aiSM = GetComponentInParent<AIStateMachine>();
            if (aiSM != null)
            {
                if (m_EnableDebugLogs) Debug.Log("AnimationEventReceiver: Forwarding OnSlapHit to AIStateMachine");
                aiSM.OnSlapHit();
                return;
            }
        }

        var turnManager = FindObjectOfType<TurnManager>();
        if (turnManager != null)
        {
            if (m_ActorType == ActorType.Player)
            {
                if (m_EnableDebugLogs) Debug.Log("AnimationEventReceiver: Fallback ApplyPlayerDamage()");
                turnManager.ApplyPlayerDamage();
            }
            else
            {
                if (m_EnableDebugLogs) Debug.Log("AnimationEventReceiver: Fallback ApplyAIDamage()");
                turnManager.ApplyAIDamage();
            }
        }
        else if (m_EnableDebugLogs)
        {
            Debug.LogWarning("AnimationEventReceiver: Neither StateMachine nor TurnManager found to receive OnSlapHit.");
        }
    }

}


