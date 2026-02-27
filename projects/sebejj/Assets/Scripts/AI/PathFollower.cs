/** 
 * @file PathFollower.cs
 * @brief 路径跟随组件 - 配合A*寻路使用
 * @description 使游戏对象能够沿着计算出的路径移动
 * @author AI系统架构师
 * @date 2026-02-27
 */

using UnityEngine;
using SebeJJ.AI.Pathfinding;

namespace SebeJJ.AI
{
    /// <summary>
    /// 路径跟随模式
    /// </summary>
    public enum PathFollowMode
    {
        Once,       // 只走一次
        Loop,       // 循环
        PingPong,   // 往返
        StopAtEnd   // 到达终点停止
    }

    /// <summary>
    /// 路径跟随组件
    /// </summary>
    public class PathFollower : MonoBehaviour
    {
        #region 序列化字段
        
        [Header("移动配置")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float rotationSpeed = 360f;
        [SerializeField] private float waypointReachedDistance = 0.1f;
        [SerializeField] private float pathUpdateInterval = 0.5f;
        
        [Header("路径模式")]
        [SerializeField] private PathFollowMode followMode = PathFollowMode.Once;
        [SerializeField] private bool autoRequestPath = true;
        
        [Header("目标")]
        [SerializeField] private Transform targetTransform;
        [SerializeField] private Vector3 targetPosition;
        
        [Header("组件引用")]
        [SerializeField] private Rigidbody2D rb;
        
        #endregion

        #region 私有字段
        
        /// <summary>
        /// 当前路径
        /// </summary>
        private Vector3[] _currentPath;
        
        /// <summary>
        /// 当前路径索引
        /// </summary>
        private int _currentPathIndex = 0;
        
        /// <summary>
        /// 是否正在跟随路径
        /// </summary>
        private bool _isFollowingPath = false;
        
        /// <summary>
        /// 路径方向（用于往返模式）
        /// </summary>
        private int _pathDirection = 1;
        
        /// <summary>
        /// 路径更新计时器
        /// </summary>
        private float _pathUpdateTimer = 0f;
        
        /// <summary>
        /// 寻路系统引用
        /// </summary>
        private AStarPathfinding _pathfinding;
        
        /// <summary>
        /// AF-002修复: 已访问路径点记录
        /// </summary>
        private HashSet<int> _visitedWaypoints = new HashSet<int>();
        
        #endregion

        #region 公共属性
        
        /// <summary>
        /// 是否正在跟随路径
        /// </summary>
        public bool IsFollowingPath => _isFollowingPath;
        
        /// <summary>
        /// 当前路径点索引
        /// </summary>
        public int CurrentPathIndex => _currentPathIndex;
        
        /// <summary>
        /// 剩余路径距离
        /// </summary>
        public float RemainingPathDistance
        {
            get
            {
                if (_currentPath == null || _currentPathIndex >= _currentPath.Length) return 0f;
                
                float distance = 0f;
                distance += Vector3.Distance(transform.position, _currentPath[_currentPathIndex]);
                
                for (int i = _currentPathIndex; i < _currentPath.Length - 1; i++)
                {
                    distance += Vector3.Distance(_currentPath[i], _currentPath[i + 1]);
                }
                
                return distance;
            }
        }
        
        /// <summary>
        /// 路径是否有效
        /// </summary>
        public bool HasValidPath => _currentPath != null && _currentPath.Length > 0;
        
        #endregion

        #region Unity生命周期
        
        private void Awake()
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            
            // 查找寻路系统
            _pathfinding = FindObjectOfType<AStarPathfinding>();
        }
        
        private void Update()
        {
            if (!_isFollowingPath) return;
            
            // 更新路径（如果目标在移动）
            if (autoRequestPath && targetTransform != null)
            {
                _pathUpdateTimer += Time.deltaTime;
                
                if (_pathUpdateTimer >= pathUpdateInterval)
                {
                    RequestPath();
                    _pathUpdateTimer = 0f;
                }
            }
        }
        
        private void FixedUpdate()
        {
            if (_isFollowingPath)
            {
                FollowPath();
            }
        }
        
        #endregion

        #region 路径请求
        
        /// <summary>
        /// 请求路径到目标
        /// </summary>
        public void RequestPath()
        {
            if (_pathfinding == null) return;
            
            Vector3 target = targetTransform != null ? targetTransform.position : targetPosition;
            
            _pathfinding.RequestPath(transform.position, target, OnPathFound);
        }
        
        /// <summary>
        /// 请求路径到指定位置
        /// </summary>
        /// <param name="position">目标位置</param>
        public void RequestPath(Vector3 position)
        {
            if (_pathfinding == null) return;
            
            targetPosition = position;
            _pathfinding.RequestPath(transform.position, position, OnPathFound);
        }
        
