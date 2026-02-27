using UnityEngine;

namespace SebeJJ.ScriptableObjects
{
    /// <summary>
    /// 武器数据 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponData", menuName = "SebeJJ/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("基础信息")]
        public string weaponId;
        public string weaponName;
        [TextArea(3, 5)]
        public string description;
        public Sprite icon;
        
        [Header("武器类型")]
        public WeaponType weaponType;
        public DamageType damageType;
        
        [Header("伤害")]
        public float damage = 10f;
        public float fireRate = 1f;  // 每秒射击次数
        public float range = 10f;
        public float projectileSpeed = 20f;
        
        [Header("消耗")]
        public float energyCost = 5f;
        public int ammoCapacity = 0;  // 0 = 无限弹药
        
        [Header("预制体")]
        public GameObject projectilePrefab;
        public GameObject muzzleFlashPrefab;
        public AudioClip fireSound;
        
        [Header("特效")]
        public bool hasRecoil = true;
        public float recoilForce = 0.5f;
        public bool isAutomatic = false;
        public int burstCount = 1;
        public float burstDelay = 0.1f;
    }

    public enum WeaponType
    {
        MiningLaser,    // 采矿激光
        Harpoon,        // 鱼叉
        Torpedo,        // 鱼雷
        SonicPulse,     // 声波脉冲
        PlasmaCutter,   // 等离子切割器
        Drill,          // 钻头
        NetLauncher,    // 网发射器
        Electromagnetic // 电磁武器
    }
}