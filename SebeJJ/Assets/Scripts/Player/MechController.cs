using UnityEngine;

namespace SebeJJ.Player
{
    /// <summary>
    /// 机甲控制器 - 处理玩家机甲的移动和输入
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class MechController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 180f;
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float deceleration = 5f;

        [Header("Buoyancy")]
        [SerializeField] private float buoyancyForce = 2f;
        [SerializeField] private float buoyancyVariation = 0.5f;
        [SerializeField] private float buoyancyCycle = 3f;

        [Header("Depth")]
        [SerializeField] private float depthMultiplier = 1f;
        [SerializeField] private float maxDepth = 1000f;
        [SerializeField] private float surfaceY = 0f;

        [Header("Energy")]
        [SerializeField] private float moveEnergyCost = 0.1f;
        [SerializeField] private float boostEnergyCost = 0.5f;
        [SerializeField] private float boostMultiplier = 2f;

        [Header("References")]
        [SerializeField] private Transform visualTransform;
        [SerializeField] private ParticleSystem moveParticles;
        [SerializeField] private ParticleSystem boostParticles;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        // 组件引用
        private Rigidbody2D _rb;
        private MechStats _stats;
        private MechStatus _status;

        // 输入
        private Vector2 _movementInput;
        private float _rotationInput;
        private bool _isBoosting;
        private bool _isMovementLocked;

        // 状态
        private float _currentSpeed;
        private float _currentDepth;
        private float _buoyancyOffset;
        private Vector2 _lastPosition;

        // 属性
        public float CurrentDepth => _currentDepth;
        public float CurrentSpeed => _currentSpeed;
        public bool IsMoving => _movementInput.magnitude > 0.1f;
        public bool IsBoosting => _isBoosting;
        public Rigidbody2D Rigidbody => _rb;

        #region Unity Lifecycle

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.drag = 2f;
            _rb.angularDrag = 2f;

            _stats = GetComponent<MechStats>();
            _status = GetComponent<MechStatus>();

