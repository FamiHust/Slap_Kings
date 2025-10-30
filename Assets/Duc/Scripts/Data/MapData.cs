using UnityEngine;
using System.Collections.Generic;

namespace Duc
{
    [CreateAssetMenu(fileName = "MapData", menuName = "Game/Map Data")]
    public class MapData : ScriptableObject
    {
        [System.Serializable]
        public class MapInfo
        {
            [Header("Map Configuration")]
            public string mapName;
            public GameObject mapPrefab;
            
            [Header("Level Range")]
            [Tooltip("Level bắt đầu cho map này")]
            public int startLevel = 1;
            [Tooltip("Level kết thúc cho map này")]
            public int endLevel = 3;
            
            [Header("Map Settings")]
            [Tooltip("Có thể spawn nhiều instance cùng lúc không")]
            public bool allowMultipleInstances = false;
            [Tooltip("Delay trước khi spawn map (giây)")]
            public float spawnDelay = 0f;
            [Tooltip("Delay trước khi destroy map (giây)")]
            public float destroyDelay = 0f;
            
            public bool IsLevelInRange(int level)
            {
                return level >= startLevel && level <= endLevel;
            }
            
            public int GetLevelRange()
            {
                return endLevel - startLevel + 1;
            }
        }
        
        [Header("Map Configuration")]
        [SerializeField] private List<MapInfo> m_Maps = new List<MapInfo>();
        
        [Header("Default Settings")]
        [SerializeField] private GameObject m_DefaultMapPrefab;
        [SerializeField] private int m_MaxActiveMaps = 1;
        
        public List<MapInfo> Maps => m_Maps;
        public GameObject DefaultMapPrefab => m_DefaultMapPrefab;
        public int MaxActiveMaps => m_MaxActiveMaps;

        public MapInfo GetMapForLevel(int level)
        {
            foreach (var map in m_Maps)
            {
                if (map.IsLevelInRange(level))
                {
                    return map;
                }
            }
            
            return null;
        }

        public List<MapInfo> GetAllMapsForLevel(int level)
        {
            List<MapInfo> result = new List<MapInfo>();
            
            foreach (var map in m_Maps)
            {
                if (map.IsLevelInRange(level))
                {
                    result.Add(map);
                }
            }
            
            return result;
        }

        public bool HasMapForLevel(int level)
        {
            return GetMapForLevel(level) != null;
        }

        public int GetMapCount()
        {
            return m_Maps.Count;
        }

        [ContextMenu("Validate Map Data")]
        public void ValidateMapData()
        {
            for (int i = 0; i < m_Maps.Count; i++)
            {
                var map = m_Maps[i];
                
                if (string.IsNullOrEmpty(map.mapName) || 
                    map.mapPrefab == null || 
                    map.startLevel > map.endLevel)
                {
                    
                }
            }
        }
    }
}
