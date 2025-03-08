// AssetPreloader.cs - Handles the actual loading process
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Match3.Core
{
    public class AssetPreloader : BaseMono
    {
        // Events for other scripts to listen to - only pass progress value
        public static event System.Action<float> OnLoadingProgressChanged;
        public static event System.Action OnLoadingComplete;
        public static event System.Action OnLoadingFailed;
        private static readonly string[] ASSET_LABELS = { "Preload_Assets", "Gameplay_Assets" };

    
        public override async UniTask Init(object data = null)
        { 
            await base.Init(data);
            await  LoadAllAssets();
            
            Debug.Log("SceneLoaderManager initialized");
        }

        
        private async UniTask LoadAllAssets()
        {
            try
            {
                ReportProgress(0.0f); // Start at 0%
        
                // Always load the assets, regardless of download size
                float progressPerLabel = 1.0f / ASSET_LABELS.Length;
                float currentProgress = 0f;
        
                foreach (string label in ASSET_LABELS)
                {
                    await LoadLabelWithProgress(label, currentProgress, progressPerLabel);
                    currentProgress += progressPerLabel;
                }
        
                // Loading complete
                ReportProgress(1.0f);
                OnLoadingComplete?.Invoke();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Asset loading failed: {e.Message}");
                OnLoadingFailed?.Invoke();
            }
        }

        private async UniTask LoadLabelWithProgress(string label, float baseProgress, float progressWeight)
        {
            // Load the assets regardless of whether they need to be downloaded
            AsyncOperationHandle loadHandle = Addressables.LoadAssetsAsync<object>(
                label,
                null // We don't need the loaded objects, just want to ensure they're loaded
            );
    
            while (!loadHandle.IsDone)
            {
                float labelProgress = loadHandle.PercentComplete;
                float totalProgress = baseProgress + (labelProgress * progressWeight);
                ReportProgress(totalProgress);
                await UniTask.Yield();
            }
    
            if (loadHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Addressables.Release(loadHandle);
                throw new System.Exception($"Failed to load assets for label: {label}");
            }
            
            Addressables.Release(loadHandle);
        }
        
        private void ReportProgress(float progress)
        {
            // Just report the raw progress - let UI handle display formatting
            OnLoadingProgressChanged?.Invoke(progress);
        }
    }
}