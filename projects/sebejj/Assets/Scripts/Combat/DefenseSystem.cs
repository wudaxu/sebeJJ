using UnityEngine;
using System;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 护盾/装甲管理器 - 整合护盾和装甲系统
    /// BUG-006修复: 修复格挡判定有时失效
    /// </summary>
    public class DefenseSystem : MonoBehaviour
    {
        [Header("组件引用")]
        [SerializeField] private ShieldSystem shieldSystem;
        [SerializeField] private ArmorSystem armorSystem;
        [SerializeField] private CombatStats combatStats;

        [Header("格挡设置")]
        [SerializeField] private float blockInputBuffer = 0.15f; // BUG-006修复: 输入缓冲时间
        [SerializeField] private float blockCooldown = 0.2f;
        [SerializeField] private float staminaCostPerBlock = 10f;
        
        [Header("格挡特效")]
        [SerializeField] private GameObject blockEffectPrefab;
        [SerializeField] private AudioClip blockSound;
        
        // BUG-006修复: 格挡状态
        private bool isBlocking = false;
        private bool canBlock = true;
        private float lastBlockInputTime;
        private float lastBlockTime;
        private float currentStamina = 100f;
        private float maxStamina = 100f;

        // 事件
        public event Action OnDefenseBroken;  // 所有防御都被打破
        public event Action OnBlockStarted;   // BUG-006修复: 格挡开始事件
        public event Action OnBlockEnded;     // BUG-006修复: 格挡结束事件
        public event Action OnBlockSuccess;   // BUG-006修复: 成功格挡事件

        // 属性
        public ShieldSystem Shield => shieldSystem;
        public ArmorSystem Armor => armorSystem;
        public bool HasAnyDefense => (shieldSystem != null && shieldSystem.HasShield) || 
                                      (armorSystem != null && armorSystem.HasArmor);
        public bool IsBlocking => isBlocking;  // BUG-006修复: 公开格挡状态
        public bool CanBlock => canBlock && currentStamina >= staminaCostPerBlock;

        private void Awake()
        {
            // 自动获取组件
            if (shieldSystem == null) shieldSystem = GetComponent<ShieldSystem>();
            if (armorSystem == null) armorSystem = GetComponent<ArmorSystem>();
            if (combatStats == null) combatStats = GetComponent<CombatStats>();

            // 订阅事件
            if (shieldSystem != null)
            {
                shieldSystem.OnShieldDepleted += OnShieldBroken;
            }
            if (armorSystem != null)
            {
                armorSystem.OnArmorDestroyed += OnArmorDestroyed;
            }
        }
        
        private void Update()
        {
            // BUG-006修复: 处理格挡输入缓冲
            HandleBlockInput();
            
            // BUG-006修复: 恢复耐力
            RecoverStamina();
            
            // BUG-006修复: 更新格挡冷却
            UpdateBlockCooldown();
        }
        
        /// <summary>
        /// BUG-006修复: 处理格挡输入
        /// </summary>
        private void HandleBlockInput()
        {
            // 检测格挡输入
            if (Input.GetButtonDown("Block") || Input.GetKeyDown(KeyCode.Mouse1))
            {
                lastBlockInputTime = Time.time;
            }
            
            // 在缓冲时间内都可以触发格挡
            if (Time.time - lastBlockInputTime <= blockInputBuffer)
            {
                if (CanBlock && !isBlocking)
                {
                    StartBlocking();
                    lastBlockInputTime = -999f; // 消耗输入
                }
            }
            
            // 检测格挡释放
            if (isBlocking && (Input.GetButtonUp("Block") || Input.GetKeyUp(KeyCode.Mouse1)))
            {
                StopBlocking();
            }
        }
        
        /// <summary>
        /// BUG-006修复: 开始格挡
        /// </summary>
        public void StartBlocking()
        {
            if (!CanBlock || isBlocking) return;
            
            isBlocking = true;
            lastBlockTime = Time.time;
            
            // 消耗耐力
            currentStamina -= staminaCostPerBlock;
            
            OnBlockStarted?.Invoke();
            
            Debug.Log("[DefenseSystem] 开始格挡");
        }
        
        /// <summary>
        /// BUG-006修复: 停止格挡
        /// </summary>
        public void StopBlocking()
        {
            if (!isBlocking) return;
            
            isBlocking = false;
            canBlock = false; // 进入冷却
            
            OnBlockEnded?.Invoke();
            
            Debug.Log("[DefenseSystem] 停止格挡");
        }
        
        /// <summary>
        /// BUG-006修复: 尝试格挡伤害
        /// </summary>
        public bool TryBlockDamage(float damage, Vector2 attackDirection)
        {
            if (!isBlocking) return false;
            
            // 检查格挡角度
            float blockAngle = 120f; // 格挡角度范围
            Vector2 blockDirection = transform.right;
            float angle = Vector2.Angle(blockDirection, attackDirection);
            
            if (angle <= blockAngle / 2f)
            {
                // 成功格挡
                OnBlockSuccess?.Invoke();
                
                // 播放格挡特效
                PlayBlockEffect();
                
                Debug.Log("[DefenseSystem] 成功格挡伤害");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// BUG-006修复: 播放格挡特效
        /// </summary>
        private void PlayBlockEffect()
        {
            // 实例化格挡特效
            if (blockEffectPrefab != null)
            {
                GameObject effect = Instantiate(blockEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 1f);
            }
            
            // 播放音效
            if (blockSound != null)
            {
                AudioManager.Instance?.PlaySFX(blockSound);
            }
            
            // 屏幕震动
            CameraShake?.Invoke(0.1f, 0.1f);
        }
        
        // 相机震动事件
        public static System.Action<float, float> CameraShake;
        
        /// <summary>
        /// BUG-006修复: 恢复耐力
        /// </summary>
        private void RecoverStamina()
        {
            if (currentStamina < maxStamina)
            {
                currentStamina += 20f * Time.deltaTime; // 每秒恢复20点
                currentStamina = Mathf.Min(currentStamina, maxStamina);
            }
        }
        
        /// <summary>
        /// BUG-006修复: 更新格挡冷却
        /// </summary>
        private void UpdateBlockCooldown()
        {
            if (!canBlock && Time.time - lastBlockTime >= blockCooldown)
            {
                canBlock = true;
            }
        }

        private void OnDestroy()
        {
            if (shieldSystem != null)
            {
                shieldSystem.OnShieldDepleted -= OnShieldBroken;
            }
            if (armorSystem != null)
            {
                armorSystem.OnArmorDestroyed -= OnArmorDestroyed;
            }
        }

        /// <summary>
        /// 计算最终伤害(经过护盾和装甲减免)
        /// </summary>
        public float CalculateFinalDamage(float baseDamage)
        {
            float damage = baseDamage;

            // 1. 先经过护盾减免
            if (shieldSystem != null && shieldSystem.HasShield)
            {
                damage = shieldSystem.TakeShieldDamage(damage);
            }

            // 2. 再经过装甲减免
            if (armorSystem != null && armorSystem.HasArmor)
            {
                damage = armorSystem.CalculateArmorMitigation(damage);
            }

            return damage;
        }

        /// <summary>
        /// 直接对护盾造成伤害
        /// </summary>
        public void DamageShield(float amount)
        {
            shieldSystem?.TakeShieldDamage(amount);
        }

        /// <summary>
        /// 直接对装甲造成伤害
        /// </summary>
        public void DamageArmor(float amount)
        {
            armorSystem?.TakeArmorDamage(amount);
        }

        /// <summary>
        /// 恢复护盾
        /// </summary>
        public void RestoreShield(float amount)
        {
            shieldSystem?.RestoreShield(amount);
        }

        /// <summary>
        /// 修复装甲
        /// </summary>
        public void RepairArmor(float amount)
        {
            armorSystem?.RepairArmor(amount);
        }

        /// <summary>
        /// 完全恢复所有防御
        /// </summary>
        public void FullRestore()
        {
            if (shieldSystem != null)
            {
                shieldSystem.RestoreShield(float.MaxValue);
            }
            if (armorSystem != null)
            {
                armorSystem.RepairArmor(float.MaxValue);
            }
        }

        private void OnShieldBroken()
        {
            CheckAllDefenseBroken();
        }

        private void OnArmorDestroyed()
        {
            CheckAllDefenseBroken();
        }

        private void CheckAllDefenseBroken()
        {
            bool shieldBroken = shieldSystem == null || !shieldSystem.HasShield;
            bool armorBroken = armorSystem == null || !armorSystem.HasArmor;

            if (shieldBroken && armorBroken)
            {
                OnDefenseBroken?.Invoke();
            }
        }
    }
}