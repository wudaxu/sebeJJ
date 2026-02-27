using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace SebeJJ.Core
{
    /// <summary>
    /// 场景加载器 - 异步加载优化
    /// 优化点:
    /// 1. 异步加载避免阻塞
    /// 2. 进度回调支持
    /// 3. 资源预加载
    /// 4. 场景切换过渡
    /// </summary>
    public class SceneLoaderOptimized : MonoBehaviour
    {
        public static SceneLoaderOptimized Instance { get; private set; }
        
        [Header("加载设置")]
        [SerializeField] private float minimumLoadTime = 1f; // 最少显示加载画面时间
        [SerializeField] private bool showLoadingScreen = true;
        
        [Header("过渡设置")]
        [SerializeField] private CanvasGroup loadingScreenCanvas;
        [SerializeField] private float fadeDuration = 0.5f;
        
        // 状态
        private bool isLoading = false;
        private float currentProgress = 0f;
        private string currentSceneName;
        
        // 事件
        public System.Action<float> OnLoadProgress;
        public System.Action<string> OnSceneLoaded;
        public System.Action OnLoadStarted;
        public System.Action OnLoadCompleted;
        
        public bool IsLoading => isLoading;
        public float CurrentProgress => currentProgress;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        /// <summary>
        /// 异步加载场景
        /// </summary>
        public async void LoadSceneAsync(string sceneName, System.Action onComplete = null)
        {
            if (isLoading)
            {
                Debug.LogWarning($"[SceneLoader] Already loading scene: {currentSceneName}");
                return;
            }
            
            isLoading = true;
            currentSceneName = sceneName;
            currentProgress = 0f;
            
            OnLoadStarted?.Invoke();
            
            // 显示加载画面
            if (showLoadingScreen)
            {
                await ShowLoadingScreen();
            }
            
            float loadStartTime = Time.time;
            
            // 开始异步加载
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;
            
            // 更新进度
            while (operation.progress < 0.9f)
            {
                currentProgress = operation.progress;
                OnLoadProgress?.Invoke(currentProgress);
                await Task.Yield();
            }
            
            currentProgress = 0.9f;
            OnLoadProgress?.Invoke(currentProgress);
            
            // 确保最少加载时间（避免加载画面闪烁）
            float elapsedTime = Time.time - loadStartTime;
            if (elapsedTime < minimumLoadTime)
            {
                float remainingTime = minimumLoadTime - elapsedTime;
                float fakeProgress = 0.9f;
                float progressStep = 0.1f / (remainingTime * 60f); // 假设60fps
                
                while (elapsedTime < minimumLoadTime)
                {
                    await Task.Delay(16); // ~60fps
                    elapsedTime = Time.time - loadStartTime;
                    fakeProgress = Mathf.Min(0.99f, fakeProgress + progressStep);
                    OnLoadProgress?.Invoke(fakeProgress);
                }
            }
            
            currentProgress = 1f;
            OnLoadProgress?.Invoke(currentProgress);
            
            // 激活场景
            operation.allowSceneActivation = true;
            
            // 等待场景加载完成
            while (!operation.isDone)
            {
                await Task.Yield();
            }
            
            OnSceneLoaded?.Invoke(sceneName);
            
            // 隐藏加载画面
            if (showLoadingScreen)
            {
                await HideLoadingScreen();
            }
            
            isLoading = false;
            OnLoadCompleted?.Invoke();
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// 带进度报告的异步加载
        /// </summary>
        public async Task LoadSceneWithProgress(string sceneName, IProgress<float> progress)
        {
            if (isLoading) return;
            
            isLoading = true;
            currentSceneName = sceneName;
            
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;
            
            while (operation.progress < 0.9f)
            {
                progress?.Report(operation.progress);
                await Task.Yield();
            }
            
            progress?.Report(1f);
            operation.allowSceneActivation = true;
            
            while (!operation.isDone)
            {
                await Task.Yield();
            }
            
            isLoading = false;
        }
        
        /// <summary>
        /// 预加载场景（不激活）
        /// </summary>
        public async Task PreloadScene(string sceneName)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            operation.allowSceneActivation = false;
            
            while (operation.progress < 0.9f)
            {
                await Task.Yield();
            }
            
            // 场景已加载但未激活，可以稍后激活
            Debug.Log($"[SceneLoader] Scene '{sceneName}' preloaded");
        }
        
        /// <summary>
        /// 激活预加载的场景
        /// </summary>
        public void ActivatePreloadedScene(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (scene.isLoaded)
            {
                SceneManager.SetActiveScene(scene);
            }
        }
        
        /// <summary>
        /// 卸载场景
        /// </summary>
        public async Task UnloadSceneAsync(string sceneName)
        {
            AsyncOperation operation = SceneManager.UnloadSceneAsync(sceneName);
            
            while (!operation.isDone)
            {
                await Task.Yield();
            }
            
            // 强制垃圾回收
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }
        
        private async Task ShowLoadingScreen()
        {
            if (loadingScreenCanvas == null) return;
            
            loadingScreenCanvas.gameObject.SetActive(true);
            loadingScreenCanvas.alpha = 0f;
            
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                loadingScreenCanvas.alpha = elapsed / fadeDuration;
                await Task.Yield();
            }
            
            loadingScreenCanvas.alpha = 1f;
        }
        
        private async Task HideLoadingScreen()
        {
            if (loadingScreenCanvas == null) return;
            
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                loadingScreenCanvas.alpha = 1f - (elapsed / fadeDuration);
                await Task.Yield();
            }
            
            loadingScreenCanvas.alpha = 0f;
            loadingScreenCanvas.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 加载场景并预加载资源
        /// </summary>
        public async void LoadSceneWithPreload(string sceneName, string[] assetsToPreload, System.Action onComplete = null)
        {
            if (isLoading) return;
            
            isLoading = true;
            OnLoadStarted?.Invoke();
            
            // 预加载资源
            if (assetsToPreload != null && assetsToPreload.Length > 0)
            {
                await PreloadAssets(assetsToPreload);
            }
            
            // 加载场景
            await LoadSceneWithProgress(sceneName, new Progress<float>(p => {
                currentProgress = p;
                OnLoadProgress?.Invoke(p);
            }));
            
            isLoading = false;
            OnLoadCompleted?.Invoke();
            onComplete?.Invoke();
        }
        
        private async Task PreloadAssets(string[] assetNames)
        {
            // 这里可以实现Addressables预加载
            // 暂时使用模拟延迟
            foreach (var assetName in assetNames)
            {
                Debug.Log($"[SceneLoader] Preloading asset: {assetName}");
                await Task.Delay(100);
            }
        }
    }
}
