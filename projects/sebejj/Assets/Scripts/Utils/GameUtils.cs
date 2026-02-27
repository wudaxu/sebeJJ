using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SebeJJ.Utils
{
    /// <summary>
    /// 单例模式基类
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;
        
        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit.");
                    return null;
                }
                
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<T>();
                        
                        if (_instance == null)
                        {
                            var singletonObject = new GameObject();
                            _instance = singletonObject.AddComponent<T>();
                            singletonObject.name = $"{typeof(T)} (Singleton)";
                            DontDestroyOnLoad(singletonObject);
                        }
                    }
                    
                    return _instance;
                }
            }
        }
        
        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        
        protected virtual void OnDestroy()
        {
            _applicationIsQuitting = true;
        }
    }
    
    /// <summary>
    /// 对象池 (CO-002 优化: 动态扩容 + 预热功能)
    /// </summary>
    public class ObjectPool<T> where T : MonoBehaviour
    {
        private T prefab;
        private Transform parent;
        private Queue<T> pool = new Queue<T>();
        private List<T> activeObjects = new List<T>();
        
        // CO-002: 动态扩容配置
        private int initialSize;
        private int maxSize = 100;
        private float expansionFactor = 1.5f;
        
        public int ActiveCount => activeObjects.Count;
        public int PooledCount => pool.Count;
        public int TotalCount => activeObjects.Count + pool.Count;
        
        public ObjectPool(T prefab, int initialSize, Transform parent = null, int maxSize = 100)
        {
            this.prefab = prefab;
            this.parent = parent;
            this.initialSize = initialSize;
            this.maxSize = maxSize;
            
            // 预热对象池
            Prewarm(initialSize);
        }
        
        /// <summary>
        /// 预热对象池 (CO-002)
        /// </summary>
        public void Prewarm(int count)
        {
            for (int i = 0; i < count && TotalCount < maxSize; i++)
            {
                CreateNewObject();
            }
            Debug.Log($"[ObjectPool] 预热完成: {typeof(T).Name} x{count}");
        }
        
        private T CreateNewObject()
        {
            T obj = UnityEngine.Object.Instantiate(prefab, parent);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
            return obj;
        }
        
        /// <summary>
        /// 从池中获取对象
        /// </summary>
        public T Get()
        {
            T obj;
            
            if (pool.Count > 0)
            {
                obj = pool.Dequeue();
            }
            else if (TotalCount < maxSize)
            {
                // CO-002: 动态扩容
                int expandCount = Mathf.Max(1, (int)(initialSize * (expansionFactor - 1)));
                Prewarm(expandCount);
                obj = pool.Dequeue();
                
                Debug.Log($"[ObjectPool] 动态扩容: {typeof(T).Name} +{expandCount}");
            }
            else
            {
                // 达到最大容量，强制回收最旧的对象
                Debug.LogWarning($"[ObjectPool] 达到最大容量，强制回收: {typeof(T).Name}");
                if (activeObjects.Count > 0)
                {
                    Return(activeObjects[0]);
                    obj = pool.Dequeue();
                }
                else
                {
                    obj = CreateNewObject();
                }
            }
            
            obj.gameObject.SetActive(true);
            activeObjects.Add(obj);
            return obj;
        }
        
        /// <summary>
        /// 归还对象到池中
        /// </summary>
        public void Return(T obj)
        {
            if (obj == null) return;
            
            if (activeObjects.Contains(obj))
            {
                activeObjects.Remove(obj);
                obj.gameObject.SetActive(false);
                
                // 重置对象状态
                ResetObject(obj);
                
                pool.Enqueue(obj);
            }
        }
        
        /// <summary>
        /// 重置对象状态 (可被子类重写)
        /// </summary>
        protected virtual void ResetObject(T obj)
        {
            // 默认实现：重置位置和旋转
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
        }
        
        /// <summary>
        /// 归还所有活跃对象
        /// </summary>
        public void ReturnAll()
        {
            // 创建副本列表避免修改迭代
            var objectsToReturn = new List<T>(activeObjects);
            foreach (var obj in objectsToReturn)
            {
                Return(obj);
            }
        }
        
        /// <summary>
        /// 清空对象池
        /// </summary>
        public void Clear()
        {
            ReturnAll();
            while (pool.Count > 0)
            {
                var obj = pool.Dequeue();
                if (obj != null)
                {
                    UnityEngine.Object.Destroy(obj.gameObject);
                }
            }
        }
        
        /// <summary>
        /// 获取命中率统计 (CO-001)
        /// </summary>
        public float GetHitRate(int totalRequests)
        {
            if (totalRequests == 0) return 1f;
            int hits = totalRequests - (TotalCount - initialSize);
            return Mathf.Clamp01((float)hits / totalRequests);
        }
    }
    
    /// <summary>
    /// 计时器工具
    /// </summary>
    public class Timer
    {
        private float duration;
        private float elapsed;
        private bool isRunning;
        private bool isLooping;
        private Action onComplete;
        
        public bool IsRunning => isRunning;
        public float Progress => duration > 0 ? elapsed / duration : 0f;
        public float Remaining => Mathf.Max(0f, duration - elapsed);
        
        public Timer(float duration, Action onComplete = null, bool loop = false)
        {
            this.duration = duration;
            this.onComplete = onComplete;
            this.isLooping = loop;
        }
        
        public void Start()
        {
            isRunning = true;
            elapsed = 0f;
        }
        
        public void Stop()
        {
            isRunning = false;
        }
        
        public void Reset()
        {
            elapsed = 0f;
        }
        
        public void Update(float deltaTime)
        {
            if (!isRunning) return;
            
            elapsed += deltaTime;
            
            if (elapsed >= duration)
            {
                onComplete?.Invoke();
                
                if (isLooping)
                {
                    elapsed = 0f;
                }
                else
                {
                    isRunning = false;
                }
            }
        }
    }
    
    /// <summary>
    /// 数学工具
    /// </summary>
    public static class MathUtils
    {
        /// <summary>
        /// 将值映射到新的范围
        /// </summary>
        public static float Map(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
        }
        
        /// <summary>
        /// 平滑阻尼
        /// </summary>
        public static float SmoothDamp(float current, float target, ref float velocity, float smoothTime, float maxSpeed = Mathf.Infinity)
        {
            return Mathf.SmoothDamp(current, target, ref velocity, smoothTime, maxSpeed);
        }
        
        /// <summary>
        /// 获取随机方向
        /// </summary>
        public static Vector2 RandomDirection()
        {
            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
        
        /// <summary>
        /// 检查点是否在扇形区域内
        /// </summary>
        public static bool IsInSector(Vector2 point, Vector2 sectorCenter, Vector2 sectorDirection, float radius, float angle)
        {
            Vector2 toPoint = point - sectorCenter;
            float distance = toPoint.magnitude;
            
            if (distance > radius) return false;
            
            float pointAngle = Vector2.Angle(sectorDirection, toPoint);
            return pointAngle <= angle / 2f;
        }
    }
    
    /// <summary>
    /// 协程工具
    /// </summary>
    public static class CoroutineUtils
    {
        /// <summary>
        /// 延迟执行
        /// </summary>
        public static IEnumerator DelayAction(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }
        
        /// <summary>
        /// 条件等待
        /// </summary>
        public static IEnumerator WaitUntil(Func<bool> condition, Action onComplete, float timeout = -1f)
        {
            float elapsed = 0f;
            
            while (!condition())
            {
                yield return null;
                elapsed += Time.deltaTime;
                
                if (timeout > 0 && elapsed >= timeout)
                    yield break;
            }
            
            onComplete?.Invoke();
        }
    }
}
