/**
 * @file BossArena.cs
 * @brief Boss战场景管理器
 * @description 管理Boss战场景、可破坏物体、即死区域和胜利传送门
 * @author Boss战设计师
 * @date 2026-02-27
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SebeJJ.Utils;

namespace SebeJJ.Boss
{
    /// <summary>
    /// Boss战场景管理器
    /// </summary>
    public class BossArena : MonoBehaviour
    {
        #region 序列化字段

        [Header("=== 场景边界 ===")]
        [SerializeField] private Collider2D arenaBounds;
        [SerializeField] private float arenaWidth = 40f;
        [SerializeField] private float arenaHeight = 25f;
        
        [Header("=== 即死深渊 ===")]
        [SerializeField] private Transform abyssTrigger;
        [SerializeField] private float abyssDamage = 9999f;
        [SerializeField] private LayerMask abyssAffectedLayers;
        [SerializeField] private ParticleSystem abyssEffect;
        
        [Header("=== 可破坏岩石 ===")]
        [SerializeField] private List<DestructibleRock> destructibleRocks = new List<DestructibleRock>();
        [SerializeField] private GameObject rockPrefab;
        [SerializeField] private int rockCount = 6;
        [SerializeField] private float rockMinDistance = 5f;
        
        [Header("=== 胜利传送门 ===")]
        [SerializeField] private VictoryPortal victoryPortal;
        [SerializeField] private Transform portalSpawnPoint;
        [SerializeField] private float portalActivateDelay = 3f;
        
        [Header("=== Boss引用 ===")]
        [SerializeField] private IronClawBeastBoss boss;
        [SerializeField] private Transform bossSpawnPoint;
        
        [Header("=== 环境特效 ===")]
        [SerializeField] private ParticleSystem ambientDust;
        [SerializeField] private ParticleSystem ambientSparks;
        [SerializeField] private Light sceneLight;
        
        [Header("=== 阶段环境变化 ===")]
        [SerializeField] private Color phase1LightColor = new Color(0.8f, 0.9f, 1f);
        [SerializeField] private Color phase2LightColor = new Color(1f, 0.8f, 0.6f);
        [SerializeField] private Color phase3LightColor = new Color(1f, 0.4f, 0.3f);
        [SerializeField] private float lightTransitionDuration = 2f;

        #endregion

        #region 私有字段

        private bool _isBossFightActive = false;
        private List<GameObject> _spawnedRocks = new List<GameObject>();
        private Camera _mainCamera;

        #endregion

        #region 公共属性

        public bool IsBossFightActive => _isBossFightActive;
        public Bounds ArenaBounds => arenaBounds != null ? arenaBounds.bounds : 
            new Bounds(transform.position, new Vector3(arenaWidth, arenaHeight, 1f));

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            _mainCamera = Camera.main;
            
            // 如果没有设置边界碰撞器，创建一个
            if (arenaBounds == null)
            {
                CreateArenaBounds();
            }
            
            // 初始化场景
            InitializeArena();
        }

        private void Start()
        {
            // 订阅Boss事件
            if (boss != null)
            {
                boss.OnPhaseChanged += OnBossPhaseChanged;
                boss.OnDefeated += OnBossDefeated;
            }
            
            // 开始Boss战
            StartBossFight();
        }

        private void Update()
        {
            // 检查即死深渊
            CheckAbyss();
            
            // 检查Boss是否离开场景
            if (_isBossFightActive && boss != null && !boss.IsDead)
            {
                KeepBossInArena();
            }
        }

        private void OnDestroy()
        {
            if (boss != null)
            {
                boss.OnPhaseChanged -= OnBossPhaseChanged;
                boss.OnDefeated -= OnBossDefeated;
            }
        }

        #endregion

        #region 初始化

        private void CreateArenaBounds()
        {
            GameObject boundsObj = new GameObject("ArenaBounds");
            boundsObj.transform.SetParent(transform);
            boundsObj.transform.localPosition = Vector3.zero;
            
            var boxCollider = boundsObj.AddComponent<BoxCollider2D>();
            boxCollider.size = new Vector2(arenaWidth, arenaHeight);
            boxCollider.isTrigger = true;
            
            arenaBounds = boxCollider;
        }

        private void InitializeArena()
        {
            // 生成可破坏岩石
            SpawnDestructibleRocks();
            
            // 初始化环境特效
            if (ambientDust != null) ambientDust.Play();
            if (ambientSparks != null) ambientSparks.Play();
            
            // 设置初始光照
            if (sceneLight != null)
            {
                sceneLight.color = phase1LightColor;
            }
            
            // 初始化传送门（隐藏）
            if (victoryPortal != null)
            {
                victoryPortal.gameObject.SetActive(false);
            }
        }

        private void SpawnDestructibleRocks()
        {
            if (rockPrefab == null) return;
            
            for (int i = 0; i < rockCount; i++)
            {
                Vector3 spawnPos = GetRandomRockPosition();
                
                if (spawnPos != Vector3.zero)
                {
                    GameObject rock = Instantiate(rockPrefab, spawnPos, Quaternion.identity);
                    rock.transform.SetParent(transform);
                    
                    var destructibleRock = rock.GetComponent<DestructibleRock>();
                    if (destructibleRock == null)
                    {
                        destructibleRock = rock.AddComponent<DestructibleRock>();
                    }
                    
                    destructibleRocks.Add(destructibleRock);
                    _spawnedRocks.Add(rock);
                }
            }
        }

        private Vector3 GetRandomRockPosition()
        {
            int maxAttempts = 50;
            
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                float x = Random.Range(-arenaWidth / 2f + 2f, arenaWidth / 2f - 2f);
                float y = Random.Range(-arenaHeight / 2f + 2f, arenaHeight / 2f - 2f);
                Vector3 pos = transform.position + new Vector3(x, y, 0);
                
                // 检查与其他岩石的距离
                bool tooClose = false;
                foreach (var rock in _spawnedRocks)
                {
                    if (Vector3.Distance(pos, rock.transform.position) < rockMinDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }
                
                // 检查与Boss生成点的距离
                if (bossSpawnPoint != null && 
                    Vector3.Distance(pos, bossSpawnPoint.position) < 8f)
                {
                    tooClose = true;
                }
                
                if (!tooClose)
                {
                    return pos;
                }
            }
            
            return Vector3.zero;
        }

        #endregion

        #region Boss战管理

        public void StartBossFight()
        {
            _isBossFightActive = true;
            
            // 生成Boss
            if (boss == null && bossSpawnPoint != null && bossPrefab != null)
            {
                boss = Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);
                
                // 初始化Boss
                var bossComponent = boss.GetComponent<IronClawBeastBoss>();
                if (bossComponent != null)
                {
                    bossComponent.OnDefeated += OnBossDefeated;
                }
            }
            
            // 关闭入口，防止逃跑
            CloseArenaEntrance();
            
            // 播放Boss战开始事件
            GameEvents.TriggerNotification("Boss战开始！");
            
            // 播放战斗音乐
            // AudioManager.Instance?.PlayMusic(bossBattleMusic);
        }

        private void CloseArenaEntrance()
        {
            // 创建屏障或关闭门
            if (entranceBarrier != null)
            {
                entranceBarrier.SetActive(true);
                
                // 播放关闭动画
                Animator barrierAnimator = entranceBarrier.GetComponent<Animator>();
                if (barrierAnimator != null)
                {
                    barrierAnimator.SetTrigger("Close");
                }
                
                // 播放音效
                AudioManager.Instance?.PlaySFX("barrier_close");
            }
            
            // 创建视觉屏障
            if (entranceBarrierEffect != null)
            {
                entranceBarrierEffect.SetActive(true);
            }
            
            Debug.Log("[BossArena] 竞技场入口已关闭");
        }

        private void KeepBossInArena()
        {
            if (arenaBounds == null) return;
            
            Vector3 bossPos = boss.transform.position;
            Bounds bounds = arenaBounds.bounds;
            
            // 检查是否超出边界
            bool outOfBounds = false;
            Vector3 newPos = bossPos;
            
            if (bossPos.x < bounds.min.x)
            {
                newPos.x = bounds.min.x;
                outOfBounds = true;
            }
            else if (bossPos.x > bounds.max.x)
            {
                newPos.x = bounds.max.x;
                outOfBounds = true;
            }
            
            if (bossPos.y < bounds.min.y)
            {
                newPos.y = bounds.min.y;
                outOfBounds = true;
            }
            else if (bossPos.y > bounds.max.y)
            {
                newPos.y = bounds.max.y;
                outOfBounds = true;
            }
            
            if (outOfBounds)
            {
                boss.transform.position = newPos;
            }
        }

        #endregion

        #region 即死深渊

        private void CheckAbyss()
        {
            if (abyssTrigger == null) return;
            
            // 检测进入深渊区域的对象
            Collider2D[] colliders = Physics2D.OverlapBoxAll(
                abyssTrigger.position, 
                new Vector2(arenaWidth, 2f), 
                0f, 
                abyssAffectedLayers);
            
            foreach (var collider in colliders)
            {
                var damageable = collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(abyssDamage, null);
                    
                    // 播放深渊特效
                    if (abyssEffect != null)
                    {
                        abyssEffect.transform.position = collider.transform.position;
                        abyssEffect.Play();
                    }
                }
            }
        }

        #endregion

        #region 事件处理

        private void OnBossPhaseChanged(BossPhase newPhase)
        {
            // 改变场景环境
            StartCoroutine(TransitionEnvironment(newPhase));
        }

        private void OnBossDefeated()
        {
            _isBossFightActive = false;
            
            // 延迟激活传送门
            StartCoroutine(ActivatePortalCoroutine());
            
            // 打开出口
            OpenArenaExit();
        }

        private System.Collections.IEnumerator ActivatePortalCoroutine()
        {
            yield return new WaitForSeconds(portalActivateDelay);
            
            if (victoryPortal != null)
            {
                victoryPortal.gameObject.SetActive(true);
                
                if (portalSpawnPoint != null)
                {
                    victoryPortal.transform.position = portalSpawnPoint.position;
                }
                
                victoryPortal.Activate();
            }
        }

        private void OpenArenaExit()
        {
            // 打开出口，允许玩家离开
            if (exitBarrier != null)
            {
                exitBarrier.SetActive(false);
                
                // 播放打开动画
                Animator barrierAnimator = exitBarrier.GetComponent<Animator>();
                if (barrierAnimator != null)
                {
                    barrierAnimator.SetTrigger("Open");
                }
            }
            
            // 禁用视觉屏障
            if (exitBarrierEffect != null)
            {
                exitBarrierEffect.SetActive(false);
            }
            
            // 显示出口开启提示
            UINotification.Instance?.ShowNotification("出口已开启！", NotificationType.Success);
            
            Debug.Log("[BossArena] 竞技场出口已打开");
        }
        }

        #endregion

        #region 环境变化

        private System.Collections.IEnumerator TransitionEnvironment(BossPhase phase)
        {
            Color targetColor;
            
            switch (phase)
            {
                case BossPhase.Phase2:
                    targetColor = phase2LightColor;
                    break;
                case BossPhase.Phase3:
                    targetColor = phase3LightColor;
                    break;
                default:
                    yield break;
            }
            
            if (sceneLight == null) yield break;
            
            Color startColor = sceneLight.color;
            float timer = 0f;
            
            while (timer < lightTransitionDuration)
            {
                sceneLight.color = Color.Lerp(startColor, targetColor, timer / lightTransitionDuration);
                timer += Time.deltaTime;
                yield return null;
            }
            
            sceneLight.color = targetColor;
        }

        #endregion

        #region 调试

        private void OnDrawGizmos()
        {
            // 绘制场景边界
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(arenaWidth, arenaHeight, 1f));
            
            // 绘制即死深渊
            if (abyssTrigger != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(abyssTrigger.position, new Vector3(arenaWidth, 2f, 1f));
            }
            
            // 绘制Boss生成点
            if (bossSpawnPoint != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(bossSpawnPoint.position, 0.5f);
            }
            
            // 绘制传送门生成点
            if (portalSpawnPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(portalSpawnPoint.position, 0.5f);
            }
        }

        #endregion
    }

    /// <summary>
    /// 可破坏岩石
    /// </summary>
    public class DestructibleRock : MonoBehaviour, IDamageable
    {
        [Header("=== 岩石属性 ===")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float explosionForce = 500f;
        [SerializeField] private float explosionRadius = 3f;
        
        [Header("=== 特效 ===")]
        [SerializeField] private ParticleSystem destroyEffect;
        [SerializeField] private ParticleSystem damageEffect;
        [SerializeField] private AudioClip destroySound;
        [SerializeField] private AudioClip damageSound;
        
        [Header("=== 物理 ===")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Collider2D rockCollider;
        
        [Header("=== 碎片 ===")]
        [SerializeField] private GameObject debrisPrefab;
        [SerializeField] private int debrisCount = 5;

        private float _currentHealth;
        private bool _isDestroyed = false;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead => _isDestroyed;

        private void Awake()
        {
            _currentHealth = maxHealth;
            
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (rockCollider == null) rockCollider = GetComponent<Collider2D>();
        }

        public void TakeDamage(float damage, Transform damageSource = null)
        {
            if (_isDestroyed) return;
            
            _currentHealth -= damage;
            
            // 播放受击特效
            damageEffect?.Play();
            
            if (damageSound != null)
            {
                AudioManager.Instance?.PlaySFX(damageSound);
            }
            
            // 震动效果
            if (rb != null)
            {
                rb.AddForce(Random.insideUnitCircle * 50f);
            }
            
            if (_currentHealth <= 0)
            {
                DestroyRock();
            }
        }

        private void DestroyRock()
        {
            _isDestroyed = true;
            
            // 播放破坏特效
            destroyEffect?.Play();
            
            if (destroySound != null)
            {
                AudioManager.Instance?.PlaySFX(destroySound);
            }
            
            // 生成碎片
            SpawnDebris();
            
            // 对周围造成伤害
            Explode();
            
            // 隐藏并延迟销毁
            GetComponent<SpriteRenderer>()?.gameObject.SetActive(false);
            rockCollider.enabled = false;
            
            Destroy(gameObject, 2f);
        }

        private void SpawnDebris()
        {
            if (debrisPrefab == null) return;
            
            for (int i = 0; i < debrisCount; i++)
            {
                Vector3 offset = Random.insideUnitSphere * 0.5f;
                GameObject debris = Instantiate(debrisPrefab, transform.position + offset, Random.rotation);
                
                var debrisRb = debris.GetComponent<Rigidbody2D>();
                if (debrisRb != null)
                {
                    Vector2 force = Random.insideUnitCircle * explosionForce;
                    debrisRb.AddForce(force);
                    debrisRb.AddTorque(Random.Range(-100f, 100f));
                }
                
                Destroy(debris, 5f);
            }
        }

        private void Explode()
        {
            // 对爆炸范围内的对象造成伤害
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
            
            foreach (var collider in colliders)
            {
                if (collider.gameObject == gameObject) continue;
                
                var damageable = collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    float damage = 30f;
                    damageable.TakeDamage(damage, transform);
                }
                
                // 施加爆炸力
                var rb = collider.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 direction = (collider.transform.position - transform.position).normalized;
                    rb.AddForce(direction * explosionForce);
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }

    /// <summary>
    /// 胜利传送门
    /// </summary>
    public class VictoryPortal : MonoBehaviour
    {
        [Header("=== 传送门设置 ===")]
        [SerializeField] private string nextSceneName = "MainHub";
        [SerializeField] private float activationDelay = 2f;
        [SerializeField] private float rotationSpeed = 30f;
        
        [Header("=== 特效 ===")]
        [SerializeField] private ParticleSystem portalEffect;
        [SerializeField] private ParticleSystem activationEffect;
        [SerializeField] private Light portalLight;
        
        [Header("=== 音频 ===")]
        [SerializeField] private AudioClip activationSound;
        [SerializeField] private AudioClip teleportSound;
        
        [Header("=== 交互 ===")]
        [SerializeField] private float interactionRadius = 2f;
        [SerializeField] private KeyCode interactionKey = KeyCode.E;
        [SerializeField] private bool autoTeleport = false;
        [SerializeField] private float autoTeleportDelay = 3f;
        
        [Header("=== 倒计时UI ===")]
        [SerializeField] private TMPro.TextMeshProUGUI countdownText;
        [SerializeField] private GameObject countdownPanel;

        private bool _isActive = false;
        private bool _isPlayerInRange = false;
        private float _countdownTimer = 0f;
        private bool _isCountingDown = false;

        public bool IsActive => _isActive;

        private void Update()
        {
            if (!_isActive) return;
            
            // 旋转特效
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            
            // 检测玩家
            CheckPlayerProximity();
            
            // 交互或自动传送
            if (_isPlayerInRange)
            {
                if (autoTeleport)
                {
                    // 自动传送倒计时
                    UpdateCountdown();
                }
                else if (Input.GetKeyDown(interactionKey))
                {
                    TeleportPlayer();
                }
            }
            else
            {
                // 玩家离开范围，重置倒计时
                ResetCountdown();
            }
        }
        
        private void UpdateCountdown()
        {
            if (!_isCountingDown)
            {
                _isCountingDown = true;
                _countdownTimer = autoTeleportDelay;
                
                if (countdownPanel != null)
                    countdownPanel.SetActive(true);
            }
            
            _countdownTimer -= Time.deltaTime;
            
            // 更新UI
            if (countdownText != null)
            {
                countdownText.text = Mathf.CeilToInt(_countdownTimer).ToString();
            }
            
            // 倒计时结束，传送
            if (_countdownTimer <= 0)
            {
                TeleportPlayer();
            }
        }
        
        private void ResetCountdown()
        {
            _isCountingDown = false;
            _countdownTimer = 0f;
            
            if (countdownPanel != null)
                countdownPanel.SetActive(false);
        }

        public void Activate()
        {
            if (_isActive) return;
            
            _isActive = true;
            
            // 播放激活特效
            portalEffect?.Play();
            activationEffect?.Play();
            
            // 播放音效
            if (activationSound != null)
            {
                AudioManager.Instance?.PlaySFX(activationSound);
            }
            
            // 激活光源
            if (portalLight != null)
            {
                portalLight.enabled = true;
            }
            
            // 显示提示
            GameEvents.TriggerNotification("胜利传送门已激活！");
            
            // 自动传送
            if (autoTeleport)
            {
                StartCoroutine(AutoTeleportCoroutine());
            }
        }

        private System.Collections.IEnumerator AutoTeleportCoroutine()
        {
            yield return new WaitForSeconds(autoTeleportDelay);
            TeleportPlayer();
        }

        private void CheckPlayerProximity()
        {
            // 检测玩家是否在范围内
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactionRadius);
            
            _isPlayerInRange = false;
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Player"))
                {
                    _isPlayerInRange = true;
                    break;
                }
            }
        }

        private void TeleportPlayer()
        {
            // 播放传送音效
            if (teleportSound != null)
            {
                AudioManager.Instance?.PlaySFX(teleportSound);
            }
            
            // 播放传送特效
            if (BossEffectManager.Instance != null)
            {
                // 创建传送特效
                GameObject teleportEffect = new GameObject("TeleportEffect");
                teleportEffect.transform.position = transform.position;
                
                // 添加粒子系统
                ParticleSystem ps = teleportEffect.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.startColor = Color.cyan;
                main.startSize = 2f;
                main.duration = 2f;
                
                Destroy(teleportEffect, 2f);
            }
            
            // 加载下一关
            GameEvents.TriggerNotification("传送中...");
            
            // 延迟加载场景
            StartCoroutine(LoadSceneCoroutine());
        }

        private System.Collections.IEnumerator LoadSceneCoroutine()
        {
            // 淡出效果
            yield return StartCoroutine(SceneFadeOut());
            
            yield return new WaitForSeconds(1f);
            
            // 加载场景
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
        
        private System.Collections.IEnumerator SceneFadeOut()
        {
            // 创建全屏淡出面罩
            GameObject fadePanel = new GameObject("FadePanel");
            Canvas canvas = fadePanel.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            
            UnityEngine.UI.Image image = fadePanel.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0, 0, 0, 0);
            
            // 渐变到黑色
            float timer = 0f;
            float duration = 1f;
            
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float alpha = timer / duration;
                image.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
            
            image.color = Color.black;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}
