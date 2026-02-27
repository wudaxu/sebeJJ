using UnityEngine;

namespace SebeJJ.Utils
{
    /// <summary>
    /// 扩展方法类
    /// </summary>
    public static class Extensions
    {
        #region Transform Extensions

        /// <summary>
        /// 重置Transform
        /// </summary>
        public static void Reset(this Transform transform)
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// 销毁所有子物体
        /// </summary>
        public static void DestroyChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// 查找或创建子物体
        /// </summary>
        public static Transform FindOrCreate(this Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child == null)
            {
                GameObject go = new GameObject(name);
                child = go.transform;
                child.SetParent(parent);
                child.Reset();
            }
            return child;
        }

        #endregion

        #region Vector Extensions

        /// <summary>
        /// 设置X坐标
        /// </summary>
        public static Vector3 WithX(this Vector3 vector, float x)
        {
            return new Vector3(x, vector.y, vector.z);
        }

        /// <summary>
        /// 设置Y坐标
        /// </summary>
        public static Vector3 WithY(this Vector3 vector, float y)
        {
            return new Vector3(vector.x, y, vector.z);
        }

        /// <summary>
        /// 设置Z坐标
        /// </summary>
        public static Vector3 WithZ(this Vector3 vector, float z)
        {
            return new Vector3(vector.x, vector.y, z);
        }

        /// <summary>
        /// 2D向量转3D (XY平面)
        /// </summary>
        public static Vector3 ToVector3(this Vector2 vector)
        {
            return new Vector3(vector.x, vector.y, 0);
        }

        /// <summary>
        /// 3D向量转2D (XY平面)
        /// </summary>
        public static Vector2 ToVector2(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.y);
        }

        #endregion

        #region GameObject Extensions

        /// <summary>
        /// 获取或添加组件
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }

        /// <summary>
        /// 检查层级
        /// </summary>
        public static bool IsInLayerMask(this GameObject gameObject, LayerMask layerMask)
        {
            return (layerMask.value & (1 << gameObject.layer)) != 0;
        }

        #endregion

        #region Float Extensions

        /// <summary>
        /// 映射到范围
        /// </summary>
        public static float Remap(this float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
        }

        /// <summary>
        /// 限制在范围内
        /// </summary>
        public static float Clamp(this float value, float min, float max)
        {
            return Mathf.Clamp(value, min, max);
        }

        /// <summary>
        /// 检查是否在范围内
        /// </summary>
        public static bool IsBetween(this float value, float min, float max)
        {
            return value >= min && value <= max;
        }

        #endregion

        #region Int Extensions

        /// <summary>
        /// 限制在范围内
        /// </summary>
        public static int Clamp(this int value, int min, int max)
        {
            return Mathf.Clamp(value, min, max);
        }

        /// <summary>
        /// 格式化数字 (如: 1.2K, 3.4M)
        /// </summary>
        public static string ToShortString(this int value)
        {
            if (value >= 1000000)
                return (value / 1000000f).ToString("F1") + "M";
            if (value >= 1000)
                return (value / 1000f).ToString("F1") + "K";
            return value.ToString();
        }

        #endregion

        #region String Extensions

        /// <summary>
        /// 添加颜色标签
        /// </summary>
        public static string Color(this string text, string color)
        {
            return $"<color={color}>{text}</color>";
        }

        /// <summary>
        /// 添加粗体标签
        /// </summary>
        public static string Bold(this string text)
        {
            return $"<b>{text}</b>";
        }

        /// <summary>
        /// 添加斜体标签
        /// </summary>
        public static string Italic(this string text)
        {
            return $"<i>{text}</i>";
        }

        /// <summary>
        /// 添加大小标签
        /// </summary>
        public static string Size(this string text, int size)
        {
            return $"<size={size}>{text}</size>";
        }

        #endregion

        #region Camera Extensions

        /// <summary>
        /// 获取相机边界
        /// </summary>
        public static Bounds OrthographicBounds(this Camera camera)
        {
            float screenAspect = (float)Screen.width / Screen.height;
            float cameraHeight = camera.orthographicSize * 2;
            Bounds bounds = new Bounds(
                camera.transform.position,
                new Vector3(cameraHeight * screenAspect, cameraHeight, 0));
            return bounds;
        }

        #endregion
    }
}