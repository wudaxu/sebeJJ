using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;

namespace SebeJJ.Tests
{
    /// <summary>
    /// 投射物系统测试 - Week 2 战斗系统
    /// </summary>
    public class ProjectileSystemTests
    {
        private GameObject _projectilePrefab;
        private ObjectPool _projectilePool;

        [SetUp]
        public void Setup()
        {
            _projectilePrefab = TestUtils.CreateTestObject("TestProjectile");
            _projectilePrefab.AddComponent<Projectile>();
            _projectilePrefab.AddComponent<Rigidbody2D>();
            _projectilePrefab.AddComponent<CircleCollider2D>();
            
            _projectilePool = new ObjectPool(_projectilePrefab, 10);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_projectilePrefab);
            _projectilePool = null;
        }

        #region 基础飞行测试

        [UnityTest]
        [Category("Combat")]
        [Category("Projectile")]
        public IEnumerator Projectile_Fly_StraightLine()
        {
            // Arrange
            var projectile = _projectilePool.Get().GetComponent<Projectile>();
            Vector2 startPos = Vector2.zero;
            Vector2 direction = Vector2.right;
            float speed = 10f;
            
            projectile.transform.position = startPos;
            projectile.Initialize(direction, speed, 10f, null);
            
            // Act
            yield return new WaitForSeconds(0.1f);
            
            // Assert
            Assert.Greater(projectile.transform.position.x, startPos.x);
            Assert.AreEqual(startPos.y, projectile.transform.position.y, 0.1f);
            
            _projectilePool.Return(projectile.gameObject);
        }

        [UnityTest]
        [Category("Combat")]
        [Category("Projectile")]
        public IEnumerator Projectile_Lifetime_ExpiresAfterMaxTime()
        {
            // Arrange
            var projectile = _projectilePool.Get().GetComponent<Projectile>();
            float maxLifetime = 0.5f;
            
            projectile.Initialize(Vector2.right, 10f, maxLifetime, null);
            
            // Act
            yield return new WaitForSeconds(maxLifetime + 0.1f);
            
            // Assert
            Assert.IsFalse(projectile.gameObject.activeInHierarchy);
        }

        [Test]
        [Category("Combat")]
        [Category("Projectile")]
        public void Projectile_Initialize_SetsProperties()
        {
            // Arrange
            var projectile = _projectilePrefab.GetComponent<Projectile>();
            Vector2 direction = Vector2.up;
            float speed = 15f;
            float damage = 25f;
            
            // Act
            projectile.Initialize(direction, speed, 5f, null);
            projectile.SetDamage(damage);
            
            // Assert
            Assert.AreEqual(direction, projectile.Direction);
            Assert.AreEqual(speed, projectile.Speed);
            Assert.AreEqual(damage, projectile.Damage);
        }

        #endregion

        #region 碰撞检测

        [UnityTest]
        [Category("Combat")]
        [Category("Projectile")]
        public IEnumerator Projectile_Collision_TriggersHitEvent()
        {
            // Arrange
            var projectile = _projectilePool.Get().GetComponent<Projectile>();
            bool hitTriggered = false;
            projectile.OnHit += (target) => hitTriggered = true;
            
            // Create target
            var target = TestUtils.CreateTestObject("Target");
            target.AddComponent<BoxCollider2D>();
            target.transform.position = new Vector2(2f, 0f);
            
            projectile.Initialize(Vector2.right, 20f, 5f, null);
            projectile.transform.position = Vector2.zero;
            
            // Act
            yield return new WaitForSeconds(0.2f);
            
            // Assert
            Assert.IsTrue(hitTriggered);
            
            // Cleanup
            Object.DestroyImmediate(target);
            _projectilePool.Return(projectile.gameObject);
        }

        [UnityTest]
        [Category("Combat")]
        [Category("Projectile")]
        public IEnumerator Projectile_Collision_AppliesDamage()
        {
            // Arrange
            var projectile = _projectilePool.Get().GetComponent<Projectile>();
            var target = TestUtils.CreateTestObject("Target");
            var damageable = target.AddComponent<TestDamageableComponent>();
            target.AddComponent<BoxCollider2D>();
            
            float damage = 50f;
            projectile.SetDamage(damage);
            projectile.Initialize(Vector2.right, 20f, 5f, null);
            projectile.transform.position = Vector2.zero;
            target.transform.position = new Vector2(2f, 0f);
            
            // Act
            yield return new WaitForSeconds(0.2f);
            
            // Assert
            Assert.AreEqual(damage, damageable.LastDamageTaken);
            
            // Cleanup
            Object.DestroyImmediate(target);
            _projectilePool.Return(projectile.gameObject);
        }

        #endregion

        #region 穿透效果

