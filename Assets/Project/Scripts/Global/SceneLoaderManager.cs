using System;
using Cysharp.Threading.Tasks;
using Match3.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderManager : BaseMono
{
    public static event Action<SceneTypes> OnBeforeSceneLoad;
    public static event Action<SceneTypes> OnAfterSceneLoad;
    
    public enum SceneTypes
    {
        None,
        LoadingScene,
        MenuScene,
        GameplayScene
    }

    public override async UniTask Init(object data = null)
    {
        // Register to asset preloader events
        AssetPreloader.OnLoadingComplete += HandleLoadingComplete;
        AssetPreloader.OnLoadingFailed += HandleLoadingFailed;

        await base.Init(data);

        Debug.Log("SceneLoaderManager initialized");
    }

    private void HandleLoadingComplete()
    {
        Debug.Log("Asset loading complete - transitioning to main menu");
        LoadMainMenuSceneAsync().Forget();
    }

    private void HandleLoadingFailed()
    {
        Debug.LogError("Asset loading failed - cannot transition to main menu");

        // Attempt to load the menu anyway after a delay
        DelayedRetryLoadMainMenuAsync().Forget();
    }

    private async UniTaskVoid DelayedRetryLoadMainMenuAsync()
    {
        await UniTask.Delay(1000);
        await LoadMainMenuSceneAsync();
    }

    private async UniTask LoadMainMenuSceneAsync()
    {
        // Load the main menu scene
        await LoadSceneAsync(SceneTypes.MenuScene);
    }

    public async UniTask LoadSceneAsync(SceneTypes sceneType)
    {
        if (sceneType == SceneTypes.None)
        {
            Debug.LogError("Cannot load scene of type None");
            return;
        }

        string sceneName = sceneType.ToString();
        Debug.Log($"Loading scene: {sceneName}");

        // Notify that we're about to load a new scene
        OnBeforeSceneLoad?.Invoke(sceneType);
        
        // Wait a bit to give the fade transition time to complete
        await UniTask.Delay(500);

        // General purpose scene loading method
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
        
        // Wait for the scene to finish loading
        await loadOperation;
        
        // Give a small delay to ensure the scene is properly initialized
        await UniTask.Delay(100);

        // Notify that the scene has been loaded
        OnAfterSceneLoad?.Invoke(sceneType);
        
        Debug.Log($"Scene loaded: {sceneName}");
    }

    private void OnDestroy()
    {
        // Ensure we unregister when this object is destroyed
        AssetPreloader.OnLoadingComplete -= HandleLoadingComplete;
        AssetPreloader.OnLoadingFailed -= HandleLoadingFailed;
    }
}