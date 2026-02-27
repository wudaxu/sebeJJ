using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 伤害数字管理器 - 显示伤害数值
    /// </summary>
    public class DamageNumberManager : MonoBehaviour
    {
        public static DamageNumberManager Instance { get; private set; }

        [Header("预制体")]
        [SerializeField] private GameObject damageNumberPrefab;
        [SerializeField] private Transform canvasTransform;

        [Header("设置")]
        [SerializeField] private float displayDuration = 1f;
        [SerializeField] private float floatSpeed = 50f;
        [SerializeField] private float spreadRange = 30f;
        [SerializeField] private Color normalDamageColor = Color.white;
        [SerializeField] private Color criticalDamageColor = Color.red;
        [SerializeField] private Color healColor = Color.green;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 显示伤害数字
        /// </summary>
        public void ShowDamage(float damage, Vector3 worldPosition, bool isCritical = false)
        {
            if (damageNumberPrefab == null) return;

            // 随机偏移
            Vector2 randomOffset = Random.insideUnitCircle * spreadRange;
            Vector3 spawnPosition = worldPosition + new Vector3(randomOffset.x, randomOffset.y, 0);

            // 创建伤害数字
            GameObject damageObj = Instantiate(damageNumberPrefab, canvasTransform);
            
            // 设置位置
            if (damageObj.TryGetComponent<RectTransform>(out var rectTransform))
            {
                Vector2 screenPosition = Camera.main.WorldToScreenPoint(spawnPosition);
                rectTransform.position = screenPosition;
            }

            // 设置文本
            if (damageObj.TryGetComponent<TextMeshProUGUI>(out var textMesh))
            {
                textMesh.text = Mathf.RoundToInt(damage).ToString();
                textMesh.color = isCritical ? criticalDamageColor : normalDamageColor;
                textMesh.fontSize = isCritical ? 36 : 24;
            }
            else if (damageObj.TryGetComponent<Text>(out var text))
            {
                text.text = Mathf.RoundToInt(damage).ToString();
                text.color = isCritical ? criticalDamageColor : normalDamageColor;
            }

            // 启动动画
            DamageNumberAnimation anim = damageObj.GetComponent<DamageNumberAnimation>();
            if (anim == null)
            {
                anim = damageObj.AddComponent<DamageNumberAnimation>();
            }
            anim.Initialize(displayDuration, floatSpeed);
        }

        /// <summary>
        /// 显示治疗数字
        /// </summary>
        public void ShowHeal(float amount, Vector3 worldPosition)
        {
            if (damageNumberPrefab == null) return;

            Vector2 randomOffset = Random.insideUnitCircle * spreadRange;
            Vector3 spawnPosition = worldPosition + new Vector3(randomOffset.x, randomOffset.y, 0);

            GameObject healObj = Instantiate(damageNumberPrefab, canvasTransform);
            
            if (healObj.TryGetComponent<RectTransform>(out var rectTransform))
            {
                Vector2 screenPosition = Camera.main.WorldToScreenPoint(spawnPosition);
                rectTransform.position = screenPosition;
            }

            if (healObj.TryGetComponent<TextMeshProUGUI>(out var textMesh))
            {
                textMesh.text = "+" + Mathf.RoundToInt(amount).ToString();
                textMesh.color = healColor;
            }
            else if (healObj.TryGetComponent<Text>(out var text))
            {
                text.text = "+" + Mathf.RoundToInt(amount).ToString();
                text.color = healColor;
            }

            DamageNumberAnimation anim = healObj.GetComponent<DamageNumberAnimation>();
            if (anim == null)
            {
                anim = healObj.AddComponent<DamageNumberAnimation>();
            }
            anim.Initialize(displayDuration, floatSpeed);
        }
    }

    /// <summary>
    /// 伤害数字动画
    /// </summary>
    public class DamageNumberAnimation : MonoBehaviour
    {
        private float _duration;
        private float _floatSpeed;
        private float _timer;
        private Vector3 _startPosition;
        private TextMeshProUGUI _textMesh;
        private Text _text;

        public void Initialize(float duration, float floatSpeed)
        {
            _duration = duration;
            _floatSpeed = floatSpeed;
            _timer = 0f;
            _startPosition = transform.position;
            _textMesh = GetComponent<TextMeshProUGUI>();
            _text = GetComponent<Text>();
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            // 向上浮动
            transform.position = _startPosition + Vector3.up * (_timer * _floatSpeed);

            // 淡出
            float alpha = 1f - (_timer / _duration);
            
            if (_textMesh != null)
            {
                Color color = _textMesh.color;
                color.a = alpha;
                _textMesh.color = color;
            }
            else if (_text != null)
            {
                Color color = _text.color;
                color.a = alpha;
                _text.color = color;
            }

            // 销毁
            if (_timer >= _duration)
            {
                Destroy(gameObject);
            }
        }
    }
}
