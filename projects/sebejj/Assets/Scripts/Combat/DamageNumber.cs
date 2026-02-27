using UnityEngine;
using TMPro;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 伤害数字显示 - DM-004
    /// </summary>
    public class DamageNumber : MonoBehaviour
    {
        [Header("视觉设置")]
        [SerializeField] private TextMeshPro textMesh;
        [SerializeField] private SpriteRenderer iconRenderer;

        [Header("动画设置")]
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float fadeDuration = 1f;
        [SerializeField] private float scaleDuration = 0.2f;
        [SerializeField] private Vector2 randomOffset = new Vector2(0.5f, 0.5f);

        [Header("颜色设置")]
        [SerializeField] private Color normalDamageColor = Color.white;
        [SerializeField] private Color criticalDamageColor = Color.yellow;
        [SerializeField] private Color healColor = Color.green;
        [SerializeField] private Color shieldDamageColor = Color.cyan;

        private float lifetime;
        private Vector2 moveDirection;
        private Vector3 targetScale;

        public void Initialize(float damage, bool isCritical = false, bool isHeal = false, bool isShieldDamage = false)
        {
            if (textMesh == null)
                textMesh = GetComponent<TextMeshPro>();

            // 设置数值文本
            int displayValue = Mathf.RoundToInt(damage);
            textMesh.text = displayValue.ToString();

            // 设置颜色
            if (isHeal)
                textMesh.color = healColor;
            else if (isShieldDamage)
                textMesh.color = shieldDamageColor;
            else if (isCritical)
                textMesh.color = criticalDamageColor;
            else
                textMesh.color = normalDamageColor;

            // 暴击放大
            if (isCritical)
            {
                textMesh.fontSize *= 1.5f;
                textMesh.text = $"{displayValue}!";
            }

            // 随机移动方向
            moveDirection = new Vector2(
                Random.Range(-randomOffset.x, randomOffset.x),
                Random.Range(0.5f, randomOffset.y)
            ).normalized;

            // 初始缩放动画
            targetScale = transform.localScale;
            transform.localScale = Vector3.zero;
            LeanTween.scale(gameObject, targetScale, scaleDuration).setEaseOutBack();

            // 销毁
            Destroy(gameObject, fadeDuration);
        }

        private void Update()
        {
            // 向上飘动
            transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);

            // 淡出
            lifetime += Time.deltaTime;
            float alpha = 1f - (lifetime / fadeDuration);
            
            if (textMesh != null)
            {
                Color color = textMesh.color;
                color.a = alpha;
                textMesh.color = color;
            }
        }
    }
}