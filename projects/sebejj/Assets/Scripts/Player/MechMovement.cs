using UnityEngine;
using System.Collections;

namespace SebeJJ.Player
{
    /// <summary>
    /// 机甲移动控制器 - 处理推进、惯性、悬浮等物理效果
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class MechMovement : MonoBehaviour
    {
        [Header("基础移动")]
        public float maxSpeed = 8f;
        public float acceleration = 15f;
        public float deceleration = 20f;
        public float rotationSpeed = 360f;
        
        [Header("推进器")]
        public float boostMultiplier = 2f;
        public float boostDuration = 1f;
        public float boostCooldown = 3f;
        public float boostEnergyCost = 20f;
        
        [Header("悬浮")]
        public float hoverForce = 5f;
        public float hoverDamping = 2f;
        public float targetHoverHeight = 0f;
        
        [Header("物理")]
        public float waterDrag = 3f;
        public float waterAngularDrag = 5f;
        public float mass = 1f;
        
        [Header("效果")]
        public ParticleSystem thrustParticles;
        public ParticleSystem boostParticles;
        public AudioSource engineSound;
        
        // 组件
        private Rigidbody2D rb;
        private Vector2 moveInput;
        private float currentSpeed;
        private bool isBoosting;
        private bool canBoost = true;
        
        // 属性
        public float CurrentSpeed => rb.velocity.magnitude;
        public bool IsMoving => rb.velocity.magnitude > 0.5f;
        public bool IsBoosting => isBoosting;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.drag = waterDrag;
            rb.angularDrag = waterAngularDrag;
            rb.mass = mass;
            
            // BUG-001 修复: 设置连续碰撞检测防止穿墙
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            
            // CO-004 优化: 设置合理的最大速度限制
            rb.velocity = Vector2.zero;
        }
        
        private void Update()
        {
            HandleInput();
            HandleRotation();
            UpdateEffects();
        }
        
        private void FixedUpdate()
        {
            HandleMovement();
            ApplyHover();
        }
        
        private void HandleInput()
        {
            // 移动输入
            moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            moveInput = Vector2.ClampMagnitude(moveInput, 1f);
            
            // 推进器加速
            if (Input.GetKey(KeyCode.LeftShift) && canBoost && !isBoosting)
            {
                StartBoost();
            }
        }
        
        private void HandleRotation()
        {
            if (moveInput.magnitude > 0.1f)
            {
                // 计算目标角度
                float targetAngle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
                float currentAngle = transform.eulerAngles.z;
                
                // 平滑旋转
                float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
            }
        }
        
        private void HandleMovement()
        {
            float targetSpeed = maxSpeed;
            
            // 推进器加速
            if (isBoosting)
            {
                targetSpeed *= boostMultiplier;
            }
            
            // 计算目标速度
            Vector2 targetVelocity = moveInput * targetSpeed;
            
            // 应用加速度
            if (moveInput.magnitude > 0.1f)
            {
                rb.AddForce(moveInput * acceleration * rb.mass, ForceMode2D.Force);
                
                // 限制最大速度
                if (rb.velocity.magnitude > targetSpeed)
                {
                    rb.velocity = rb.velocity.normalized * targetSpeed;
                }
            }
            else
            {
                // 自然减速
                rb.velocity = Vector2.MoveTowards(rb.velocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            }
            
            // 消耗能源
            if (moveInput.magnitude > 0.1f)
            {
                float energyCost = isBoosting ? 0.5f : 0.1f;
                Core.GameManager.Instance?.resourceManager?.ConsumeEnergy(energyCost * Time.fixedDeltaTime);
            }
        }
        
        /// <summary>
        /// 应用悬浮力
        /// </summary>
        private void ApplyHover()
        {
            // 简单的悬浮阻尼效果
            Vector2 verticalVelocity = Vector2.Project(rb.velocity, Vector2.up);
            rb.AddForce(-verticalVelocity * hoverDamping, ForceMode2D.Force);
        }
        
        /// <summary>
        /// 启动推进器
        /// </summary>
        private void StartBoost()
        {
            var resourceManager = Core.GameManager.Instance?.resourceManager;
            if (resourceManager == null) return;
            
            if (!resourceManager.ConsumeEnergy(boostEnergyCost))
            {
                Core.UIManager.Instance?.ShowNotification("能源不足！");
                return;
            }
            
            isBoosting = true;
            canBoost = false;
            
            // 播放特效
            if (boostParticles != null)
                boostParticles.Play();
            
            Utils.AudioManager.Instance?.PlaySFX(
                Utils.AudioManager.Instance?.GetClip("boost"));
            
            // 推进器冷却
            Invoke(nameof(EndBoost), boostDuration);
            Invoke(nameof(ResetBoost), boostCooldown);
            
            Debug.Log("[MechMovement] 推进器启动");
        }
        
        private void EndBoost()
        {
            isBoosting = false;
            
            if (boostParticles != null)
                boostParticles.Stop();
        }
        
        private void ResetBoost()
        {
            canBoost = true;
            Core.UIManager.Instance?.ShowNotification("推进器就绪");
        }
        
        /// <summary>
        /// 更新视觉效果
        /// </summary>
        private void UpdateEffects()
        {
            // 推进器粒子
            if (thrustParticles != null)
            {
                if (moveInput.magnitude > 0.1f)
                {
                    if (!thrustParticles.isPlaying)
                        thrustParticles.Play();
                    
                    // 根据速度调整粒子
                    var emission = thrustParticles.emission;
                    emission.rateOverTime = 50f + CurrentSpeed * 10f;
                }
                else
                {
                    if (thrustParticles.isPlaying)
                        thrustParticles.Stop();
                }
            }
            
            // 引擎音效
            if (engineSound != null)
            {
                engineSound.pitch = 0.8f + (CurrentSpeed / maxSpeed) * 0.4f;
                engineSound.volume = 0.3f + (CurrentSpeed / maxSpeed) * 0.3f;
            }
        }
        
        /// <summary>
        /// 应用冲击力（受击等）
        /// </summary>
        public void ApplyImpulse(Vector2 direction, float force)
        {
            rb.AddForce(direction * force, ForceMode2D.Impulse);
        }
        
        /// <summary>
        /// 瞬间移动（传送等）
        /// </summary>
        public void Teleport(Vector2 position)
        {
            rb.velocity = Vector2.zero;
            transform.position = position;
        }
        
        /// <summary>
        /// 停止移动
        /// </summary>
        public void Stop()
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        /// <summary>
        /// 获取移动方向
        /// </summary>
        public Vector2 GetMoveDirection()
        {
            return rb.velocity.normalized;
        }
        
        /// <summary>
        /// 设置移动速度（用于外部控制）
        /// </summary>
        public void SetMoveSpeed(float speed)
        {
            maxSpeed = speed;
        }
        
        /// <summary>
        /// 获取当前速度
        /// </summary>
        public float GetCurrentSpeed()
        {
            return rb.velocity.magnitude;
        }
        
        private void OnDrawGizmosSelected()
        {
            // 绘制速度向量
            if (Application.isPlaying && rb != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, transform.position + (Vector3)rb.velocity);
            }
        }
    }
}
