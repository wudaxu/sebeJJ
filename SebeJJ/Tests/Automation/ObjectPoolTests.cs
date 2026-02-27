using NUnit.Framework;
using UnityEngine;

namespace SebeJJ.Tests.Core
{
    /// <summary>
    /// ObjectPool 单元测试
    /// </summary>
    public class ObjectPoolTests
    {
        private GameObject _poolContainer;
        private GameObject _testPrefab;

        [SetUp]
        public void Setup()
        {
            _poolContainer = new GameObject("PoolContainer");
            _testPrefab = new GameObject("TestPrefab");
            _testPrefab.AddComponent<TestPoolable>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_poolContainer);
            Object.DestroyImmediate(_testPrefab);
        }

        [Test]
        public void ObjectPool_Get_ReturnsObject()
        {
            // Arrange
            var pool = new ObjectPool<TestPoolable>(_testPrefab.GetComponent<TestPoolable>(), 5, _poolContainer.transform);

            // Act
            var obj = pool.Get();

            // Assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj.gameObject.activeSelf);
        }

        [Test]
        public void ObjectPool_Get_ExpandsWhenEmpty()
        {
            // Arrange
            var pool = new ObjectPool<TestPoolable>(_testPrefab.GetComponent<TestPoolable>(), 1, _poolContainer.transform);
            
            // Act - 获取比初始容量多的对象
            var obj1 = pool.Get();
            var obj2 = pool.Get();
            var obj3 = pool.Get();

            // Assert
            Assert.IsNotNull(obj1);
            Assert.IsNotNull(obj2);
            Assert.IsNotNull(obj3);
            Assert.AreNotSame(obj1, obj2);
            Assert.AreNotSame(obj2, obj3);
        }

        [Test]
        public void ObjectPool_Return_ObjectGoesBackToPool()
        {
            // Arrange
            var pool = new ObjectPool<TestPoolable>(_testPrefab.GetComponent<TestPoolable>(), 5, _poolContainer.transform);
            var obj = pool.Get();

            // Act
            pool.Return(obj);

            // Assert
            Assert.IsFalse(obj.gameObject.activeSelf);
            Assert.AreEqual(1, pool.CountInactive);
        }

        [Test]
        public void ObjectPool_ReturnedObject_CanBeReused()
        {
            // Arrange
            var pool = new ObjectPool<TestPoolable>(_testPrefab.GetComponent<TestPoolable>(), 5, _poolContainer.transform);
            var obj1 = pool.Get();
            pool.Return(obj1);

            // Act
            var obj2 = pool.Get();

            // Assert
            Assert.AreSame(obj1, obj2);
        }

        [Test]
        public void ObjectPool_Prewarm_CreatesCorrectCount()
        {
            // Arrange
            var pool = new ObjectPool<TestPoolable>(_testPrefab.GetComponent<TestPoolable>(), 0, _poolContainer.transform);

            // Act
            pool.Prewarm(10);

            // Assert
            Assert.AreEqual(10, pool.CountInactive);
        }

        [Test]
        public void ObjectPool_Clear_DisablesAllActive()
        {
            // Arrange
            var pool = new ObjectPool<TestPoolable>(_testPrefab.GetComponent<TestPoolable>(), 5, _poolContainer.transform);
            var obj1 = pool.Get();
            var obj2 = pool.Get();

            // Act
            pool.Clear();

            // Assert
            Assert.IsFalse(obj1.gameObject.activeSelf);
            Assert.IsFalse(obj2.gameObject.activeSelf);
        }

        [Test]
        public void ObjectPool_Get_ResetsObjectState()
        {
            // Arrange
            var pool = new ObjectPool<TestPoolable>(_testPrefab.GetComponent<TestPoolable>(), 5, _poolContainer.transform);
            var obj = pool.Get();
            obj.CustomData = "Modified";
            pool.Return(obj);

            // Act
            var reusedObj = pool.Get();

            // Assert
            Assert.AreEqual("Reset", reusedObj.CustomData);
        }
    }

    /// <summary>
    /// 可池化对象接口
    /// </summary>
    public interface IPoolable
    {
        void OnGetFromPool();
        void OnReturnToPool();
    }

    /// <summary>
    /// 测试用的池化组件
    /// </summary>
    public class TestPoolable : MonoBehaviour, IPoolable
    {
        public string CustomData { get; set; } = "Default";

        public void OnGetFromPool()
        {
            CustomData = "Reset";
        }

        public void OnReturnToPool()
        {
            CustomData = "Returned";
        }
    }

    /// <summary>
    /// 泛型对象池
    /// </summary>
    public class ObjectPool<T> where T : MonoBehaviour, IPoolable
    {
        private T _prefab;
        private Transform _container;
        private System.Collections.Generic.Queue<T> _inactiveObjects;
        private System.Collections.Generic.HashSet<T> _activeObjects;

        public int CountInactive => _inactiveObjects.Count;
        public int CountActive => _activeObjects.Count;
        public int CountTotal => CountInactive + CountActive;

        public ObjectPool(T prefab, int initialSize, Transform container)
        {
            _prefab = prefab;
            _container = container;
            _inactiveObjects = new System.Collections.Generic.Queue<T>();
            _activeObjects = new System.Collections.Generic.HashSet<T>();

            Prewarm(initialSize);
        }

        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var obj = CreateNewObject();
                obj.gameObject.SetActive(false);
                _inactiveObjects.Enqueue(obj);
            }
        }

        public T Get()
        {
            T obj;

            if (_inactiveObjects.Count > 0)
            {
                obj = _inactiveObjects.Dequeue();
            }
            else
            {
                obj = CreateNewObject();
            }

            obj.gameObject.SetActive(true);
            obj.OnGetFromPool();
            _activeObjects.Add(obj);

            return obj;
        }

        public void Return(T obj)
        {
            if (!_activeObjects.Contains(obj))
            {
                Debug.LogWarning($"Trying to return object that is not from this pool: {obj.name}");
                return;
            }

            _activeObjects.Remove(obj);
            obj.OnReturnToPool();
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(_container);
            _inactiveObjects.Enqueue(obj);
        }

        public void Clear()
        {
            foreach (var obj in _activeObjects)
            {
                obj.gameObject.SetActive(false);
                obj.OnReturnToPool();
                _inactiveObjects.Enqueue(obj);
            }
            _activeObjects.Clear();
        }

        private T CreateNewObject()
        {
            var obj = Object.Instantiate(_prefab, _container);
            obj.name = $"{_prefab.name}_{CountTotal}";
            return obj;
        }
    }
}
