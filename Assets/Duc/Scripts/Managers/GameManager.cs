using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Duc
{
    public class GameManager : SingletonManager<GameManager>
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject m_StartPanel;
        [SerializeField] private GameObject m_GameplayPanel;
        [SerializeField] private GameObject m_VictoryPanel;
        [SerializeField] private GameObject m_DefeatPanel;
        [SerializeField] private GameObject m_PausePanel;

        [Header("References")]
        [SerializeField] private TurnManager m_TurnManager;
        [SerializeField] private CameraSwitcher m_CameraSwitcher;
        [SerializeField] private GameplayInput m_GameplayInput;
        
        private PersistentGameManager m_PersistentGameManager;

        protected override void Awake()
        {
            base.Awake();
            m_PersistentGameManager = PersistentGameManager.Instance;
            
            // Initialize UI state - hide all panels initially
            SetUIPanel(m_StartPanel, false);
            SetUIPanel(m_GameplayPanel, false);
            SetUIPanel(m_VictoryPanel, false);
            SetUIPanel(m_DefeatPanel, false);
            SetUIPanel(m_PausePanel, false);

            // Disable gameplay input until StartGame is pressed
            if (m_GameplayInput == null)
                m_GameplayInput = FindObjectOfType<GameplayInput>();
            if (m_GameplayInput != null)
                m_GameplayInput.enabled = false;
        }
        
        public void SetPersistentData(PersistentGameManager persistentGameManager)
        {
            m_PersistentGameManager = persistentGameManager;
        }

        private void Start()
        {
            ReacquireReferences();
            // Show start panel with 2-second delay
            StartCoroutine(ShowStartPanelWithDelay());
        }

        private void ReacquireReferences()
        {
            // Auto-assign references if not set
            if (m_TurnManager == null)
                m_TurnManager = FindObjectOfType<TurnManager>();
            
            if (m_CameraSwitcher == null)
                m_CameraSwitcher = FindObjectOfType<CameraSwitcher>();

            if (m_GameplayInput == null)
                m_GameplayInput = FindObjectOfType<GameplayInput>();

            // Reacquire UI panel references (include inactive objects under Canvas)
            if (m_StartPanel == null)
                m_StartPanel = FindObjectIncludingInactive("StartPanel");
            if (m_GameplayPanel == null)
                m_GameplayPanel = FindObjectIncludingInactive("GameplayPanel");
            if (m_VictoryPanel == null)
                m_VictoryPanel = FindObjectIncludingInactive("VictoryPanel");
            if (m_DefeatPanel == null)
                m_DefeatPanel = FindObjectIncludingInactive("DefeatPanel");
            if (m_PausePanel == null)
                m_PausePanel = FindObjectIncludingInactive("PausePanel");

        }

        private GameObject FindObjectIncludingInactive(string name)
        {
            // Search all objects including inactive ones, filter to current scene (exclude prefabs/assets)
            var all = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < all.Length; i++)
            {
                var go = all[i];
                if (go == null) continue;
                if (go.name != name) continue;
                // Exclude assets not in a scene
                if (go.scene.IsValid() && go.scene.isLoaded)
                {
                    return go;
                }
            }
            return null;
        }


        public void StartGame()
        {
            if (m_PersistentGameManager == null) return;
            if (m_PersistentGameManager.HasGameStarted()) return;

            // Reacquire references before starting
            ReacquireReferences();

            // Show gameplay UI with delay
            SetUIPanel(m_StartPanel, false);
            StartCoroutine(ShowPanelWithDelay(m_GameplayPanel, 2f));

            // Enable gameplay input now that the game has started
            if (m_GameplayInput != null)
                m_GameplayInput.enabled = true;

            // Start the game
            if (m_TurnManager != null)
            {
                m_TurnManager.StartPlayerTurn();
            }

            // Notify persistent game manager
            m_PersistentGameManager.StartGame();
        }

        public void EndGame()
        {
            if (m_PersistentGameManager == null) return;
            if (m_PersistentGameManager.IsGameOver()) return;

            // Stop all turns
            if (m_TurnManager != null)
            {
                m_TurnManager.StopAllTurns();
            }

            // Notify persistent game manager
            m_PersistentGameManager.EndGame();
        }

        public void OnPlayerDied()
        {
            if (m_PersistentGameManager == null) return;
            if (m_PersistentGameManager.IsGameOver()) return;

            Debug.Log("Player Died - Game Over!");
            
            // Hide both bars when player dies
            if (PowerMeter.Get() != null)
            {
                PowerMeter.Get().gameObject.SetActive(false);
            }

            if (CounterSystem.Get() != null)
            {
                CounterSystem.Get().StopCounter();
            }
            
            // Show defeat panel with delay
            SetUIPanel(m_GameplayPanel, false);
            StartCoroutine(ShowPanelWithDelay(m_DefeatPanel, 3f));

            // Award defeat reward
            var coinManager = CoinManager.Get();
            if (coinManager != null)
            {
                coinManager.OnPlayerDefeat();
            }

            // Notify persistent game manager
            m_PersistentGameManager.OnPlayerDied();
        }

        public void OnAIDied()
        {
            if (m_PersistentGameManager == null) return;
            if (m_PersistentGameManager.IsGameOver()) return;

            Debug.Log("AI Died - Player Victory!");
            
            // Hide both bars when AI dies (player wins)
            if (PowerMeter.Get() != null)
            {
                PowerMeter.Get().gameObject.SetActive(false);
            }

            if (CounterSystem.Get() != null)
            {
                CounterSystem.Get().StopCounter();
            }
            
            SetUIPanel(m_GameplayPanel, false);
            StartCoroutine(ShowPanelWithDelay(m_VictoryPanel, 3f));

            var coinManager = CoinManager.Get();
            if (coinManager != null)
            {
                coinManager.OnPlayerVictory();
            }

            m_PersistentGameManager.OnAIDied();
        }

        public void RestartGame()
        {
            if (m_TurnManager != null)
            {
                m_TurnManager.ResetGame();
            }

            if (m_GameplayInput != null) m_GameplayInput.enabled = false;

            if (m_PersistentGameManager != null)
            {
                m_PersistentGameManager.RestartGame();
            }
        }

        private void SetUIPanel(GameObject panel, bool active)
        {
            if (panel != null)
            {
                panel.SetActive(active);
            }
        }

        public bool HasGameStarted() => m_PersistentGameManager != null ? m_PersistentGameManager.HasGameStarted() : false;
        public bool IsGameOver() => m_PersistentGameManager != null ? m_PersistentGameManager.IsGameOver() : false;
        public bool IsPaused() => m_PersistentGameManager != null ? m_PersistentGameManager.IsPaused() : false;

        public void OnStartButtonClick()
        {
            StartGame();
        }

        public void OnRestartButtonClick()
        {
            RestartGame();
        }

        private IEnumerator ShowStartPanelWithDelay()
        {
            yield return new WaitForSeconds(2f);
            SetUIPanel(m_StartPanel, true);
        }

        private IEnumerator ShowPanelWithDelay(GameObject panel, float delay)
        {
            yield return new WaitForSeconds(delay);
            SetUIPanel(panel, true);
        }

        protected override void OnInitialize()
        {
            // GameManager specific initialization
            m_PersistentGameManager = PersistentGameManager.Instance;
            
            // Initialize UI state - hide all panels initially
            SetUIPanel(m_StartPanel, false);
            SetUIPanel(m_GameplayPanel, false);
            SetUIPanel(m_VictoryPanel, false);
            SetUIPanel(m_DefeatPanel, false);
            SetUIPanel(m_PausePanel, false);

            // Disable gameplay input until StartGame is pressed
            if (m_GameplayInput == null)
                m_GameplayInput = FindObjectOfType<GameplayInput>();
            if (m_GameplayInput != null)
                m_GameplayInput.enabled = false;
        }

        protected override void OnCleanup()
        {
            // GameManager specific cleanup
            if (m_TurnManager != null)
            {
                m_TurnManager.StopAllTurns();
            }
            
        if (m_GameplayInput != null)
        {
            m_GameplayInput.enabled = false;
        }
    }

    private void OnApplicationQuit()
    {
        // Cleanup all singleton instances
        SingletonCleanup.CleanupAllSingletons();
        
        // Stop all turns
        if (m_TurnManager != null)
        {
            m_TurnManager.StopAllTurns();
        }
    }
}
}
