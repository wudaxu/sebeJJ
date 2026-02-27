using System.Collections.Generic;
using UnityEngine;

namespace SebeJJ.Utils
{
    /// <summary>
    /// 对象池基类
    /// </summary>
    public class ObjectPool<T> where T : MonoBehaviour
    {
        private T _prefab;
        private Transform _parent;
        private Queue<T> _pool;
        private List<T> _activeObjects;
        private int _initialSize;
        private int _maxSize;

        public int ActiveCount => _activeObjects?.Count ?? 0;
        public int PooledCount => _pool?.Count ?? 0;
        public int TotalCount => ActiveCount + PooledCount;

        public ObjectPool(T prefab, int initialSize = 10, int maxSize = 100, Transform parent = null)
        {
            _prefab = prefab;
            _initialSize = initialSize;
            _maxSize = maxSize;
            _parent = parent;
            _pool = new Queue<T>();
            _activeObjects = new List<T>();

            Initialize();
        }

        private void Initialize()
        {
            for (int i = 0; i < _initialSize; i++)
            {
                CreateNewObject();
            }
        }

        private T CreateNewObject()
        {
            if (TotalCount >= _maxSize) return null;

            T obj = Object.Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
            return obj;
        }

        /// <summary>
        /// 获取对象
        /// </summary>
        public T Get()
        {
            T obj = null;

            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
            }
            else if (TotalCount < _maxSize)
            {
                obj = CreateNewObject();
            }

            if (obj != null)
            {
                obj.gameObject.SetActive(true);
                _activeObjects.Add(obj);
            }

            return obj;
        }

        /// <summary>
        /// 获取对象并设置位置
        /// </summary>
        public T Get(Vector3 position, Quaternion rotation)
        {
            T obj = Get();
            if (obj != null)
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
            }
            return obj;
        }

        /// <summary>
        /// 归还对象
        /// </summary>
        public void Return(T obj)
        {
            if (obj == null) return;

            if (_activeObjects.Remove(obj))
            {
                obj.gameObject.SetActive(false);
                _pool.Enqueue(obj);
            }
        }

        /// <summary>
        /// 归还所有活跃对象
        /// </summary>
        public void ReturnAll()
        {
            var activeCopy = new List<T>(_activeObjects);
            foreach (var obj in activeCopy)
            {
                Return(obj);
            }
        }

        /// <summary>
        /// 清空池
        /// </summary>
        public void Clear()
        {
            ReturnAll();
            
            while (_pool.Count > 0)
            {
                var obj = _pool.Dequeue();
                if (obj != null)
                {
                    Object.Destroy(obj.gameObject);
                }
            }
        }
    }

    /// <summary>
    /// MonoBehaviour对象池组件
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int initialSize = 10;
        [SerializeField] private int maxSize = 100;

        private Queue<GameObject> _pool;
        private List<GameObject> _activeObjects;

        private void Awake()
        {
            _pool = new Queue<GameObject>();
            _activeObjects = new List<GameObject>();
            Initialize();
        }

        private void Initialize()
        {
            for (int i = 0; i < initialSize; i++)
            {
                CreateNewObject();
            }
        }

        private GameObject CreateNewObject()
        {
            if (prefab == null) return null;

            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            _pool.Enqueue(obj);
            return obj;
        }

        public GameObject Get()
        {
            GameObject obj = null;

            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
            }
            else if (_activeObjects.Count + _pool.Count < maxSize)
            {
                obj = CreateNewObject();
            }

            if (obj != null)
            {
                obj.SetActive(true);
                _activeObjects.Add(obj);
            }

            return obj;
        }

        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            GameObject obj = Get();
            if (obj != null)
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
            }
            return obj;
        }

        public void Return(GameObject obj)
        {
            if (obj == null) return;

            if (_activeObjects.Remove(obj))
            {
                obj.SetActive(false);
                _pool.Enqueue(obj);
            }
        }

        public void ReturnAll()
        {
            var activeCopy = new List<GameObject>(_activeObjects);
            foreach (var obj in activeCopy)
            {
                Return(obj);
            }
        }
    }
}