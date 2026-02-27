using UnityEngine;
using System;
using System.Collections;

namespace SebeJJ.Player
{
    /// <summary>
    /// 采集器 - 处理机甲的资源采集逻辑
    /// </summary>
    public class MechCollector : MonoBehaviour
    {
        [Header("采集设置")]
        public float collectRange = 2.5f;
        public float collectDuration = 1.5f;
        public float beamRange = 15f;
        public LayerMask collectibleLayers;
        
        [Header("牵引设置")]
        public float pullForce = 10f;
        public float maxPullDistance = 8f;
        
        [Header("组件引用")]
        public Transform collectPoint;
        public Transform beamOrigin;
        public LineRenderer tractorBeam;
        
        // 状态
        private CollectibleResource currentTarget;
        private bool isCollecting;
        private bool isPulling;
        private float collectProgress;
        
        // 事件
        public event Action<CollectibleResource> OnCollectStarted;
        public event Action<CollectibleResource> OnCollectCompleted;
        public event Action<CollectibleResource> OnCollectCancelled;
        public event Action<CollectibleResource> OnTargetLocked;
        public event Action OnTargetLost;
        
        private void Update()
        {
            HandleInput();
            UpdateCollection();
            UpdateTractorBeam();
        }
        
        private void HandleInput()
        {
            // 锁定目标
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                CycleTarget();
            }
            
            // 开始采集
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (currentTarget != null && !isCollecting)
                {
                    StartCollection();
                }
            }
            
