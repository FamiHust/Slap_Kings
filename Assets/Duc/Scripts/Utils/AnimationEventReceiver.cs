using UnityEngine;

namespace Duc
{
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
                    aiSM.OnSlapHit();
                    return;
                }
            }

            var turnManager = FindObjectOfType<TurnManager>();
            if (turnManager != null)
            {
                if (m_ActorType == ActorType.Player)
                {
                    turnManager.ApplyPlayerDamage();
                }
                else
                {
                    turnManager.ApplyAIDamage();
                }
            }
        }
    }
}


