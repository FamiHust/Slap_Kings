using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentGameManager : MonoBehaviour
{
    private static PersistentGameManager s_Instance;
    
    // Game state
    private bool m_HasGameStarted = false;
    private bool m_IsGameOver = false;
    private bool m_IsPaused = false;
    
    // Events
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
        // Reset game state on scene load
        m_HasGameStarted = false;
        m_IsGameOver = false;
        m_IsPaused = false;
        
        // Find and setup GameManager in new scene
        var gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.SetPersistentData(this);
        }
    }
    
    // Getters
    public bool HasGameStarted() => m_HasGameStarted;
    public bool IsGameOver() => m_IsGameOver;
    public bool IsPaused() => m_IsPaused;
    
    // Setters
    public void SetGameStarted(bool started) => m_HasGameStarted = started;
    public void SetGameOver(bool gameOver) => m_IsGameOver = gameOver;
    public void SetPaused(bool paused) => m_IsPaused = paused;
    
    // Game control methods
    public void StartGame()
    {
        if (m_HasGameStarted) return;
        
        m_HasGameStarted = true;
        m_IsGameOver = false;
        m_IsPaused = false;
        
        OnGameStart?.Invoke();
        Debug.Log("Game Started!");
    }
    
    public void EndGame()
    {
        if (m_IsGameOver) return;
        
        m_IsGameOver = true;
        m_HasGameStarted = false;
        
        OnGameOver?.Invoke();
        Debug.Log("Game Ended!");
    }
    
    public void OnPlayerDied()
    {
        if (m_IsGameOver) return;
        
        Debug.Log("Player Died - Game Over!");
        OnPlayerDefeat?.Invoke();
        EndGame();
    }
    
    public void OnAIDied()
    {
        if (m_IsGameOver) return;
        
        Debug.Log("AI Died - Player Victory!");
        OnPlayerVictory?.Invoke();
        EndGame();
    }
    
    public void RestartGame()
    {
        // Reset game state first
        m_HasGameStarted = false;
        m_IsGameOver = false;
        m_IsPaused = false;
        
        // Ensure time scale normal
        Time.timeScale = 1f;
        
        // Load scene last
        SceneManager.LoadScene(0);
    }
}