        [UnityTest]
        [Category("Combat")]
        [Category("Projectile")]
        public IEnumerator Projectile_Pierce_PassesThroughTargets()
        {
            // Arrange
            var projectile = _projectilePool.Get().GetComponent<Projectile>();
            projectile.SetPierceCount(3);
            
            int hitCount = 0;
            projectile.OnHit += (target) => hitCount++;
            
            // Create multiple targets
            var targets = new List<GameObject>();
            for (int i = 0; i < 5; i++)
            {
                var target = TestUtils.CreateTestObject($"Target{i}");
                target.AddComponent<BoxCollider2D>();
                target.transform.position = new Vector2(1f + i * 0.5f, 0f);
                targets.Add(target);
            }
            
            projectile.Initialize(Vector2.right, 10f, 5f, null);
            projectile.transform.position = Vector2.zero;
            
            // Act
            yield return new WaitForSeconds(0.5f);
            
            // Assert
            Assert.AreEqual(3, hitCount); // Should pierce 3 targets
            
            // Cleanup
            foreach (var t in targets)
                Object.DestroyImmediate(t);
            _projectilePool.Return(projectile.gameObject);
        }

        #endregion

        #region 反弹效果

        [UnityTest]
        [Category("Combat")]
        [Category("Projectile")]
        public IEnumerator Projectile_Bounce_ChangesDirection()
        {
            // Arrange
            var projectile = _projectilePool.Get().GetComponent<Projectile>();
            projectile.SetBounceCount(2);
            
            // Create wall
            var wall = TestUtils.CreateTestObject("Wall");
            wall.AddComponent<BoxCollider2D>();
            wall.transform.position = new Vector2(3f, 0f);
            wall.transform.localScale = new Vector3(0.1f, 5f, 1f);
            
            Vector2 initialDirection = Vector2.right;
            projectile.Initialize(initialDirection, 10f, 5f, null);
            projectile.transform.position = Vector2.zero;
            
            // Act
            yield return new WaitForSeconds(0.5f);
            
            // Assert
            Assert.AreNotEqual(initialDirection, projectile.Direction);
            
            // Cleanup
            Object.DestroyImmediate(wall);
            _projectilePool.Return(projectile.gameObject);
        }

        #endregion

        #region 追踪效果

        [UnityTest]
        [Category("Combat")]
        [Category("Projectile")]
        public IEnumerator Projectile_Homing_FollowsTarget()
        {
            // Arrange
            var projectile = _projectilePool.Get().GetComponent<Projectile>();
            projectile.EnableHoming(5f);
            
            var target = TestUtils.CreateTestObject("Target");
            target.transform.position = new Vector2(5f, 2f);
            
            projectile.SetTarget(target.transform);
            projectile.Initialize(Vector2.right, 8f, 5f, null);
            projectile.transform.position = Vector2.zero;
            
            float initialAngle = Vector2.Angle(Vector2.right, 
                (target.transform.position - projectile.transform.position).normalized);
            
            // Act
            yield return new WaitForSeconds(0.3f);
            
            // Assert - projectile should have turned towards target
            float newAngle = Vector2.Angle(Vector2.right,
                (target.transform.position - projectile.transform.position).normalized);
            Assert.Less(newAngle, initialAngle);
            
            // Cleanup
            Object.DestroyImmediate(target);
            _projectilePool.Return(projectile.gameObject);
        }

        #endregion

        #region 范围爆炸

        [Test]
        [Category("Combat")]
        [Category("Projectile")]
        public void Projectile_Explosion_DamagesInRadius()
        {
            // Arrange
            var projectile = _projectilePrefab.GetComponent<Projectile>();
            projectile.EnableExplosion(5f, 80f);
            
            // Create targets at different distances
            var targetInRange = TestUtils.CreateTestObject("TargetInRange");
            targetInRange.AddComponent<TestDamageableComponent>();
            targetInRange.transform.position = new Vector2(3f, 0f);
            
            var targetOutOfRange = TestUtils.CreateTestObject("TargetOutOfRange");
            targetOutOfRange.AddComponent<TestDamageableComponent>();
            targetOutOfRange.transform.position = new Vector2(10f, 0f);
            
            // Act
            projectile.Explode();
            
            // Assert
            var inRangeDamageable = targetInRange.GetComponent<TestDamageableComponent>();
            var outOfRangeDamageable = targetOutOfRange.GetComponent<TestDamageableComponent>();
            
            Assert.Greater(inRangeDamageable.LastDamageTaken, 0f);
            Assert.AreEqual(0f, outOfRangeDamageable.LastDamageTaken);
            
            // Cleanup
            Object.DestroyImmediate(targetInRange);
            Object.DestroyImmediate(targetOutOfRange);
        }

        #endregion

        #region 对象池

        [Test]
        [Category("Combat")]
        [Category("Projectile")]
        [Category("ObjectPool")]
        public void Projectile_Pool_ReturnsToPool()
        {
            // Arrange
            var projectile = _projectilePool.Get();
            
            // Act
            _projectilePool.Return(projectile);
            
            // Assert
            Assert.IsFalse(projectile.activeInHierarchy);
        }

        [Test]
        [Category("Combat")]
        [Category("Projectile")]
        [Category("ObjectPool")]
        public void Projectile_Pool_ReusesObject()
        {
            // Arrange
            var projectile1 = _projectilePool.Get();
            int instanceId = projectile1.GetInstanceID();
            _projectilePool.Return(projectile1);
            
            // Act
            var projectile2 = _projectilePool.Get();
            
            // Assert
            Assert.AreEqual(instanceId, projectile2.GetInstanceID());
        }

