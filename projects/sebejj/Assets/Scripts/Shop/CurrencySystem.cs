using UnityEngine;
using System;

namespace SebeJJ.Shop
{
    /// <summary>
    /// 货币类型
    /// </summary>
    public enum CurrencyType
    {
        Credits,        // 信用点（主要货币）
        PremiumCredits, // 高级信用点（付费货币）
        Scrap,          // 废料（次要货币）
        Reputation      // 声望
    }

    /// <summary>
    /// 货币变更事件
    /// </summary>
    public class CurrencyChangedEvent
    {
        public CurrencyType CurrencyType;
        public int OldAmount;
        public int NewAmount;
        public int Delta;
        public string Reason;
    }

    /// <summary>
    /// 货币系统 - 管理所有货币类型
    /// </summary>
    public class CurrencySystem : MonoBehaviour
    {
        public static CurrencySystem Instance { get; private set; }
        
        [Header("初始货币")]
        [SerializeField] private int initialCredits = 1000;
        [SerializeField] private int initialPremiumCredits = 0;
        [SerializeField] private int initialScrap = 0;
        [SerializeField] private int initialReputation = 0;
        
        // 货币存储
        private int _credits;
        private int _premiumCredits;
        private int _scrap;
        private int _reputation;
        
        // 事件
        public event Action<CurrencyChangedEvent> OnCurrencyChanged;
        
        // 属性
        public int Credits 
        { 
            get => _credits;
            private set
            {
                if (_credits != value)
                {
                    int old = _credits;
                    _credits = Mathf.Max(0, value);
                    OnCurrencyChanged?.Invoke(new CurrencyChangedEvent
                    {
                        CurrencyType = CurrencyType.Credits,
                        OldAmount = old,
                        NewAmount = _credits,
                        Delta = _credits - old,
                        Reason = "Manual Set"
                    });
                }
            }
        }
        
        public int PremiumCredits 
        { 
            get => _premiumCredits;
            private set
            {
                if (_premiumCredits != value)
                {
                    int old = _premiumCredits;
                    _premiumCredits = Mathf.Max(0, value);
                    OnCurrencyChanged?.Invoke(new CurrencyChangedEvent
                    {
                        CurrencyType = CurrencyType.PremiumCredits,
                        OldAmount = old,
                        NewAmount = _premiumCredits,
                        Delta = _premiumCredits - old,
                        Reason = "Manual Set"
                    });
                }
            }
        }
        
        public int Scrap 
        { 
            get => _scrap;
            private set
            {
                if (_scrap != value)
                {
                    int old = _scrap;
                    _scrap = Mathf.Max(0, value);
                    OnCurrencyChanged?.Invoke(new CurrencyChangedEvent
                    {
                        CurrencyType = CurrencyType.Scrap,
                        OldAmount = old,
                        NewAmount = _scrap,
                        Delta = _scrap - old,
                        Reason = "Manual Set"
                    });
                }
            }
        }
        
        public int Reputation 
        { 
            get => _reputation;
            private set
            {
                if (_reputation != value)
                {
                    int old = _reputation;
                    _reputation = Mathf.Max(0, value);
                    OnCurrencyChanged?.Invoke(new CurrencyChangedEvent
                    {
                        CurrencyType = CurrencyType.Reputation,
                        OldAmount = old,
                        NewAmount = _reputation,
                        Delta = _reputation - old,
                        Reason = "Manual Set"
                    });
                }
            }
        }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeCurrency();
        }
        
        /// <summary>
        /// 初始化货币
        /// </summary>
        private void InitializeCurrency()
        {
            _credits = initialCredits;
            _premiumCredits = initialPremiumCredits;
            _scrap = initialScrap;
            _reputation = initialReputation;
        }
        
        /// <summary>
        /// 获取指定类型的货币数量
        /// </summary>
        public int GetCurrency(CurrencyType type)
        {
            return type switch
            {
                CurrencyType.Credits => Credits,
                CurrencyType.PremiumCredits => PremiumCredits,
                CurrencyType.Scrap => Scrap,
                CurrencyType.Reputation => Reputation,
                _ => 0
            };
        }
        
        /// <summary>
        /// 增加货币
        /// </summary>
        public void AddCurrency(CurrencyType type, int amount, string reason = "")
        {
            if (amount <= 0) return;
            
            int oldAmount = GetCurrency(type);
            
            switch (type)
            {
                case CurrencyType.Credits:
                    Credits += amount;
                    break;
                case CurrencyType.PremiumCredits:
                    PremiumCredits += amount;
                    break;
                case CurrencyType.Scrap:
                    Scrap += amount;
                    break;
                case CurrencyType.Reputation:
                    Reputation += amount;
                    break;
            }
            
            OnCurrencyChanged?.Invoke(new CurrencyChangedEvent
            {
                CurrencyType = type,
                OldAmount = oldAmount,
                NewAmount = GetCurrency(type),
                Delta = amount,
                Reason = string.IsNullOrEmpty(reason) ? "Added" : reason
            });
        }
        
        /// <summary>
        /// 扣除货币
        /// </summary>
        public bool DeductCurrency(CurrencyType type, int amount, string reason = "")
        {
            if (amount <= 0) return true;
            if (GetCurrency(type) < amount) return false;
            
            int oldAmount = GetCurrency(type);
            
            switch (type)
            {
                case CurrencyType.Credits:
                    Credits -= amount;
                    break;
                case CurrencyType.PremiumCredits:
                    PremiumCredits -= amount;
                    break;
                case CurrencyType.Scrap:
                    Scrap -= amount;
                    break;
                case CurrencyType.Reputation:
                    Reputation -= amount;
                    break;
            }
            
            OnCurrencyChanged?.Invoke(new CurrencyChangedEvent
            {
                CurrencyType = type,
                OldAmount = oldAmount,
                NewAmount = GetCurrency(type),
                Delta = -amount,
                Reason = string.IsNullOrEmpty(reason) ? "Deducted" : reason
            });
            
            return true;
        }
        
        /// <summary>
        /// 检查是否有足够的货币
        /// </summary>
        public bool HasEnoughCurrency(CurrencyType type, int amount)
        {
            return GetCurrency(type) >= amount;
        }
        
        /// <summary>
        /// 重置所有货币
        /// </summary>
        public void ResetAllCurrency()
        {
            InitializeCurrency();
        }
        
        /// <summary>
        /// 保存货币数据（可扩展为持久化存储）
        /// </summary>
        public void SaveCurrencyData()
        {
            PlayerPrefs.SetInt("Credits", Credits);
            PlayerPrefs.SetInt("PremiumCredits", PremiumCredits);
            PlayerPrefs.SetInt("Scrap", Scrap);
            PlayerPrefs.SetInt("Reputation", Reputation);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// 加载货币数据
        /// </summary>
        public void LoadCurrencyData()
        {
            _credits = PlayerPrefs.GetInt("Credits", initialCredits);
            _premiumCredits = PlayerPrefs.GetInt("PremiumCredits", initialPremiumCredits);
            _scrap = PlayerPrefs.GetInt("Scrap", initialScrap);
            _reputation = PlayerPrefs.GetInt("Reputation", initialReputation);
        }
    }
}