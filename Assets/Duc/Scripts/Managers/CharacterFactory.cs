using UnityEngine;

namespace Duc
{
    /// <summary>
    /// Interface for character factories
    /// </summary>
    public interface ICharacterFactory
    {
        GameObject CreatePlayer(Vector3 position, Quaternion rotation);
        GameObject CreateAI(Vector3 position, Quaternion rotation);
        GameObject CreateCharacter(CharacterType type, Vector3 position, Quaternion rotation);
    }

    /// <summary>
    /// Character types
    /// </summary>
    public enum CharacterType
    {
        Player,
        AI
    }

    /// <summary>
    /// Character creation data
    /// </summary>
    [System.Serializable]
    public class CharacterCreationData
    {
        [Header("Prefab References")]
        public GameObject playerPrefab;
        public GameObject aiPrefab;
        
        [Header("Default Settings")]
        public Vector3 defaultPosition = Vector3.zero;
        public Quaternion defaultRotation = Quaternion.identity;
        
        [Header("Health Settings")]
        public int defaultPlayerHealth = 100;
        public int defaultAIHealth = 200;
    }

    /// <summary>
    /// Factory for creating characters
    /// </summary>
    public class CharacterFactory : MonoBehaviour, ICharacterFactory
    {
        [Header("Character Creation Data")]
        [SerializeField] private CharacterCreationData m_CreationData = new CharacterCreationData();
        
        [Header("References")]
        [SerializeField] private Transform m_PlayerSpawnPoint;
        [SerializeField] private Transform m_AISpawnPoint;
        
        private void Awake()
        {
            // Register this factory with ServiceLocator
            ServiceLocator.Instance.Register<ICharacterFactory>(this);
        }
        
        public GameObject CreatePlayer(Vector3 position, Quaternion rotation)
        {
            if (m_CreationData.playerPrefab == null)
            {
                Debug.LogError("Player prefab is not assigned!");
                return null;
            }
            
            GameObject player = Instantiate(m_CreationData.playerPrefab, position, rotation);
            
            // Initialize player components
            InitializePlayer(player);
            
            return player;
        }
        
        public GameObject CreateAI(Vector3 position, Quaternion rotation)
        {
            if (m_CreationData.aiPrefab == null)
            {
                Debug.LogError("AI prefab is not assigned!");
                return null;
            }
            
            GameObject ai = Instantiate(m_CreationData.aiPrefab, position, rotation);
            
            // Initialize AI components
            InitializeAI(ai);
            
            return ai;
        }
        
        public GameObject CreateCharacter(CharacterType type, Vector3 position, Quaternion rotation)
        {
            switch (type)
            {
                case CharacterType.Player:
                    return CreatePlayer(position, rotation);
                case CharacterType.AI:
                    return CreateAI(position, rotation);
                default:
                    Debug.LogError($"Unknown character type: {type}");
                    return null;
            }
        }
        
        private void InitializePlayer(GameObject player)
        {
            // Set up player health
            var playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Health will be initialized by the PlayerHealth component
            }
            
            // Set up player state machine
            var playerStateMachine = player.GetComponent<PlayerStateMachine>();
            if (playerStateMachine != null)
            {
                // State machine will be initialized automatically
            }
            
            // Set up player input
            var gameplayInput = player.GetComponent<GameplayInput>();
            if (gameplayInput != null)
            {
                gameplayInput.enabled = false; // Will be enabled when game starts
            }
            
            Debug.Log("Player initialized successfully");
        }
        
        private void InitializeAI(GameObject ai)
        {
            // Set up AI health
            var aiHealth = ai.GetComponent<AIHealth>();
            if (aiHealth != null)
            {
                // Health will be initialized by the AIHealth component
            }
            
            // Set up AI state machine
            var aiStateMachine = ai.GetComponent<AIStateMachine>();
            if (aiStateMachine != null)
            {
                // State machine will be initialized automatically
            }
            
            Debug.Log("AI initialized successfully");
        }
        
        /// <summary>
        /// Create player at default spawn point
        /// </summary>
        public GameObject CreatePlayerAtSpawnPoint()
        {
            Vector3 position = m_PlayerSpawnPoint != null ? m_PlayerSpawnPoint.position : m_CreationData.defaultPosition;
            Quaternion rotation = m_PlayerSpawnPoint != null ? m_PlayerSpawnPoint.rotation : m_CreationData.defaultRotation;
            
            return CreatePlayer(position, rotation);
        }
        
        /// <summary>
        /// Create AI at default spawn point
        /// </summary>
        public GameObject CreateAIAtSpawnPoint()
        {
            Vector3 position = m_AISpawnPoint != null ? m_AISpawnPoint.position : m_CreationData.defaultPosition;
            Quaternion rotation = m_AISpawnPoint != null ? m_AISpawnPoint.rotation : m_CreationData.defaultRotation;
            
            return CreateAI(position, rotation);
        }
        
        /// <summary>
        /// Set spawn points
        /// </summary>
        public void SetSpawnPoints(Transform playerSpawn, Transform aiSpawn)
        {
            m_PlayerSpawnPoint = playerSpawn;
            m_AISpawnPoint = aiSpawn;
        }
        
        /// <summary>
        /// Update creation data
        /// </summary>
        public void UpdateCreationData(CharacterCreationData newData)
        {
            m_CreationData = newData;
        }
    }
}
