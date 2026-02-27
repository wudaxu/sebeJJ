using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 等离子弹丸组件 - 穿透性能量弹丸
    /// </summary>
    public class PlasmaProjectile : MonoBehaviour
    {
        private PlasmaCannon ownerWeapon;
        private Vector2 direction;
        private PlasmaCannonData plasmaData;
        private float speed;
        private float lifetime;
        private float maxDistance;
        private float traveledDistance = 0f;
        
        private List<GameObject> hitTargets = new List<GameObject>();
        private int pierceCount = 0;
        private bool isInitialized = false;
        
        // 组件引用
        private TrailRenderer trailRenderer;
        private Light pointLight;
        private SpriteRenderer spriteRenderer;

        public void Initialize(PlasmaCannon owner, Vector2 dir, PlasmaCannonData data)
        {
            ownerWeapon = owner;
            direction = dir.normalized;
            plasmaData = data;
            speed = data?.projectileSpeed ?? 15f;
            lifetime = data?.projectileLifetime ?? 2f;
            maxDistance = data?.maxPierceDistance ?? 20f;
            isInitialized = true;

            // 设置旋转
            transform.up = direction;
            
            // 获取组件
            trailRenderer = GetComponent<TrailRenderer>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            pointLight = GetComponent<Light>();
            
            // 设置颜色
            if (spriteRenderer != null && data != null)
            {
                spriteRenderer.color = data.plasmaColor;
            }
            if (trailRenderer != null && data != null)
            {
                trailRenderer.startColor = data.plasmaColor;
                trailRenderer.endColor = new Color(data.plasmaColor.r, data.plasmaColor.g, data.plasmaColor.b, 0);
                trailRenderer.time = data.trailDuration;
            }

            // 启动销毁计时
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            if (!isInitialized) return;

            // 移动
            float moveDistance = speed * Time.deltaTime;
            transform.position += (Vector3)(direction * moveDistance);
            traveledDistance += moveDistance;
            
            // 检查最大距离
            if (traveledDistance >= maxDistance)
            {
                DestroyProjectile();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isInitialized || ownerWeapon == null) return;
            
            // 避免重复命中同一目标
            if (hitTargets.Contains(other.gameObject)) return;

            // 记录命中
            hitTargets.Add(other.gameObject);

            // 计算命中点
            Vector2 hitPosition = other.ClosestPoint(transform.position);
            Vector2 hitDirection = direction;

            // 通知武器处理命中
            ownerWeapon.OnPlasmaHit(other.gameObject, hitPosition, hitDirection, pierceCount);
            
            // 增加穿透计数
            pierceCount++;
            
            // 穿透效果 - 继续飞行
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!isInitialized) return;
            
            // 等离子球穿透墙壁，但会损失能量
            pierceCount += 2; // 墙壁消耗更多穿透力
        }

        /// <summary>
        /// 销毁弹丸
        /// </summary>
        private void DestroyProjectile()
        {
            // 停止拖尾并让它自然消失
            if (trailRenderer != null)
            {
                trailRenderer.transform.SetParent(null);
                trailRenderer.autodestruct = true;
            }

            Destroy(gameObject);
        }
    }
}