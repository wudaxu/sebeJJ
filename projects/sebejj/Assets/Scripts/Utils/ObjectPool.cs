using UnityEngine;
using System.Collections.Generic;

namespace SebeJJ.Utils
{
    /// <summary>
    /// 通用对象池系统 - 性能优化核心
    /// 支持任意Component类型的对象池
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private readonly Queue<T> pool;
        private readonly T prefab;
        private readonly Transform container;
        private readonly int maxSize;
        private int currentSize;
        
        public int Count => pool.Count;
        public int CurrentSize => currentSize;
        public int MaxSize => maxSize;
        
        public ObjectPool(T prefab, int initialSize, int maxSize, Transform container = null)
        {
            this.prefab = prefab;
            this.maxSize = maxSize;
            this.container = container;
            this.pool = new Queue<T>(initialSize);
            
            // 预创建对象
            for (int i = 0; i < initialSize; i++)
            {
                T obj = CreateNew();
                obj.gameObject.SetActive(false);
                pool.Enqueue(obj);
            }
            
            currentSize = initialSize;
        }
        
        /// <summary>
        /// 从池中获取对象
        /// </summary>
        public T Get()
        {
            if (pool.Count > 0)
            {
                T obj = pool.Dequeue();
                obj.gameObject.SetActive(true);
                return obj;
            }
            
            // 池为空，创建新对象（如果未超过最大限制）
            if (currentSize < maxSize)
            {
                currentSize++;
                return CreateNew();
            }
            
            // 超过最大限制，返回null或创建临时对象
            Debug.LogWarning($"[ObjectPool] Pool for {typeof(T).Name} has reached max size ({maxSize})");
            return null;
        }
        
        /// <summary>
        /// 将对象返回池中
        /// </summary>
        public void Return(T obj)
        {
            if (obj == null) return;
            
            if (pool.Count >= maxSize)
            {
                // 池已满，销毁对象
                Object.Destroy(obj.gameObject);
                currentSize--;
                return;
            }
            
            obj.gameObject.SetActive(false);
            
            if (container != null)
            {
                obj.transform.SetParent(container);
            }
            
            pool.Enqueue(obj);
        }
        
        /// <summary>
        /// 预加载更多对象到池中
        /// </summary>
        public void Prewarm(int count)
        {
            int toCreate = Mathf.Min(count, maxSize - currentSize);
            
            for (int i = 0; i < toCreate; i++)
            {
                T obj = CreateNew();
                obj.gameObject.SetActive(false);
                pool.Enqueue(obj);
                currentSize++;
            }
        }
        
        /// <summary>
        /// 清空对象池
        /// </summary>
        public void Clear()
        {
            while (pool.Count > 0)
            {
                T obj = pool.Dequeue();
                if (obj != null)
                {
                    Object.Destroy(obj.gameObject);
                }
            }
            currentSize = 0;
        }
        
        private T CreateNew()
        {
            GameObject go;
            if (container != null)
            {
                go = Object.Instantiate(prefab.gameObject, container);
            }
            else
            {
                go = Object.Instantiate(prefab.gameObject);
            }
            
            return go.GetComponent<T>();
        }
    }
    
    /// <summary>
    /// GameObject专用对象池（不需要Component）
    /// </summary>
    public class GameObjectPool
    {
        private readonly Queue<GameObject> pool;
        private readonly GameObject prefab;
        private readonly Transform container;
        private readonly int maxSize;
        private int currentSize;
        
        public int Count => pool.Count;
        public int CurrentSize => currentSize;
        
        public GameObjectPool(GameObject prefab, int initialSize, int maxSize, Transform container = null)
        {
            this.prefab = prefab;
            this.maxSize = maxSize;
            this.container = container;
            this.pool = new Queue<GameObject>(initialSize);
            
            for (int i = 0; i < initialSize; i++)
            {
                GameObject obj = CreateNew();
                obj.SetActive(false);
                pool.Enqueue(obj);
            }
            
            currentSize = initialSize;
        }
        
        public GameObject Get()
        {
            if (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                obj.SetActive(true);
                return obj;
            }
            
            if (currentSize < maxSize)
            {
                currentSize++;
                return CreateNew();
            }
            
            Debug.LogWarning($"[GameObjectPool] Pool for {prefab.name} has reached max size ({maxSize})");
            return null;
        }
        
        public void Return(GameObject obj)
        {
            if (obj == null) return;
            
            if (pool.Count >= maxSize)
            {
                Object.Destroy(obj);
                currentSize--;
                return;
            }
            
            obj.SetActive(false);
            
            if (container != null)
            {
                obj.transform.SetParent(container);
            }
            
            pool.Enqueue(obj);
        }
        
        public void Prewarm(int count)
        {
            int toCreate = Mathf.Min(count, maxSize - currentSize);
            
            for (int i = 0; i < toCreate; i++)
            {
                GameObject obj = CreateNew();
                obj.SetActive(false);
                pool.Enqueue(obj);
                currentSize++;
            }
        }
        
        public void Clear()
        {
            while (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                if (obj != null)
                {
                    Object.Destroy(obj);
                }
            }
            currentSize = 0;
        }
        
        private GameObject CreateNew()
        {
            if (container != null)
            {
                return Object.Instantiate(prefab, container);
            }
            return Object.Instantiate(prefab);
        }
    }
    
    /// <summary>
    /// 对象池管理器 - 统一管理所有对象池
    /// </summary>
    public class ObjectPoolManager : MonoBehaviour
    {
        public static ObjectPoolManager Instance { get; private set; }
        
        [System.Serializable]
        public class PoolConfig
        {
            public string poolName;
            public GameObject prefab;
            public int initialSize = 10;
            public int maxSize = 50;
        }
        
        [SerializeField] private PoolConfig[] poolConfigs;
        [SerializeField] private Transform poolContainer;
        
        private Dictionary<string, GameObjectPool> pools;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            InitializePools();
        }
        
        private void InitializePools()
        {
            pools = new Dictionary<string, GameObjectPool>();
            
            if (poolContainer == null)
            {
                poolContainer = new GameObject("ObjectPools").transform;
                poolContainer.SetParent(transform);
            }
            
            foreach (var config in poolConfigs)
            {
                if (config.prefab == null) continue;
                
                Transform container = new GameObject($"Pool_{config.poolName}").transform;
                container.SetParent(poolContainer);
                
                var pool = new GameObjectPool(
                    config.prefab,
                    config.initialSize,
                    config.maxSize,
                    container
                );
                
                pools[config.poolName] = pool;
            }
        }
        
        public GameObject Get(string poolName)
        {
            if (pools.TryGetValue(poolName, out var pool))
            {
                return pool.Get();
            }
            Debug.LogWarning($"[ObjectPoolManager] Pool '{poolName}' not found");
            return null;
        }
        
        public void Return(string poolName, GameObject obj)
        {
            if (pools.TryGetValue(poolName, out var pool))
            {
                pool.Return(obj);
            }
            else
            {
                Destroy(obj);
            }
        }
        
        public void Prewarm(string poolName, int count)
        {
            if (pools.TryGetValue(poolName, out var pool))
            {
                pool.Prewarm(count);
            }
        }
        
        public void ClearPool(string poolName)
        {
            if (pools.TryGetValue(poolName, out var pool))
            {
                pool.Clear();
            }
        }
        
        public void ClearAllPools()
        {
            foreach (var pool in pools.Values)
            {
                pool.Clear();
            }
        }
    }
}
