using UnityEngine;
using UnityEngine.SceneManagement;

namespace Duc
{
    public class PersistentGameManager : MonoBehaviour
    {
        private static PersistentGameManager s_Instance;
        
        private bool m_HasGameStarted = false;
        private bool m_IsGameOver = false;
        private bool m_IsPaused = false;
        
        public System.Action OnGameStart;
        public System.Action OnGameOver;
        public System.Action OnPlayerVictory;
        public System.Action OnPlayerDefeat;
        public System.Action OnGamePause;
        public System.Action OnGameResume;
        
        public static PersistentGameManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = FindObjectOfType<PersistentGameManager>();
                    if (s_Instance == null)
                    {
                        GameObject go = new GameObject("PersistentGameManager");
                        s_Instance = go.AddComponent<PersistentGameManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return s_Instance;
            }
        }
        
        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (s_Instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void OnEnable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            m_HasGameStarted = false;
            m_IsGameOver = false;
            m_IsPaused = false;
            
            var gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.SetPersistentData(this);
            }
        }
        
        public bool HasGameStarted() => m_HasGameStarted;
        public bool IsGameOver() => m_IsGameOver;
        public bool IsPaused() => m_IsPaused;
        
        public void SetGameStarted(bool started) => m_HasGameStarted = started;
        public void SetGameOver(bool gameOver) => m_IsGameOver = gameOver;
        public void SetPaused(bool paused) => m_IsPaused = paused;
        
        public void StartGame()
        {
            if (m_HasGameStarted) return;
            
            m_HasGameStarted = true;
            m_IsGameOver = false;
            m_IsPaused = false;
            
            OnGameStart?.Invoke();
        }
        
        public void EndGame()
        {
            if (m_IsGameOver) return;
            
            m_IsGameOver = true;
            m_HasGameStarted = false;
            
            OnGameOver?.Invoke();
        }
        
        public void OnPlayerDied()
        {
            if (m_IsGameOver) return;
            
            OnPlayerDefeat?.Invoke();
            EndGame();
        }
        
        public void OnAIDied()
        {
            if (m_IsGameOver) return;
            
            OnPlayerVictory?.Invoke();
            EndGame();
        }
        
        public void RestartGame()
        {
            m_HasGameStarted = false;
            m_IsGameOver = false;
            m_IsPaused = false;
                    
            SceneManager.LoadScene(0);
        }

        private void OnApplicationQuit()
        {
            OnGameStart = null;
            OnGameOver = null;
            OnPlayerVictory = null;
            OnPlayerDefeat = null;
            OnGamePause = null;
            OnGameResume = null;
        }

        private void OnDestroy()
        {
            if (s_Instance == this)
            {
                s_Instance = null;
            }
        }
    }
}