            // 取消采集
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E))
            {
                if (isCollecting)
                {
                    CancelCollection();
                }
            }
        }
        
        /// <summary>
        /// 循环选择目标
        /// </summary>
        private void CycleTarget()
        {
            var resources = FindObjectsOfType<CollectibleResource>();
            CollectibleResource nearest = null;
            float nearestDist = float.MaxValue;
            
            foreach (var resource in resources)
            {
                if (!resource.CanCollect()) continue;
                
                float dist = Vector2.Distance(transform.position, resource.transform.position);
                if (dist <= beamRange && dist < nearestDist)
                {
                    // 如果已有目标，选择下一个
                    if (currentTarget != null && resource == currentTarget)
                        continue;
                    
                    nearest = resource;
                    nearestDist = dist;
                }
            }
            
            SetTarget(nearest);
        }
        
        /// <summary>
        /// 设置采集目标
        /// </summary>
        public void SetTarget(CollectibleResource target)
        {
            // 清除旧目标高亮
            if (currentTarget != null)
            {
                currentTarget.SetHighlighted(false);
            }
            
            currentTarget = target;
            
            if (currentTarget != null)
            {
                currentTarget.SetHighlighted(true);
                OnTargetLocked?.Invoke(currentTarget);
                
                // 显示目标信息
                Utils.GameEvents.TriggerNotification($"目标: {currentTarget.ResourceName}");
            }
            else
            {
                OnTargetLost?.Invoke();
            }
        }
        
        /// <summary>
        /// 开始采集
        /// </summary>
        private void StartCollection()
        {
            if (currentTarget == null) return;
            
            float dist = Vector2.Distance(transform.position, currentTarget.transform.position);
            
            // 如果距离太远，先牵引
            if (dist > collectRange)
            {
                StartCoroutine(PullTarget());
                return;
            }
            
            // 开始采集
            isCollecting = true;
            collectProgress = 0f;
            
            // 消耗能源
            Core.GameManager.Instance?.resourceManager?.ConsumeEnergy(3f);
            
            OnCollectStarted?.Invoke(currentTarget);
            
            // 播放采集特效
            Utils.EffectManager.Instance?.PlayCollectEffect(currentTarget.transform.position);
            
            Debug.Log($"[MechCollector] 开始采集: {currentTarget.ResourceName}");
        }
        
        /// <summary>
        /// 牵引目标
        /// </summary>
        private IEnumerator PullTarget()
        {
            isPulling = true;
            
            // 播放牵引光束特效
            if (tractorBeam != null)
            {
                tractorBeam.enabled = true;
            }
            
            Utils.AudioManager.Instance?.PlaySFX(
                Utils.AudioManager.Instance?.GetClip("tractor_beam"));
            
            while (currentTarget != null && isPulling)
            {
                float dist = Vector2.Distance(transform.position, currentTarget.transform.position);
                
                // 到达采集范围
                if (dist <= collectRange)
                {
                    isPulling = false;
                    StartCollection();
                    yield break;
                }
                
                // 超出最大牵引距离
                if (dist > maxPullDistance)
                {
                    CancelPull();
                    yield break;
                }
                
                // 牵引物体
                Vector2 direction = (transform.position - currentTarget.transform.position).normalized;
                Rigidbody2D rb = currentTarget.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.AddForce(direction * pullForce, ForceMode2D.Force);
                }
                else
                {
                    currentTarget.transform.position += (Vector3)(direction * pullForce * Time.deltaTime);
                }
                
                yield return null;
            }
            
            CancelPull();
        }
        
        private void CancelPull()
        {
            isPulling = false;
            if (tractorBeam != null)
            {
                tractorBeam.enabled = false;
            }
        }
        
        /// <summary>
        /// 更新采集进度
        /// </summary>
        private void UpdateCollection()
        {
            if (!isCollecting || currentTarget == null) return;
            
            // 检查目标是否还在范围内
            float dist = Vector2.Distance(transform.position, currentTarget.transform.position);
            if (dist > collectRange * 1.5f)
            {
                CancelCollection();
                return;
            }
            
            // 更新进度
            collectProgress += Time.deltaTime;
            
            // 更新UI
            Core.UIManager.Instance?.UpdateCollectProgress(collectProgress / collectDuration);
            
            // 采集完成
            if (collectProgress >= collectDuration)
            {
                CompleteCollection();
            }
        }
        
        /// <summary>
        /// 更新牵引光束
        /// </summary>
        private void UpdateTractorBeam()
        {
            if (tractorBeam == null) return;
            
            if (isPulling && currentTarget != null)
            {
                tractorBeam.SetPosition(0, beamOrigin.position);
                tractorBeam.SetPosition(1, currentTarget.transform.position);
                
                // 根据距离调整光束颜色
                float dist = Vector2.Distance(transform.position, currentTarget.transform.position);
                float ratio = Mathf.Clamp01(dist / maxPullDistance);
                tractorBeam.startColor = Color.Lerp(Color.cyan, Color.red, ratio);
                tractorBeam.endColor = tractorBeam.startColor;
            }
            else
            {
                tractorBeam.enabled = false;
            }
        }
        
        /// <summary>
        /// 完成采集
        /// </summary>
        private void CompleteCollection()
        {
            if (currentTarget == null) return;
            
            // 执行采集
            if (currentTarget.Collect())
            {
                OnCollectCompleted?.Invoke(currentTarget);
                
                // 更新委托进度
                Core.GameManager.Instance?.missionManager?.UpdateMissionProgress(
                    currentTarget.resourceId, 1);
                
                // 播放完成特效
                Utils.EffectManager.Instance?.PlayCollectCompleteEffect(
                    currentTarget.transform.position);
                
                Utils.AudioManager.Instance?.PlaySFX(
                    Utils.AudioManager.Instance?.GetClip("collect_complete"));
                
                Debug.Log($"[MechCollector] 采集完成: {currentTarget.ResourceName}");
            }
            
            currentTarget = null;
            isCollecting = false;
            collectProgress = 0f;
            
            Core.UIManager.Instance?.UpdateCollectProgress(0f);
        }
        
        /// <summary>
        /// 取消采集
        /// </summary>
        private void CancelCollection()
        {
            if (!isCollecting) return;
            
            isCollecting = false;
            collectProgress = 0f;
            
            Core.UIManager.Instance?.UpdateCollectProgress(0f);
            
            if (currentTarget != null)
            {
                OnCollectCancelled?.Invoke(currentTarget);
            }
            
            Debug.Log("[MechCollector] 采集取消");
        }
        
        /// <summary>
        /// 获取当前目标
        /// </summary>
        public CollectibleResource GetCurrentTarget()
        {
            return currentTarget;
        }
        
        /// <summary>
        /// 是否正在采集
        /// </summary>
        public bool IsCollecting => isCollecting;
        
        /// <summary>
        /// 获取采集进度 (0-1)
        /// </summary>
        public float GetCollectProgress()
        {
            return isCollecting ? collectProgress / collectDuration : 0f;
        }
        
        private void OnDrawGizmosSelected()
        {
            // 采集范围
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, collectRange);
            
            // 牵引范围
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, beamRange);
            
            // 最大牵引距离
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, maxPullDistance);
            
            // 当前目标连线
            if (currentTarget != null)
            {
                Gizmos.color = isCollecting ? Color.green : Color.yellow;
                Gizmos.DrawLine(transform.position, currentTarget.transform.position);
            }
        }
    }
}
