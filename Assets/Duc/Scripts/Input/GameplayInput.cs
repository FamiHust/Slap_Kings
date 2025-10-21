using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

namespace Duc
{
    public class GameplayInput : MonoBehaviour
    {
        private InputAction m_TapAction;
        public bool isMeterActive = false;

        [SerializeField] private CameraSwitcher m_CameraSwitcher;
        [SerializeField] private AIHealth m_AIHealth;
        [SerializeField] private TurnManager m_TurnManager;
        private GameplayInput m_GameplayInput;

        void Awake()
        {
            var m_PlayerInput = GetComponent<PlayerInput>();
            m_GameplayInput = GetComponent<GameplayInput>();
            m_TapAction = m_PlayerInput.actions["Tap"];
            
            if (m_TurnManager == null)
                m_TurnManager = FindObjectOfType<TurnManager>();
        }

        void OnEnable()
        {
            m_TapAction.Enable();
            m_TapAction.performed += OnTapPerformed;
        }

        void OnDisable()
        {
            m_TapAction.performed -= OnTapPerformed;
            m_TapAction.Disable();
        }

        private void OnTapPerformed(InputAction.CallbackContext ctx)
        {
            // Only allow tap after game started and during player's turn
            var gameManager = GameManager.Get();
            bool gameStarted = gameManager != null && gameManager.HasGameStarted();
            if (gameStarted && m_TurnManager != null && m_TurnManager.IsPlayerTurn())
            {
                if (PowerMeter.Get() != null)
                {
                    PowerMeter.Get().StopMeter();
                }
                
                if (m_TurnManager != null)
                {
                    m_TurnManager.PlayerAttacks();
                }
            }
        }

        private IEnumerator DelaySwitchCamera(float delay)
        {
            yield return new WaitForSeconds(delay);
            m_CameraSwitcher.SwitchCamera();
        }
    }
}