            if (visualTransform == null)
            {
                visualTransform = transform;
            }
        }

        private void Start()
        {
            _lastPosition = transform.position;
        }

        private void Update()
        {
            HandleInput();
            UpdateDepth();
            UpdateBuoyancy();
            UpdateVisuals();
        }

        private void FixedUpdate()
        {
            if (!_isMovementLocked)
            {
                ApplyMovement();
                ApplyBuoyancy();
            }
        }

        #endregion

        #region Input Handling

        private void HandleInput()
        {
            // 键盘/手柄输入
            _movementInput = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            ).normalized;

            // 旋转输入（如果有机甲朝向控制）
            _rotationInput = Input.GetAxisRaw("Rotate");

            // 冲刺输入
            _isBoosting = Input.GetButton("Boost") && _status != null && _status.CurrentEnergy > 0;

            // 锁定移动（例如采集时）
            if (Input.GetButtonDown("LockMovement"))
            {
                ToggleMovementLock();
            }
        }

        public void SetMovementInput(Vector2 input)
        {
            _movementInput = input.normalized;
        }

        public void SetBoosting(bool boosting)
        {
            _isBoosting = boosting && _status != null && _status.CurrentEnergy > 0;
        }

        public void ToggleMovementLock()
        {
            _isMovementLocked = !_isMovementLocked;
        }

        public void LockMovement()
        {
            _isMovementLocked = true;
        }

        public void UnlockMovement()
        {
            _isMovementLocked = false;
        }

        #endregion

        #region Movement

        private void ApplyMovement()
        {
            if (_rb == null) return;

            // 计算目标速度
            float targetSpeed = moveSpeed;
            
            if (_stats != null)
            {
                targetSpeed = _stats.GetModifiedSpeed(moveSpeed);
            }

            // 冲刺加成
            if (_isBoosting)
            {
                targetSpeed *= boostMultiplier;
                _status?.ConsumeEnergy(boostEnergyCost * Time.fixedDeltaTime);
                
                if (boostParticles != null && !boostParticles.isPlaying)
                {
                    boostParticles.Play();
                }
            }
            else
            {
                if (boostParticles != null && boostParticles.isPlaying)
                {
                    boostParticles.Stop();
                }
            }

            // 应用移动
            Vector2 targetVelocity = _movementInput * targetSpeed;
            _rb.velocity = Vector2.Lerp(_rb.velocity, targetVelocity, acceleration * Time.fixedDeltaTime);

            // 旋转（如果有旋转输入）
            if (Mathf.Abs(_rotationInput) > 0.1f)
            {
                _rb.angularVelocity = -_rotationInput * rotationSpeed;
            }
            else
            {
                _rb.angularVelocity = Mathf.Lerp(_rb.angularVelocity, 0, deceleration * Time.fixedDeltaTime);
            }

            // 消耗移动能量
            if (IsMoving && _status != null)
            {
                _status.ConsumeEnergy(moveEnergyCost * Time.fixedDeltaTime);
            }

            // 计算当前速度
            _currentSpeed = _rb.velocity.magnitude;

            // 粒子效果
            UpdateMoveParticles();
        }

        private void UpdateMoveParticles()
        {
            if (moveParticles == null) return;

            if (IsMoving && _currentSpeed > 0.5f)
            {
                if (!moveParticles.isPlaying)
                {
                    moveParticles.Play();
                }
                
                // 根据速度调整粒子发射速率
                var emission = moveParticles.emission;
                emission.rateOverTime = _currentSpeed * 10f;
            }
            else
            {
                if (moveParticles.isPlaying)
                {
                    moveParticles.Stop();
                }
            }
        }

        #endregion

        #region Buoyancy

        private void UpdateBuoyancy()
        {
            // 模拟浮力变化
            _buoyancyOffset = Mathf.Sin(Time.time / buoyancyCycle * Mathf.PI * 2) * buoyancyVariation;
        }

        private void ApplyBuoyancy()
        {
            if (_rb == null) return;

            // 基础浮力 + 变化
            float currentBuoyancy = buoyancyForce + _buoyancyOffset;
            
            // 深度越深，浮力越小（压强影响）
            float depthFactor = 1f - (_currentDepth / maxDepth * 0.3f);
            currentBuoyancy *= depthFactor;

            // 应用浮力
            _rb.AddForce(Vector2.up * currentBuoyancy, ForceMode2D.Force);
        }

        #endregion

        #region Depth

        private void UpdateDepth()
        {
            // 根据Y坐标计算深度
            _currentDepth = Mathf.Max(0, (surfaceY - transform.position.y) * depthMultiplier);

            // 触发深度变化事件
            if (Mathf.Abs(_currentDepth - (surfaceY - _lastPosition.y) * depthMultiplier) > 0.1f)
            {
                Core.GameEvents.OnDepthChanged?.Invoke(_currentDepth);
            }
        }

        public void SetSurfaceY(float y)
        {
            surfaceY = y;
        }

        #endregion

        #region Visuals

        private void UpdateVisuals()
        {
            if (visualTransform == null) return;

            // 根据移动方向倾斜视觉
            if (IsMoving)
            {
                float tiltAngle = _movementInput.x * 15f;
                float targetRotation = Mathf.Atan2(_movementInput.y, _movementInput.x) * Mathf.Rad2Deg - 90f;
                
                // 平滑旋转
                Quaternion targetRot = Quaternion.Euler(0, 0, targetRotation + tiltAngle);
                visualTransform.rotation = Quaternion.Lerp(visualTransform.rotation, targetRot, 10f * Time.deltaTime);
            }
        }

        #endregion

        #region Public Methods

        public void TeleportTo(Vector2 position)
        {
            _rb.position = position;
            _rb.velocity = Vector2.zero;
            _rb.angularVelocity = 0f;
        }

        public void ApplyForce(Vector2 force, ForceMode2D mode = ForceMode2D.Impulse)
        {
            _rb.AddForce(force, mode);
        }

        public void ApplyTorque(float torque)
        {
            _rb.AddTorque(torque);
        }

        public void Stop()
        {
            _rb.velocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            _movementInput = Vector2.zero;
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            if (!showDebugInfo) return;

            // 绘制移动方向
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, _movementInput * 2f);

            // 绘制速度向量
            if (Application.isPlaying && _rb != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, _rb.velocity);
            }
        }

        #endregion
    }
}
