using UnityEngine;
using System.Collections;
using SebeJJ.Core;
using SebeJJ.Player;

namespace SebeJJ.Mech
{
    /// <summary>
    /// 采集器组件 - 处理资源采集
    /// </summary>
    public class Collector : MonoBehaviour
    {
        [Header("采集设置")]
        [SerializeField] private float collectRange = 3f;
        [SerializeField] private float collectSpeed = 1f;
        [SerializeField] private float energyCostPerSecond = 2f;
        [SerializeField] private LayerMask collectableLayers;

        [Header("视觉效果")]
        [SerializeField] private LineRenderer laserLine;
        [SerializeField] private ParticleSystem collectParticles;
        [SerializeField] private Transform laserOrigin;

        [Header("引用")]
        [SerializeField] private MechStatus mechStatus;
        [SerializeField] private MechController mechController;
        [SerializeField] private ResourceInventory inventory;

        // 状态
        private ICollectable _currentTarget;
        private bool _isCollecting;
        private float _collectProgress;

        // 属性
        public bool IsCollecting => _isCollecting;
        public ICollectable CurrentTarget => _currentTarget;
        public float CollectProgress => _collectProgress;

        // 事件
        public System.Action<ICollectable> OnStartCollecting;
        public System.Action<ICollectable> OnStopCollecting;
        public System.Action<ResourceType, int> OnResourceCollected;

        private void Awake()
        {
            if (mechStatus == null) mechStatus = GetComponent<MechStatus>();
            if (mechController == null) mechController = GetComponent<MechController>();
            if (inventory == null) inventory = GetComponent<ResourceInventory>();

            SetupLaserLine();
        }

        private void SetupLaserLine()
        {
            if (laserLine == null)
            {
                GameObject laserObj = new GameObject("CollectLaser");
                laserObj.transform.SetParent(transform);
                laserLine = laserObj.AddComponent<LineRenderer>();
            }

            laserLine.startWidth = 0.05f;
            laserLine.endWidth = 0.05f;
            laserLine.positionCount = 2;
            laserLine.useWorldSpace = true;
            laserLine.enabled = false;

            // 设置材质（绿色采集激光）
            laserLine.startColor = Color.green;
            laserLine.endColor = new Color(0, 1, 0, 0.5f);
        }

        private void Update()
        {
            HandleInput();
            UpdateCollection();
            UpdateVisuals();
        }

        /// <summary>
        /// 处理输入
        /// </summary>
        private void HandleInput()
        {
            if (Input.GetButtonDown("Collect"))
            {
                TryStartCollecting();
            }
            else if (Input.GetButtonUp("Collect"))
            {
                StopCollecting();
            }
        }

        /// <summary>
        /// 尝试开始采集
        /// </summary>
        private void TryStartCollecting()
        {
            // 寻找最近的采集目标
            ICollectable target = FindNearestCollectable();
            
            if (target != null)
            {
                StartCollecting(target);
            }
        }

        /// <summary>
        /// 寻找最近的采集目标
        /// </summary>
        private ICollectable FindNearestCollectable()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, collectRange, collectableLayers);
            
            ICollectable nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent<ICollectable>(out var collectable))
                {
                    if (!collectable.CanCollect) continue;

                    float distance = Vector2.Distance(transform.position, collider.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearest = collectable;
                    }
                }
            }

            return nearest;
        }

        /// <summary>
        /// 开始采集
        /// </summary>
        public void StartCollecting(ICollectable target)
        {
            if (target == null || !target.CanCollect) return;

            _currentTarget = target;
            _isCollecting = true;
            _collectProgress = 0f;

            // 锁定移动
            mechController?.LockMovement();

            OnStartCollecting?.Invoke(target);
        }

        /// <summary>
        /// 停止采集
        /// </summary>
        public void StopCollecting()
        {
            if (!_isCollecting) return;

            _isCollecting = false;
            _collectProgress = 0f;

            // 解锁移动
            mechController?.UnlockMovement();

            OnStopCollecting?.Invoke(_currentTarget);
            _currentTarget = null;

            // 隐藏激光
            laserLine.enabled = false;
            
            if (collectParticles != null)
            {
                collectParticles.Stop();
            }
        }

        /// <summary>
        /// 更新采集过程
        /// </summary>
        private void UpdateCollection()
        {
            if (!_isCollecting || _currentTarget == null)
            {
                StopCollecting();
                return;
            }

            // 检查目标是否仍然有效
            if (_currentTarget is MonoBehaviour mb && mb == null)
            {
                StopCollecting();
                return;
            }

            // 检查距离
            float distance = Vector2.Distance(transform.position, 
                ((MonoBehaviour)_currentTarget).transform.position);
            if (distance > collectRange * 1.5f)
            {
                StopCollecting();
                return;
            }

            // 检查能量
            if (mechStatus != null && mechStatus.CurrentEnergy < energyCostPerSecond * Time.deltaTime)
            {
                StopCollecting();
                return;
            }

            // 消耗能量
            mechStatus?.ConsumeEnergy(energyCostPerSecond * Time.deltaTime);

            // 更新进度
            _collectProgress += collectSpeed * Time.deltaTime;

            // 采集完成
            if (_collectProgress >= _currentTarget.CollectTime)
            {
                CompleteCollection();
            }
        }

        /// <summary>
        /// 完成采集
        /// </summary>
        private void CompleteCollection()
        {
            if (_currentTarget == null) return;

            // 获取资源
            var resources = _currentTarget.Collect();
            
            foreach (var resource in resources)
            {
                // 添加到库存
                if (inventory != null)
                {
                    inventory.AddResource(resource.type, resource.amount);
                }

                // 触发事件
                OnResourceCollected?.Invoke(resource.type, resource.amount);
                GameEvents.OnResourceCollected?.Invoke(resource.type, resource.amount);
            }

            // 停止采集
            StopCollecting();
        }

        /// <summary>
        /// 更新视觉效果
        /// </summary>
        private void UpdateVisuals()
        {
            if (!_isCollecting || _currentTarget == null)
            {
                laserLine.enabled = false;
                return;
            }

            // 更新激光线
            Vector2 origin = laserOrigin != null ? laserOrigin.position : transform.position;
            Vector2 targetPos = ((MonoBehaviour)_currentTarget).transform.position;

            laserLine.SetPosition(0, origin);
            laserLine.SetPosition(1, targetPos);
            laserLine.enabled = true;

            // 粒子效果
            if (collectParticles != null)
            {
                collectParticles.transform.position = targetPos;
                if (!collectParticles.isPlaying)
                {
                    collectParticles.Play();
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            // 绘制采集范围
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, collectRange);
        }
    }

    /// <summary>
    /// 可采集接口
    /// </summary>
    public interface ICollectable
    {
        bool CanCollect { get; }
        float CollectTime { get; }
        System.Collections.Generic.List<ResourceDrop> Collect();
    }

    /// <summary>
    /// 资源掉落数据
    /// </summary>
    public struct ResourceDrop
    {
        public ResourceType type;
        public int amount;

        public ResourceDrop(ResourceType type, int amount)
        {
            this.type = type;
            this.amount = amount;
        }
    }
}
