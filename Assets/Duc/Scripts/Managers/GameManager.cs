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
            
            // Play defeated sound
            var soundManager = SoundManager.Get();
            if (soundManager != null)
            {
                soundManager.PlaySound(SoundManager.SoundType.Defeated);
            }
            
            SetUIPanel(m_GameplayPanel, false);
            StartCoroutine(ShowPanelWithDelay(m_DefeatPanel, 3f));
            StartCoroutine(AddDefeatRewardWithDelay(3f));

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
            
            // Play victory sound
            var soundManager = SoundManager.Get();
            if (soundManager != null)
            {
                soundManager.PlaySound(SoundManager.SoundType.Victory);
            }
            
            SetUIPanel(m_GameplayPanel, false);
            StartCoroutine(ShowPanelWithDelay(m_VictoryPanel, 3f));
            StartCoroutine(AddVictoryRewardWithDelay(3f));

            m_PersistentGameManager.OnAIDied();
        }

        public void RestartGame()
        {
            if (m_TurnManager != null)
            {
                m_TurnManager.ResetGame();
            }

            if (m_GameplayInput != null) m_GameplayInput.enabled = false;

            var loadingPanel = FindObjectIncludingInactive("LoadingPanel");
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(true);
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
            
            // Unlock the victory reward display so it updates for the next round
            var coinDisplay = FindObjectOfType<CoinDisplay>();
            if (coinDisplay != null)
            {
                coinDisplay.UnlockVictoryReward();
            }
            
            SetUIPanel(m_StartPanel, true);
        }

        private IEnumerator ShowPanelWithDelay(GameObject panel, float delay)
        {
            yield return new WaitForSeconds(delay);
            SetUIPanel(panel, true);
        }
        
        private IEnumerator AddDefeatRewardWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            var rewardManager = RewardManager.Get();
            if (rewardManager != null && m_DefeatPanel != null)
            {
                Vector3 panelPosition = m_DefeatPanel.transform.position;
                rewardManager.ShowRewardCoinsFromPosition(5, panelPosition);
            }
            
            yield return new WaitForSeconds(2f);
            
            var coinManager = CoinManager.Get();
            if (coinManager != null)
            {
                coinManager.OnPlayerDefeat();
                
                var coinDisplay = FindObjectOfType<CoinDisplay>();
                if (coinDisplay != null)
                {
                    coinDisplay.UpdateDisplay();
                }
            }
        }
        
        private IEnumerator AddVictoryRewardWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            var coinManager = CoinManager.Get();
            int rewardAmount = 0;
            
            if (coinManager != null)
            {
                rewardAmount = coinManager.CalculateReward();
            }
            
            var coinDisplay = FindObjectOfType<CoinDisplay>();
            if (coinDisplay != null)
            {
                coinDisplay.LockVictoryReward(rewardAmount);
            }
            
            var rewardManager = RewardManager.Get();
            if (rewardManager != null && m_VictoryPanel != null)
            {
                Vector3 panelPosition = m_VictoryPanel.transform.position;
                rewardManager.ShowRewardCoinsFromPosition(rewardAmount, panelPosition);
            }
            
            yield return new WaitForSeconds(2f); 
            
            if (coinManager != null)
            {
                coinManager.OnPlayerVictory();
            }
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
