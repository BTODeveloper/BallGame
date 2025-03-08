using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class PopupLoadingController : BaseMono
{
    [Header("References")]
    [SerializeField] private CanvasGroup loadingCanvasGroup;
    [SerializeField] private GameObject loadingContainer;
    
    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private Ease fadeEase = Ease.OutQuad;
    
    private PopupsManager _popupsManager;
    private Tweener _currentTween;
    
    public override async UniTask Init(object data = null)
    {
        if (data is GlobalManagers globalManagers)
        {
            _popupsManager = globalManagers.PopupsManager;
            
            // Register to popup events
            _popupsManager.OnPopupLoadingStarted += ShowLoading;
            _popupsManager.OnPopupLoadingFinished += HideLoading;
        }
        
        // Initialize loading screen
        if (loadingCanvasGroup != null)
        {
            loadingCanvasGroup.alpha = 0f;
            loadingCanvasGroup.blocksRaycasts = false;
            loadingContainer.SetActive(false);
        }
        
        await base.Init(data);
    }
    
    private async void ShowLoading()
    {
        // Make sure loading container is active
        loadingContainer.SetActive(true);
        
        // Kill any active tween
        _currentTween?.Kill();
        
        // Enable blocking input
        loadingCanvasGroup.blocksRaycasts = true;
        
        // Fade in animation
        UniTaskCompletionSource fadeCompletionSource = new UniTaskCompletionSource();
        
        _currentTween = loadingCanvasGroup.DOFade(1f, fadeDuration)
            .SetEase(fadeEase)
            .SetUpdate(true)
            .OnComplete(() => fadeCompletionSource.TrySetResult());
            
        await fadeCompletionSource.Task;
    }
    
    private async void HideLoading()
    {
        // Kill any active tween
        _currentTween?.Kill();
        
        // Fade out animation
        UniTaskCompletionSource fadeCompletionSource = new UniTaskCompletionSource();
        
        _currentTween = loadingCanvasGroup.DOFade(0f, fadeDuration)
            .SetEase(fadeEase)
            .SetUpdate(true)
            .OnComplete(() => {
                loadingCanvasGroup.blocksRaycasts = false;
                loadingContainer.SetActive(false);
                fadeCompletionSource.TrySetResult();
            });
            
        await fadeCompletionSource.Task;
    }
    
    private void OnDestroy()
    {
        // Clean up events
        if (_popupsManager != null)
        {
            _popupsManager.OnPopupLoadingStarted -= ShowLoading;
            _popupsManager.OnPopupLoadingFinished -= HideLoading;
        }
        
        // Kill any active tweens
        _currentTween?.Kill();
    }
}