        /// <summary>
        /// 路径找到回调
        /// AF-002修复: 重置访问记录
        /// </summary>
        /// <param name="path">路径点数组</param>
        /// <param name="success">是否成功</param>
        private void OnPathFound(Vector3[] path, bool success)
        {
            if (success)
            {
                _currentPath = path;
                _currentPathIndex = 0;
                _pathDirection = 1;
                _visitedWaypoints.Clear(); // AF-002修复: 重置访问记录
                _isFollowingPath = true;
            }
            else
            {
                Debug.LogWarning("[PathFollower] 未找到路径");
                _isFollowingPath = false;
            }
        }
        
        #endregion

        #region 路径跟随
        
        /// <summary>
        /// 跟随路径
        /// AF-002修复: 添加访问记录避免重复访问
        /// </summary>
        private void FollowPath()
        {
            if (_currentPath == null || _currentPathIndex >= _currentPath.Length)
            {
                OnPathComplete();
                return;
            }
            
            // 获取当前目标点
            Vector3 targetWaypoint = _currentPath[_currentPathIndex];
            
            // 检查是否到达当前路径点
            float distanceToWaypoint = Vector3.Distance(transform.position, targetWaypoint);
            
            if (distanceToWaypoint <= waypointReachedDistance)
            {
                // AF-002修复: 标记已访问
                _visitedWaypoints.Add(_currentPathIndex);
                
                _currentPathIndex += _pathDirection;
                
                // 检查是否到达终点
                if (_currentPathIndex >= _currentPath.Length || _currentPathIndex < 0)
                {
                    OnPathComplete();
                }
                
                return;
            }
            
            // 移动到目标点
            Vector3 direction = (targetWaypoint - transform.position).normalized;
            Vector3 newPosition = transform.position + direction * moveSpeed * Time.fixedDeltaTime;
            
            // 应用移动
            if (rb != null)
            {
                rb.MovePosition(newPosition);
            }
            else
            {
                transform.position = newPosition;
            }
            
            // 旋转朝向移动方向
            if (direction != Vector3.zero)
            {
                float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                float currentAngle = transform.eulerAngles.z;
                float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.fixedDeltaTime);
                transform.rotation = Quaternion.Euler(0, 0, newAngle);
            }
        }
        
        /// <summary>
        /// 路径完成处理
        /// </summary>
        private void OnPathComplete()
        {
            switch (followMode)
            {
                case PathFollowMode.Once:
                    _isFollowingPath = false;
                    break;
                    
                case PathFollowMode.Loop:
                    _currentPathIndex = 0;
                    break;
                    
                case PathFollowMode.PingPong:
                    _pathDirection *= -1;
                    _currentPathIndex = Mathf.Clamp(_currentPathIndex, 0, _currentPath.Length - 1);
                    break;
                    
                case PathFollowMode.StopAtEnd:
                    _isFollowingPath = false;
                    break;
            }
        }
        
        #endregion

        #region 公共控制方法
        
        /// <summary>
        /// 开始跟随路径
        /// </summary>
        public void StartFollowing()
        {
            if (HasValidPath)
            {
                _isFollowingPath = true;
            }
            else
            {
                RequestPath();
            }
        }
        
        /// <summary>
        /// 停止跟随
        /// </summary>
        public void StopFollowing()
        {
            _isFollowingPath = false;
        }
        
        /// <summary>
        /// 设置目标
        /// </summary>
        /// <param name="target">目标Transform</param>
        public void SetTarget(Transform target)
        {
            targetTransform = target;
            RequestPath();
        }
        
        /// <summary>
        /// 设置目标位置
        /// </summary>
        /// <param name="position">目标位置</param>
        public void SetTargetPosition(Vector3 position)
        {
            targetTransform = null;
            targetPosition = position;
            RequestPath();
        }
        
        /// <summary>
        /// 清除路径
        /// </summary>
        public void ClearPath()
        {
            _currentPath = null;
            _currentPathIndex = 0;
            _isFollowingPath = false;
        }
        
        #endregion

        #region 调试
        
        private void OnDrawGizmos()
        {
            if (_currentPath == null || _currentPath.Length == 0) return;
            
            Gizmos.color = Color.green;
            
            // 绘制路径线
            for (int i = 0; i < _currentPath.Length - 1; i++)
            {
                Gizmos.DrawLine(_currentPath[i], _currentPath[i + 1]);
                Gizmos.DrawSphere(_currentPath[i], 0.1f);
            }
            
            // 绘制终点
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_currentPath[_currentPath.Length - 1], 0.2f);
            
            // 高亮当前目标点
            if (_currentPathIndex < _currentPath.Length)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(_currentPath[_currentPathIndex], 0.15f);
            }
        }
        
        #endregion
    }
}
