using UnityEngine;

namespace Duc
{
    public interface ICharacterFactory
    {
        GameObject CreatePlayer(Vector3 position, Quaternion rotation);
        GameObject CreateAI(Vector3 position, Quaternion rotation);
        GameObject CreateCharacter(CharacterType type, Vector3 position, Quaternion rotation);
    }

    public enum CharacterType
    {
        Player,
        AI
    }

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

    public class CharacterFactory : MonoBehaviour, ICharacterFactory
    {
        [Header("Character Creation Data")]
        [SerializeField] private CharacterCreationData m_CreationData = new CharacterCreationData();
        
        [Header("References")]
        [SerializeField] private Transform m_PlayerSpawnPoint;
        [SerializeField] private Transform m_AISpawnPoint;
        
        private void Awake()
        {
            ServiceLocator.Instance.Register<ICharacterFactory>(this);
        }
        
        public GameObject CreatePlayer(Vector3 position, Quaternion rotation)
        {
            if (m_CreationData.playerPrefab == null)
            {
                return null;
            }
            
            GameObject player = Instantiate(m_CreationData.playerPrefab, position, rotation);
            
            InitializePlayer(player);
            
            return player;
        }
        
        public GameObject CreateAI(Vector3 position, Quaternion rotation)
        {
            if (m_CreationData.aiPrefab == null)
            {
                return null;
            }
            
            GameObject ai = Instantiate(m_CreationData.aiPrefab, position, rotation);
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
                    return null;
            }
        }
        
        private void InitializePlayer(GameObject player)
        {
            var playerHealth = player.GetComponent<PlayerHealth>();
            
            var playerStateMachine = player.GetComponent<PlayerStateMachine>();
            
            var gameplayInput = player.GetComponent<GameplayInput>();
            if (gameplayInput != null)
            {
                gameplayInput.enabled = false; 
            }
        }
        
        private void InitializeAI(GameObject ai)
        {
            var aiHealth = ai.GetComponent<AIHealth>();

            var aiStateMachine = ai.GetComponent<AIStateMachine>();
            
            var appearanceManager = ai.GetComponent<AIAppearanceManager>();
            if (appearanceManager == null)
            {
                appearanceManager = ai.AddComponent<AIAppearanceManager>();
            }
            
            var dataManager = DataManager.Get();
            if (dataManager != null && dataManager.AIStats != null && dataManager.AIStats.appearanceData != null)
            {
                appearanceManager.SetAppearanceData(dataManager.AIStats.appearanceData);
            }
        }

        public GameObject CreatePlayerAtSpawnPoint()
        {
            Vector3 position = m_PlayerSpawnPoint != null ? m_PlayerSpawnPoint.position : m_CreationData.defaultPosition;
            Quaternion rotation = m_PlayerSpawnPoint != null ? m_PlayerSpawnPoint.rotation : m_CreationData.defaultRotation;
            
            return CreatePlayer(position, rotation);
        }

        public GameObject CreateAIAtSpawnPoint()
        {
            Vector3 position = m_AISpawnPoint != null ? m_AISpawnPoint.position : m_CreationData.defaultPosition;
            Quaternion rotation = m_AISpawnPoint != null ? m_AISpawnPoint.rotation : m_CreationData.defaultRotation;
            
            return CreateAI(position, rotation);
        }

        public void SetSpawnPoints(Transform playerSpawn, Transform aiSpawn)
        {
            m_PlayerSpawnPoint = playerSpawn;
            m_AISpawnPoint = aiSpawn;
        }

        public void UpdateCreationData(CharacterCreationData newData)
        {
            m_CreationData = newData;
        }
    }
}
