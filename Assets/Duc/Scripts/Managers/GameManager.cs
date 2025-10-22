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
        [SerializeField] private MapManager m_MapManager;
        
        private PersistentGameManager m_PersistentGameManager;

        protected override void Awake()
        {
            base.Awake();
            m_PersistentGameManager = PersistentGameManager.Instance;
            
            SetUIPanel(m_StartPanel, false);
            SetUIPanel(m_GameplayPanel, false);
            SetUIPanel(m_VictoryPanel, false);
            SetUIPanel(m_DefeatPanel, false);
            SetUIPanel(m_PausePanel, false);

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
            StartCoroutine(ShowStartPanelWithDelay());
        }

        private void ReacquireReferences()
        {
            if (m_TurnManager == null)
                m_TurnManager = FindObjectOfType<TurnManager>();
            if (m_CameraSwitcher == null)
                m_CameraSwitcher = FindObjectOfType<CameraSwitcher>();
            if (m_GameplayInput == null)
                m_GameplayInput = FindObjectOfType<GameplayInput>();
            if (m_MapManager == null)
                m_MapManager = FindObjectOfType<MapManager>();
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
            var all = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < all.Length; i++)
            {
                var go = all[i];
                if (go == null) continue;
                if (go.name != name) continue;
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

            ReacquireReferences();

            SetUIPanel(m_StartPanel, false);
            StartCoroutine(ShowPanelWithDelay(m_GameplayPanel, 2f));

            if (m_GameplayInput != null)
                m_GameplayInput.enabled = true;

            if (m_TurnManager != null)
            {
                m_TurnManager.StartPlayerTurn();
            }

            m_PersistentGameManager.StartGame();
        }

        public void EndGame()
        {
            if (m_PersistentGameManager == null) return;
            if (m_PersistentGameManager.IsGameOver()) return;

            if (m_TurnManager != null)
            {
                m_TurnManager.StopAllTurns();
            }

            m_PersistentGameManager.EndGame();
        }

        public void OnPlayerDied()
        {
            if (m_PersistentGameManager == null) return;
            if (m_PersistentGameManager.IsGameOver()) return;

            if (PowerMeter.Get() != null)
            {
                PowerMeter.Get().gameObject.SetActive(false);
            }

            if (CounterSystem.Get() != null)
            {
                CounterSystem.Get().StopCounter();
            }
            
            SetUIPanel(m_GameplayPanel, false);
            StartCoroutine(ShowPanelWithDelay(m_DefeatPanel, 3f));

            var coinManager = CoinManager.Get();
            if (coinManager != null)
            {
                coinManager.OnPlayerDefeat();
            }

            m_PersistentGameManager.OnPlayerDied();
        }

        public void OnAIDied()
        {
            if (m_PersistentGameManager == null) return;
            if (m_PersistentGameManager.IsGameOver()) return;

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

            // Map will be updated when restarting game, not immediately on victory

            m_PersistentGameManager.OnAIDied();
        }

        public void RestartGame()
        {
            if (m_TurnManager != null)
            {
                m_TurnManager.ResetGame();
            }

            if (m_GameplayInput != null) m_GameplayInput.enabled = false;

            // Map will be updated automatically when scene loads

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
            m_PersistentGameManager = PersistentGameManager.Instance;
            
            SetUIPanel(m_StartPanel, false);
            SetUIPanel(m_GameplayPanel, false);
            SetUIPanel(m_VictoryPanel, false);
            SetUIPanel(m_DefeatPanel, false);
            SetUIPanel(m_PausePanel, false);

            if (m_GameplayInput == null)
                m_GameplayInput = FindObjectOfType<GameplayInput>();
            if (m_GameplayInput != null)
                m_GameplayInput.enabled = false;
        }

        protected override void OnCleanup()
        {
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
        SingletonCleanup.CleanupAllSingletons();
        
        if (m_TurnManager != null)
        {
            m_TurnManager.StopAllTurns();
        }
    }
}
}