        #endregion

        #region 性能测试

        [UnityTest]
        [Category("Combat")]
        [Category("Projectile")]
        [Category("Performance")]
        public IEnumerator Projectile_Performance_100Projectiles()
        {
            // Arrange
            var projectiles = new List<GameObject>();
            float startTime = Time.realtimeSinceStartup;
            
            // Act - Spawn 100 projectiles
            for (int i = 0; i < 100; i++)
            {
                var proj = _projectilePool.Get();
                proj.GetComponent<Projectile>().Initialize(
                    Random.insideUnitCircle.normalized, 
                    10f, 5f, null);
                projectiles.Add(proj);
            }
            
            float spawnTime = Time.realtimeSinceStartup - startTime;
            
            // Run for a bit
            yield return new WaitForSeconds(1f);
            
            // Cleanup
            foreach (var proj in projectiles)
                _projectilePool.Return(proj);
            
            // Assert
            Assert.Less(spawnTime, 0.1f, "Spawning 100 projectiles should take less than 100ms");
        }

        #endregion

        #region 测试组件

        private class TestDamageableComponent : MonoBehaviour, IDamageable
        {
            public float MaxHealth { get; set; } = 100f;
            public float CurrentHealth { get; set; } = 100f;
            public bool IsAlive => CurrentHealth > 0;
            public float LastDamageTaken { get; private set; }

            public void TakeDamage(DamageInfo damageInfo)
            {
                LastDamageTaken = damageInfo.Amount;
                CurrentHealth -= damageInfo.Amount;
            }

            public void Heal(float amount)
            {
                CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
            }
        }

        #endregion
    }

    #region Projectile 类定义

    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float _damage = 10f;
        [SerializeField] private float _speed = 10f;
        [SerializeField] private Vector2 _direction = Vector2.right;
        [SerializeField] private float _maxLifetime = 5f;
        [SerializeField] private int _pierceCount = 0;
        [SerializeField] private int _bounceCount = 0;
        [SerializeField] private bool _homingEnabled = false;
        [SerializeField] private float _homingStrength = 5f;
        [SerializeField] private bool _explosive = false;
        [SerializeField] private float _explosionRadius = 5f;
        
        private float _lifetime;
        private int _currentPierceCount;
        private int _currentBounceCount;
        private Transform _target;
        private Rigidbody2D _rb;

        public Vector2 Direction => _direction;
        public float Speed => _speed;
        public float Damage => _damage;

        public event System.Action<GameObject> OnHit;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            _lifetime = 0f;
            _currentPierceCount = _pierceCount;
            _currentBounceCount = _bounceCount;
        }

        private void Update()
        {
            _lifetime += Time.deltaTime;
            
            if (_lifetime >= _maxLifetime)
            {
                ReturnToPool();
                return;
            }

            if (_homingEnabled && _target != null)
            {
                UpdateHoming();
            }

            // Move projectile
            if (_rb != null)
            {
                _rb.velocity = _direction * _speed;
            }
            else
            {
                transform.position += (Vector3)(_direction * _speed * Time.deltaTime);
            }
        }

        public void Initialize(Vector2 direction, float speed, float lifetime, GameObject source)
        {
            _direction = direction.normalized;
            _speed = speed;
            _maxLifetime = lifetime;
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        }

        public void SetDamage(float damage) => _damage = damage;
        public void SetPierceCount(int count) => _pierceCount = count;
        public void SetBounceCount(int count) => _bounceCount = count;
        public void EnableHoming(float strength)
        {
            _homingEnabled = true;
            _homingStrength = strength;
        }
        public void SetTarget(Transform target) => _target = target;
        public void EnableExplosion(float radius, float damage)
        {
            _explosive = true;
            _explosionRadius = radius;
            _damage = damage;
        }

        private void UpdateHoming()
        {
            Vector2 toTarget = (_target.position - transform.position).normalized;
            _direction = Vector2.Lerp(_direction, toTarget, _homingStrength * Time.deltaTime).normalized;
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_explosive)
            {
                Explode();
                return;
            }

            var damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(new DamageInfo 
                { 
                    Amount = _damage, 
                    Type = DamageType.Physical,
                    Source = gameObject 
                });
            }

            OnHit?.Invoke(other.gameObject);

            if (_currentPierceCount > 0)
            {
                _currentPierceCount--;
                return;
            }

            ReturnToPool();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_currentBounceCount > 0)
            {
                _currentBounceCount--;
                Vector2 reflectDir = Vector2.Reflect(_direction, collision.contacts[0].normal);
                _direction = reflectDir.normalized;
                transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg);
            }
            else
            {
                ReturnToPool();
            }
        }

        public void Explode()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _explosionRadius);
            foreach (var hit in hits)
            {
                var damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(new DamageInfo 
                    { 
                        Amount = _damage, 
                        Type = DamageType.Explosive,
                        Source = gameObject 
                    });
                }
            }
            ReturnToPool();
        }

        private void ReturnToPool()
        {
            // Would normally return to pool
            gameObject.SetActive(false);
        }
    }

    #endregion
}
