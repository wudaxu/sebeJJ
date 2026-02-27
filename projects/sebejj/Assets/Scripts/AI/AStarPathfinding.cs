/** 
 * @file AStarPathfinding.cs
 * @brief A*寻路系统 - 任务PT-001~006
 * @description 实现2D网格A*寻路算法，支持动态障碍物、路径平滑和性能优化
 * @author AI系统架构师
 * @date 2026-02-27
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace SebeJJ.AI.Pathfinding
{
    /// <summary>
    /// 网格节点
    /// </summary>
    public class PathNode : IComparable<PathNode>
    {
        /// <summary>
        /// 网格坐标X
        /// </summary>
        public int X { get; set; }
        
        /// <summary>
        /// 网格坐标Y
        /// </summary>
        public int Y { get; set; }
        
        /// <summary>
        /// 世界坐标
        /// </summary>
        public Vector3 WorldPosition { get; set; }
        
        /// <summary>
        /// 是否可行走
        /// </summary>
        public bool IsWalkable { get; set; } = true;
        
        /// <summary>
        /// 移动代价
        /// </summary>
        public float MovementCost { get; set; } = 1f;
        
        /// <summary>
        /// G值 - 从起点到当前节点的实际代价
        /// </summary>
        public float GCost { get; set; } = float.MaxValue;
        
        /// <summary>
        /// H值 - 从当前节点到终点的估计代价（启发值）
        /// </summary>
        public float HCost { get; set; } = 0f;
        
        /// <summary>
        /// F值 = G + H
        /// </summary>
        public float FCost => GCost + HCost;
        
        /// <summary>
        /// 父节点
        /// </summary>
        public PathNode Parent { get; set; }
        
        /// <summary>
        /// 节点版本（用于路径缓存）
        /// </summary>
        public int Version { get; set; } = 0;
        
        /// <summary>
        /// 是否在开放列表中
        /// </summary>
        public bool IsInOpenSet { get; set; } = false;
        
        /// <summary>
        /// 是否在关闭列表中
        /// </summary>
        public bool IsInClosedSet { get; set; } = false;
        
        public PathNode(int x, int y, Vector3 worldPos)
        {
            X = x;
            Y = y;
            WorldPosition = worldPos;
        }
        
        /// <summary>
        /// 重置节点状态
        /// </summary>
        public void Reset()
        {
            GCost = float.MaxValue;
            HCost = 0f;
            Parent = null;
            IsInOpenSet = false;
            IsInClosedSet = false;
        }
        
        /// <summary>
        /// 比较节点（用于优先队列）
        /// </summary>
        public int CompareTo(PathNode other)
        {
            int compare = FCost.CompareTo(other.FCost);
            if (compare == 0)
            {
                compare = HCost.CompareTo(other.HCost);
            }
            return compare;
        }
        
        public override bool Equals(object obj)
        {
            if (obj is PathNode other)
            {
                return X == other.X && Y == other.Y;
            }
            return false;
        }
        
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ (Y.GetHashCode() << 2);
        }
        
        public override string ToString()
        {
            return $"Node({X}, {Y}) G:{GCost:F1} H:{HCost:F1} F:{FCost:F1}";
        }
    }

    /// <summary>
    /// 路径请求
    /// </summary>
    public class PathRequest
    {
        /// <summary>
        /// 起点
        /// </summary>
        public Vector3 StartPosition { get; set; }
        
        /// <summary>
        /// 终点
        /// </summary>
        public Vector3 EndPosition { get; set; }
        
        /// <summary>
        /// 路径回调
        /// </summary>
        public Action<Vector3[], bool> Callback { get; set; }
        
        /// <summary>
        /// 请求ID
        /// </summary>
        public int RequestId { get; set; }
        
        /// <summary>
        /// 请求时间
        /// </summary>
        public float RequestTime { get; set; }
    }

    /// <summary>
    /// A*寻路系统
    /// </summary>
    public class AStarPathfinding : MonoBehaviour
    {
        #region 事件定义
        
        /// <summary>
        /// 网格更新事件
        /// </summary>
        public event Action OnGridUpdated;
        
        /// <summary>
        /// 路径计算完成事件
        /// </summary>
        public event Action<Vector3[], bool> OnPathCalculated;
        
        #endregion

        #region 序列化字段
        
        [Header("网格配置")]
        [SerializeField] private Vector2 gridWorldSize = new Vector2(100f, 100f);
        [SerializeField] private float nodeRadius = 0.5f;
        [SerializeField] private LayerMask obstacleMask;
        [SerializeField] private bool showGridGizmos = false;
        
        [Header("寻路配置")]
        [SerializeField] private bool allowDiagonal = true;
        [SerializeField] private float diagonalCost = 1.414f;
        [SerializeField] private int maxIterations = 1000;
        [SerializeField] private bool useSmoothing = true;
        [SerializeField] private int smoothingIterations = 2;
        
        [Header("性能配置")]
        [SerializeField] private bool useAsyncPathfinding = true;
        [SerializeField] private int maxPathRequestsPerFrame = 5;
        [SerializeField] private float pathCacheDuration = 2f;
        
        #endregion

        #region 私有字段
        
        /// <summary>
        /// 节点直径
        /// </summary>
        private float _nodeDiameter;
        
        /// <summary>
        /// 网格大小
        /// </summary>
        private int _gridSizeX, _gridSizeY;
        
        /// <summary>
        /// 网格节点数组
        /// </summary>
        private PathNode[,] _grid;
        
        /// <summary>
        /// 路径请求队列
        /// </summary>
        private Queue<PathRequest> _pathRequestQueue = new Queue<PathRequest>();
        
        /// <summary>
        /// 是否正在处理路径
        /// </summary>
        private bool _isProcessingPath = false;
        
        /// <summary>
        /// 路径缓存
        /// </summary>
        private Dictionary<Vector2Int, CachedPath> _pathCache = new Dictionary<Vector2Int, CachedPath>();
        
        /// <summary>
        /// 当前版本号
        /// </summary>
        private int _currentVersion = 0;
        
        /// <summary>
        /// 请求计数器
        /// </summary>
        private int _requestIdCounter = 0;
        
        #endregion

        #region 公共属性
        
        /// <summary>
        /// 网格宽度
        /// </summary>
        public int GridSizeX => _gridSizeX;
        
        /// <summary>
        /// 网格高度
        /// </summary>
        public int GridSizeY => _gridSizeY;
        
        /// <summary>
        /// 节点半径
        /// </summary>
        public float NodeRadius => nodeRadius;
        
        /// <summary>
        /// 网格世界大小
        /// </summary>
        public Vector2 GridWorldSize => gridWorldSize;
        
        #endregion

        #region Unity生命周期
        
        private void Awake()
        {
            InitializeGrid();
        }
        
        private void Update()
        {
            // 处理路径请求队列
            ProcessPathRequests();
            
            // 清理过期缓存
            CleanupCache();
        }
        
        private void OnDrawGizmos()
        {
            if (!showGridGizmos || _grid == null) return;
            
            DrawGridGizmos();
        }
        
        #endregion

        #region 网格初始化
        
        /// <summary>
        /// 初始化网格
        /// </summary>
        private void InitializeGrid()
        {
            _nodeDiameter = nodeRadius * 2f;
            _gridSizeX = Mathf.RoundToInt(gridWorldSize.x / _nodeDiameter);
            _gridSizeY = Mathf.RoundToInt(gridWorldSize.y / _nodeDiameter);
            
            _grid = new PathNode[_gridSizeX, _gridSizeY];
            
            Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2f 
                                                       - Vector3.up * gridWorldSize.y / 2f;
            
            for (int x = 0; x < _gridSizeX; x++)
            {
                for (int y = 0; y < _gridSizeY; y++)
                {
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * _nodeDiameter + nodeRadius)
                                                         + Vector3.up * (y * _nodeDiameter + nodeRadius);
                    
                    bool walkable = !Physics2D.OverlapCircle(worldPoint, nodeRadius * 0.9f, obstacleMask);
                    
                    _grid[x, y] = new PathNode(x, y, worldPoint)
                    {
                        IsWalkable = walkable
                    };
                }
            }
            
            Debug.Log($"[AStarPathfinding] 网格初始化完成: {_gridSizeX}x{_gridSizeY}");
        }
        
        /// <summary>
        /// 重新扫描网格
        /// </summary>
        public void RescanGrid()
        {
            Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2f 
                                                       - Vector3.up * gridWorldSize.y / 2f;
            
            for (int x = 0; x < _gridSizeX; x++)
            {
                for (int y = 0; y < _gridSizeY; y++)
                {
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * _nodeDiameter + nodeRadius)
                                                         + Vector3.up * (y * _nodeDiameter + nodeRadius);
                    
                    _grid[x, y].IsWalkable = !Physics2D.OverlapCircle(worldPoint, nodeRadius * 0.9f, obstacleMask);
                }
            }
            
            _currentVersion++;
            OnGridUpdated?.Invoke();
            
            Debug.Log("[AStarPathfinding] 网格重新扫描完成");
        }
        
        /// <summary>
        /// 更新特定区域的网格
        /// </summary>
        /// <param name="worldPosition">世界坐标</param>
        /// <param name="radius">半径</param>
        public void UpdateGridRegion(Vector3 worldPosition, float radius)
        {
            PathNode centerNode = GetNodeFromWorldPosition(worldPosition);
            if (centerNode == null) return;
            
            int nodeRadius = Mathf.CeilToInt(radius / _nodeDiameter);
            
            for (int x = -nodeRadius; x <= nodeRadius; x++)
            {
                for (int y = -nodeRadius; y <= nodeRadius; y++)
                {
                    int checkX = centerNode.X + x;
                    int checkY = centerNode.Y + y;
                    
                    if (checkX >= 0 && checkX < _gridSizeX && checkY >= 0 && checkY < _gridSizeY)
                    {
                        PathNode node = _grid[checkX, checkY];
                        node.IsWalkable = !Physics2D.OverlapCircle(node.WorldPosition, nodeRadius * 0.9f, obstacleMask);
                    }
                }
            }
            
            _currentVersion++;
            OnGridUpdated?.Invoke();
        }
        
        #endregion

        #region 节点操作
        
        /// <summary>
        /// 从世界坐标获取节点
        /// </summary>
        /// <param name="worldPosition">世界坐标</param>
        /// <returns>节点</returns>
        public PathNode GetNodeFromWorldPosition(Vector3 worldPosition)
        {
            float percentX = (worldPosition.x + gridWorldSize.x / 2f) / gridWorldSize.x;
            float percentY = (worldPosition.y + gridWorldSize.y / 2f) / gridWorldSize.y;
            
            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);
            
            int x = Mathf.RoundToInt((_gridSizeX - 1) * percentX);
            int y = Mathf.RoundToInt((_gridSizeY - 1) * percentY);
            
            return _grid[x, y];
        }
        
        /// <summary>
        /// 获取邻居节点
        /// </summary>
        /// <param name="node">当前节点</param>
        /// <returns>邻居节点列表</returns>
        private List<PathNode> GetNeighbors(PathNode node)
        {
            List<PathNode> neighbors = new List<PathNode>();
            
            // 四方向
            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };
            
            for (int i = 0; i < 4; i++)
            {
                int checkX = node.X + dx[i];
                int checkY = node.Y + dy[i];
                
                if (checkX >= 0 && checkX < _gridSizeX && checkY >= 0 && checkY < _gridSizeY)
                {
                    neighbors.Add(_grid[checkX, checkY]);
                }
            }
            
            // 对角线方向
            if (allowDiagonal)
            {
                int[] ddx = { -1, 1, -1, 1 };
                int[] ddy = { -1, -1, 1, 1 };
                
                for (int i = 0; i < 4; i++)
                {
                    int checkX = node.X + ddx[i];
                    int checkY = node.Y + ddy[i];
                    
                    if (checkX >= 0 && checkX < _gridSizeX && checkY >= 0 && checkY < _gridSizeY)
                    {
                        // 检查对角线是否被阻挡
                        PathNode neighborX = _grid[node.X + ddx[i], node.Y];
                        PathNode neighborY = _grid[node.X, node.Y + ddy[i]];
                        
                        if (neighborX.IsWalkable || neighborY.IsWalkable)
                        {
                            neighbors.Add(_grid[checkX, checkY]);
                        }
                    }
                }
            }
            
            return neighbors;
        }
        
        /// <summary>
        /// 获取移动代价
        /// </summary>
        private float GetMovementCost(PathNode from, PathNode to)
        {
            float dx = Mathf.Abs(from.X - to.X);
            float dy = Mathf.Abs(from.Y - to.Y);
            
            if (dx > 0 && dy > 0)
            {
                return diagonalCost * to.MovementCost;
            }
            
            return to.MovementCost;
        }
        
        /// <summary>
        /// 计算启发值（曼哈顿距离）
        /// </summary>
        private float GetHeuristic(PathNode from, PathNode to)
        {
            return Mathf.Abs(from.X - to.X) + Mathf.Abs(from.Y - to.Y);
        }
        
        #endregion

        #region 路径计算
        
        /// <summary>
        /// 请求路径
        /// </summary>
        /// <param name="startPos">起点</param>
        /// <param name="endPos">终点</param>
        /// <param name="callback">回调函数</param>
        public void RequestPath(Vector3 startPos, Vector3 endPos, Action<Vector3[], bool> callback)
        {
            PathRequest request = new PathRequest
            {
                StartPosition = startPos,
                EndPosition = endPos,
                Callback = callback,
                RequestId = _requestIdCounter++,
                RequestTime = Time.time
            };
            
            _pathRequestQueue.Enqueue(request);
        }
        
        /// <summary>
        /// 立即计算路径（同步）
        /// </summary>
        /// <param name="startPos">起点</param>
        /// <param name="endPos">终点</param>
        /// <returns>路径点数组和是否成功</returns>
        public (Vector3[] path, bool success) FindPathImmediate(Vector3 startPos, Vector3 endPos)
        {
            PathNode startNode = GetNodeFromWorldPosition(startPos);
            PathNode endNode = GetNodeFromWorldPosition(endPos);
            
            if (startNode == null || endNode == null)
            {
                return (null, false);
            }
            
            // 检查缓存
            Vector2Int cacheKey = new Vector2Int(startNode.X * 10000 + startNode.Y, endNode.X * 10000 + endNode.Y);
            if (_pathCache.TryGetValue(cacheKey, out var cachedPath))
            {
                if (Time.time - cachedPath.CacheTime < pathCacheDuration)
                {
                    return (cachedPath.Path, true);
                }
            }
            
            Vector3[] path = CalculatePath(startNode, endNode);
            bool success = path != null && path.Length > 0;
            
            if (success)
            {
                _pathCache[cacheKey] = new CachedPath { Path = path, CacheTime = Time.time };
            }
            
            return (path, success);
        }
        
        /// <summary>
        /// 处理路径请求队列
        /// </summary>
        private void ProcessPathRequests()
        {
            if (_isProcessingPath || _pathRequestQueue.Count == 0) return;
            
            int processedCount = 0;
            
            while (_pathRequestQueue.Count > 0 && processedCount < maxPathRequestsPerFrame)
            {
                PathRequest request = _pathRequestQueue.Dequeue();
                
                var (path, success) = FindPathImmediate(request.StartPosition, request.EndPosition);
                
                request.Callback?.Invoke(path, success);
                OnPathCalculated?.Invoke(path, success);
                
                processedCount++;
            }
        }
        
        /// <summary>
        /// 计算路径（A*算法核心）
        /// </summary>
        private Vector3[] CalculatePath(PathNode startNode, PathNode endNode)
        {
            if (!startNode.IsWalkable || !endNode.IsWalkable)
            {
                return null;
            }
            
            // 使用优先队列（二叉堆）
            Heap<PathNode> openSet = new Heap<PathNode>(_gridSizeX * _gridSizeY);
            HashSet<PathNode> closedSet = new HashSet<PathNode>();
            
            // 重置节点状态
            for (int x = 0; x < _gridSizeX; x++)
            {
                for (int y = 0; y < _gridSizeY; y++)
                {
                    _grid[x, y].Reset();
                    _grid[x, y].Version = _currentVersion;
                }
            }
            
            openSet.Add(startNode);
            startNode.GCost = 0f;
            startNode.HCost = GetHeuristic(startNode, endNode);
            
            int iterations = 0;
            
            while (openSet.Count > 0)
            {
                if (iterations++ > maxIterations)
                {
                    Debug.LogWarning("[AStarPathfinding] 路径计算超过最大迭代次数");
                    return null;
                }
                
                PathNode currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);
                
                if (currentNode == endNode)
                {
                    return RetracePath(startNode, endNode);
                }
                
                foreach (PathNode neighbor in GetNeighbors(currentNode))
                {
                    if (!neighbor.IsWalkable || closedSet.Contains(neighbor))
                    {
                        continue;
                    }
                    
                    float newMovementCostToNeighbor = currentNode.GCost + GetMovementCost(currentNode, neighbor);
                    
                    if (newMovementCostToNeighbor < neighbor.GCost || !openSet.Contains(neighbor))
                    {
                        neighbor.GCost = newMovementCostToNeighbor;
                        neighbor.HCost = GetHeuristic(neighbor, endNode);
                        neighbor.Parent = currentNode;
                        
                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                        else
                        {
                            openSet.UpdateItem(neighbor);
                        }
                    }
                }
            }
            
            return null; // 未找到路径
        }
        
        /// <summary>
        /// 回溯路径
        /// </summary>
        private Vector3[] RetracePath(PathNode startNode, PathNode endNode)
        {
            List<Vector3> path = new List<Vector3>();
            PathNode currentNode = endNode;
            
            while (currentNode != startNode)
            {
                path.Add(currentNode.WorldPosition);
                currentNode = currentNode.Parent;
            }
            
            path.Add(startNode.WorldPosition);
            path.Reverse();
            
            // 路径平滑
            if (useSmoothing)
            {
                path = SmoothPath(path);
            }
            
            return path.ToArray();
        }
        
        /// <summary>
        /// 路径平滑
        /// </summary>
        private List<Vector3> SmoothPath(List<Vector3> path)
        {
            if (path.Count <= 2) return path;
            
            for (int i = 0; i < smoothingIterations; i++)
            {
                path = SimplifyPath(path);
            }
            
            return path;
        }
        
        /// <summary>
        /// 简化路径（移除不必要的拐点）
        /// </summary>
        private List<Vector3> SimplifyPath(List<Vector3> path)
        {
            if (path.Count <= 2) return path;
            
            List<Vector3> simplified = new List<Vector3>();
            simplified.Add(path[0]);
            
            int currentIndex = 0;
            
            while (currentIndex < path.Count - 1)
            {
                // 尝试找到最远的可以直接到达的点
                int furthestIndex = currentIndex + 1;
                
                for (int i = currentIndex + 2; i < path.Count; i++)
                {
                    if (HasLineOfSight(path[currentIndex], path[i]))
                    {
                        furthestIndex = i;
                    }
                    else
                    {
                        break;
                    }
                }
                
                simplified.Add(path[furthestIndex]);
                currentIndex = furthestIndex;
            }
            
            return simplified;
        }
        
        /// <summary>
        /// 检查两点之间是否有视线
        /// </summary>
        private bool HasLineOfSight(Vector3 from, Vector3 to)
        {
            Vector3 direction = to - from;
            float distance = direction.magnitude;
            direction.Normalize();
            
            RaycastHit2D hit = Physics2D.Raycast(from, direction, distance, obstacleMask);
            return !hit;
        }
        
        #endregion

        #region 缓存管理
        
        /// <summary>
        /// 清理过期缓存
        /// </summary>
        private void CleanupCache()
        {
            List<Vector2Int> keysToRemove = new List<Vector2Int>();
            
            foreach (var kvp in _pathCache)
            {
                if (Time.time - kvp.Value.CacheTime > pathCacheDuration)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }
            
            foreach (var key in keysToRemove)
            {
                _pathCache.Remove(key);
            }
        }
        
        /// <summary>
        /// 清除所有缓存
        /// </summary>
        public void ClearCache()
        {
            _pathCache.Clear();
        }
        
        #endregion

        #region 调试可视化
        
        private void DrawGridGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 0.1f));
            
            if (_grid == null) return;
            
            foreach (var node in _grid)
            {
                Gizmos.color = node.IsWalkable ? Color.white : Color.red;
                Gizmos.DrawCube(node.WorldPosition, Vector3.one * (_nodeDiameter - 0.1f));
            }
        }
        
        #endregion
    }

    /// <summary>
    /// 缓存的路径
    /// </summary>
    public class CachedPath
    {
        public Vector3[] Path { get; set; }
        public float CacheTime { get; set; }
    }

    /// <summary>
    /// 二叉堆实现（用于优先队列）
        /// </summary>
    public class Heap<T> where T : IComparable<T>
    {
        private T[] _items;
        private int _currentItemCount;
        
        public Heap(int maxHeapSize)
        {
            _items = new T[maxHeapSize];
        }
        
        public int Count => _currentItemCount;
        
        public void Add(T item)
        {
            item.CompareTo(item); // 确保实现了接口
            _items[_currentItemCount] = item;
            SortUp(_currentItemCount);
            _currentItemCount++;
        }
        
        public T RemoveFirst()
        {
            T firstItem = _items[0];
            _currentItemCount--;
            _items[0] = _items[_currentItemCount];
            _items[_currentItemCount] = default;
            SortDown(0);
            return firstItem;
        }
        
        public void UpdateItem(T item)
        {
            // 找到并更新
            for (int i = 0; i < _currentItemCount; i++)
            {
                if (_items[i].Equals(item))
                {
                    SortUp(i);
                    return;
                }
            }
        }
        
        public bool Contains(T item)
        {
            for (int i = 0; i < _currentItemCount; i++)
            {
                if (_items[i].Equals(item))
                {
                    return true;
                }
            }
            return false;
        }
        
        private void SortUp(int index)
        {
            int parentIndex = (index - 1) / 2;
            
            while (true)
            {
                T item = _items[index];
                T parentItem = _items[parentIndex];
                
                if (item.CompareTo(parentItem) > 0)
                {
                    Swap(index, parentIndex);
                    index = parentIndex;
                    parentIndex = (index - 1) / 2;
                }
                else
                {
                    break;
                }
            }
        }
        
        private void SortDown(int index)
        {
            while (true)
            {
                int childIndexLeft = index * 2 + 1;
                int childIndexRight = index * 2 + 2;
                int swapIndex = 0;
                
                if (childIndexLeft < _currentItemCount)
                {
                    swapIndex = childIndexLeft;
                    
                    if (childIndexRight < _currentItemCount)
                    {
                        if (_items[childIndexLeft].CompareTo(_items[childIndexRight]) < 0)
                        {
                            swapIndex = childIndexRight;
                        }
                    }
                    
                    if (_items[index].CompareTo(_items[swapIndex]) < 0)
                    {
                        Swap(index, swapIndex);
                        index = swapIndex;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
        }
        
        private void Swap(int indexA, int indexB)
        {
            T temp = _items[indexA];
            _items[indexA] = _items[indexB];
            _items[indexB] = temp;
        }
    }
}
