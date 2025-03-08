using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SceneFader : BaseMono
{
    [SerializeField] private Image fadePanel;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private Ease fadeInEase = Ease.InQuad;
    [SerializeField] private Ease fadeOutEase = Ease.OutQuad;

    private Tweener _currentTween;

    public override async UniTask Init(object data = null)
    {
        // Set initial state
        fadePanel.raycastTarget = false;
        fadePanel.color = new Color(0, 0, 0, 0);

        // Register to scene loader events
        SceneLoaderManager.OnBeforeSceneLoad += HandleBeforeSceneLoad;
        SceneLoaderManager.OnAfterSceneLoad += HandleAfterSceneLoad;

        await base.Init(data);
    }

    private void HandleBeforeSceneLoad(SceneLoaderManager.SceneTypes sceneType)
    {
        Debug.Log("FadeInFadeInFadeInFadeIn");
        // Fade to black when scene loading begins
        FadeIn(fadeDuration).Forget();
    }

    private void HandleAfterSceneLoad(SceneLoaderManager.SceneTypes sceneType)
    {
        // Fade from black when scene loading completes
        FadeOut(fadeDuration).Forget();
    }

    // Fade to black
    private async UniTask FadeIn(float duration)
    {
        // Kill any active fade
        _currentTween?.Kill(true);

        // Enable raycast blocking during fade
        fadePanel.raycastTarget = true;

        // Create a UniTask completion source
        UniTaskCompletionSource completionSource = new UniTaskCompletionSource();

        // Start the fade in tween
        _currentTween = fadePanel.DOFade(1f, duration)
            .SetEase(fadeInEase)
            .SetUpdate(true)
            .OnComplete(() => completionSource.TrySetResult());

        // Wait for completion
        await completionSource.Task;
    }

    // Fade from black
    private async UniTask FadeOut(float duration)
    {
        // Kill any active fade
        _currentTween?.Kill(true);

        // Create a UniTask completion source
        UniTaskCompletionSource completionSource = new UniTaskCompletionSource();

        // Start the fade out tween
        _currentTween = fadePanel.DOFade(0f, duration)
            .SetEase(fadeOutEase)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                fadePanel.raycastTarget = false;
                completionSource.TrySetResult();
            });

        // Wait for completion
        await completionSource.Task;
    }

    private void OnDestroy()
    {
        SceneLoaderManager.OnBeforeSceneLoad -= HandleBeforeSceneLoad;
        SceneLoaderManager.OnAfterSceneLoad -= HandleAfterSceneLoad;
        _currentTween?.Kill();
    }
}