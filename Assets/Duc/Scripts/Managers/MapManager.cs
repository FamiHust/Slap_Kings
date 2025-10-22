using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Duc
{
    public class MapManager : SingletonManager<MapManager>
    {
        [Header("Map Configuration")]
        [SerializeField] private MapData m_MapData;
        [SerializeField] private Transform m_MapContainer;
        
        [Header("Object Pooling Settings")]
        [SerializeField] private int m_InitialPoolSize = 2;
        [SerializeField] private bool m_ExpandPool = true;
        
        [Header("Map Transition")]
        [SerializeField] private float m_TransitionDuration = 1f;
        [SerializeField] private AnimationCurve m_TransitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        // Object Pool
        private Dictionary<GameObject, Queue<GameObject>> m_MapPools = new Dictionary<GameObject, Queue<GameObject>>();
        private Dictionary<GameObject, List<GameObject>> m_ActiveMaps = new Dictionary<GameObject, List<GameObject>>();
        
        // Current state
        private MapData.MapInfo m_CurrentMapInfo;
        private Coroutine m_TransitionCoroutine;
        
        // Events
        public System.Action<MapData.MapInfo> OnMapChanged;
        public System.Action<int> OnLevelChanged;
        
        public MapData MapData => m_MapData;
        public int CurrentLevel => GetCurrentLevelFromPersistentData();
        public MapData.MapInfo CurrentMapInfo => m_CurrentMapInfo;
        
        protected override void Awake()
        {
            base.Awake();
            
            if (m_MapContainer == null)
            {
                GameObject container = new GameObject("MapContainer");
                m_MapContainer = container.transform;
                m_MapContainer.SetParent(transform);
            }
            
            InitializeMapPools();
        }
        
        protected override void OnInitialize()
        {
            // Subscribe to scene loaded events
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            
            // Load current level from persistent data
            LoadCurrentLevel();
        }
        
        protected override void OnCleanup()
        {
            StopAllCoroutines();
            ClearAllMaps();
            ClearAllPools();
            
            // Unsubscribe from scene loaded events
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        private void InitializeMapPools()
        {
            if (m_MapData == null)
            {
                Debug.LogError("MapData is not assigned!");
                return;
            }
            
            foreach (var mapInfo in m_MapData.Maps)
            {
                if (mapInfo.mapPrefab != null)
                {
                    CreatePoolForMap(mapInfo.mapPrefab);
                }
            }
            
            if (m_EnableDebugLogs)
            {
                Debug.Log($"Initialized {m_MapPools.Count} map pools");
            }
        }
        
        private void CreatePoolForMap(GameObject mapPrefab)
        {
            if (m_MapPools.ContainsKey(mapPrefab))
                return;
            
            Queue<GameObject> pool = new Queue<GameObject>();
            List<GameObject> activeList = new List<GameObject>();
            
            // Pre-instantiate objects
            for (int i = 0; i < m_InitialPoolSize; i++)
            {
                GameObject instance = Instantiate(mapPrefab, m_MapContainer);
                instance.SetActive(false);
                pool.Enqueue(instance);
            }
            
            m_MapPools[mapPrefab] = pool;
            m_ActiveMaps[mapPrefab] = activeList;
            
            if (m_EnableDebugLogs)
            {
                Debug.Log($"Created pool for map: {mapPrefab.name}");
            }
        }
        
        private int GetCurrentLevelFromPersistentData()
        {
            var persistentDataManager = PersistentDataManager.Instance;
            if (persistentDataManager != null)
            {
                return persistentDataManager.GetLevelCount();
            }
            return 1; // Default level
        }
        
        private void LoadCurrentLevel()
        {
            UpdateMapForCurrentLevel();
        }
        
        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if (m_EnableDebugLogs)
            {
                Debug.Log($"Scene loaded: {scene.name}, updating map...");
            }
            
            // Update map after scene is fully loaded
            UpdateMapForCurrentLevel();
        }
        
        public void SetLevel(int level)
        {
            // Level is managed by PersistentDataManager, we just update the map
            OnLevelChanged?.Invoke(level);
            UpdateMapForCurrentLevel();
        }
        
        public void OnLevelUp()
        {
            // Level up is handled by PersistentDataManager, we just update the map
            UpdateMapForCurrentLevel();
        }
        
        private void UpdateMapForCurrentLevel()
        {
            if (m_MapData == null)
            {
                Debug.LogError("MapData is not assigned!");
                return;
            }
            
            int currentLevel = GetCurrentLevelFromPersistentData();
            MapData.MapInfo newMapInfo = m_MapData.GetMapForLevel(currentLevel);
            
            if (newMapInfo == null)
            {
                Debug.LogWarning($"No map found for level {currentLevel}");
                return;
            }
            
            if (m_CurrentMapInfo == newMapInfo)
            {
                if (m_EnableDebugLogs)
                {
                    Debug.Log($"Map for level {currentLevel} is already active: {newMapInfo.mapName}");
                }
                return;
            }
            
            if (m_EnableDebugLogs)
            {
                Debug.Log($"Changing map for level {currentLevel}: {newMapInfo.mapName}");
            }
            
            // Start transition
            if (m_TransitionCoroutine != null)
            {
                StopCoroutine(m_TransitionCoroutine);
            }
            
            m_TransitionCoroutine = StartCoroutine(TransitionToMap(newMapInfo));
        }
        
        private IEnumerator TransitionToMap(MapData.MapInfo newMapInfo)
        {
            // Deactivate current maps
            DeactivateAllMaps();
            
            // Wait for destroy delay
            if (m_CurrentMapInfo != null && m_CurrentMapInfo.destroyDelay > 0)
            {
                yield return new WaitForSeconds(m_CurrentMapInfo.destroyDelay);
            }
            
            // Wait for spawn delay
            if (newMapInfo.spawnDelay > 0)
            {
                yield return new WaitForSeconds(newMapInfo.spawnDelay);
            }
            
            // Activate new map
            ActivateMap(newMapInfo);
            
            // Update current map info
            m_CurrentMapInfo = newMapInfo;
            OnMapChanged?.Invoke(newMapInfo);
            
            m_TransitionCoroutine = null;
        }
        
        private void ActivateMap(MapData.MapInfo mapInfo)
        {
            if (mapInfo.mapPrefab == null)
            {
                Debug.LogError($"Map prefab is null for map: {mapInfo.mapName}");
                return;
            }
            
            GameObject mapInstance = GetMapFromPool(mapInfo.mapPrefab);
            if (mapInstance != null)
            {
                mapInstance.SetActive(true);
                m_ActiveMaps[mapInfo.mapPrefab].Add(mapInstance);
                
                if (m_EnableDebugLogs)
                {
                    Debug.Log($"Activated map: {mapInfo.mapName}");
                }
            }
        }
        
        private void DeactivateAllMaps()
        {
            foreach (var kvp in m_ActiveMaps)
            {
                foreach (var mapInstance in kvp.Value.ToList())
                {
                    ReturnMapToPool(kvp.Key, mapInstance);
                }
                kvp.Value.Clear();
            }
        }
        
        private GameObject GetMapFromPool(GameObject mapPrefab)
        {
            if (!m_MapPools.ContainsKey(mapPrefab))
            {
                CreatePoolForMap(mapPrefab);
            }
            
            Queue<GameObject> pool = m_MapPools[mapPrefab];
            
            if (pool.Count > 0)
            {
                return pool.Dequeue();
            }
            else if (m_ExpandPool)
            {
                // Create new instance if pool is empty and expansion is allowed
                GameObject newInstance = Instantiate(mapPrefab, m_MapContainer);
                if (m_EnableDebugLogs)
                {
                    Debug.Log($"Expanded pool for map: {mapPrefab.name}");
                }
                return newInstance;
            }
            
            Debug.LogWarning($"Pool exhausted for map: {mapPrefab.name}");
            return null;
        }
        
        private void ReturnMapToPool(GameObject mapPrefab, GameObject mapInstance)
        {
            if (!m_MapPools.ContainsKey(mapPrefab))
            {
                Debug.LogError($"No pool found for map: {mapPrefab.name}");
                Destroy(mapInstance);
                return;
            }
            
            mapInstance.SetActive(false);
            m_MapPools[mapPrefab].Enqueue(mapInstance);
        }
        
        private void ClearAllMaps()
        {
            DeactivateAllMaps();
        }
        
        private void ClearAllPools()
        {
            foreach (var kvp in m_MapPools)
            {
                while (kvp.Value.Count > 0)
                {
                    GameObject instance = kvp.Value.Dequeue();
                    if (instance != null)
                    {
                        Destroy(instance);
                    }
                }
            }
            
            m_MapPools.Clear();
            m_ActiveMaps.Clear();
        }
        
        // Public API methods
        public void ForceUpdateMap()
        {
            UpdateMapForCurrentLevel();
        }
        
        public void RefreshMapForCurrentLevel()
        {
            // Map will be updated when scene loads, not immediately
            if (m_EnableDebugLogs)
            {
                Debug.Log("Map will be updated when scene loads");
            }
        }
        
        public bool IsMapActive(GameObject mapPrefab)
        {
            return m_ActiveMaps.ContainsKey(mapPrefab) && m_ActiveMaps[mapPrefab].Count > 0;
        }
        
        public int GetActiveMapCount()
        {
            int count = 0;
            foreach (var kvp in m_ActiveMaps)
            {
                count += kvp.Value.Count;
            }
            return count;
        }
        
        public void SetMapData(MapData mapData)
        {
            if (m_MapData != mapData)
            {
                ClearAllMaps();
                ClearAllPools();
                
                m_MapData = mapData;
                InitializeMapPools();
                UpdateMapForCurrentLevel();
            }
        }
        
        // Debug methods
        [ContextMenu("Debug Map Status")]
        public void DebugMapStatus()
        {
            Debug.Log($"Current Level: {CurrentLevel}");
            Debug.Log($"Current Map: {(m_CurrentMapInfo != null ? m_CurrentMapInfo.mapName : "None")}");
            Debug.Log($"Active Maps: {GetActiveMapCount()}");
            Debug.Log($"Total Pools: {m_MapPools.Count}");
        }
        
        [ContextMenu("Test Level Up")]
        public void TestLevelUp()
        {
            OnLevelUp();
        }
    }
}
