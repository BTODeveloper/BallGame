using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Match3.Core
{
    public class LoadingBar : BaseMono
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private TMP_Text loadingText;
        [SerializeField] private string loadingMessageFormat = "Loading cool stuff... {0}%";
        [SerializeField] private string completedMessage = "Loading complete!";
        [SerializeField] private string failedMessage = "Loading failed!";
        
        [Header("Animation Settings")]
        [SerializeField] private float minLoadingDuration = 2.0f;
        [SerializeField] private float fillAnimationDuration = 0.5f;
        [SerializeField] private Ease fillEaseType = Ease.OutQuad;
        
        private Tweener _fillTweener;
        private float _loadingStartTime;
        
        public override async UniTask Init(object data = null)
        { 
            AssetPreloader.OnLoadingProgressChanged += UpdateLoadingProgress;
            AssetPreloader.OnLoadingComplete += HandleLoadingComplete;
            AssetPreloader.OnLoadingFailed += HandleLoadingFailed;
            
            // Record when we start loading
            _loadingStartTime = Time.time;
            
            await base.Init(data);
        
            Debug.Log("SceneLoaderManager initialized");
        }
        
        private void UpdateLoadingProgress(float progress)
        {
            // Kill any existing tween
            _fillTweener?.Kill();
    
            // Animate to the new progress value
            _fillTweener = fillImage.DOFillAmount(progress, fillAnimationDuration)
                .SetEase(fillEaseType);
    
            // Update loading text - use one decimal place for more gradual updates
            float percentage = progress * 100f;
            loadingText.text = string.Format(loadingMessageFormat, percentage.ToString("F0"));
        }
        
        private void HandleLoadingComplete()
        {
            // Kill any existing tween
            _fillTweener?.Kill();
            
            // Calculate how long we've been loading
            float loadingElapsed = Time.time - _loadingStartTime;
            float remainingTime = Mathf.Max(0, minLoadingDuration - loadingElapsed);
            
            // Ensure we show the loading animation for at least minLoadingDuration
            if (remainingTime > 0)
            {
                // Create a sequence to finish the progress bar smoothly
                DOTween.Sequence()
                    .Append(fillImage.DOFillAmount(1.0f, remainingTime).SetEase(Ease.InOutQuad))
                    .AppendCallback(() => {
                        loadingText.text = completedMessage;
                    });
            }
            else
            {
                // If we've already been loading longer than minLoadingDuration, just finish immediately
                fillImage.DOFillAmount(1.0f, fillAnimationDuration).SetEase(fillEaseType);
                loadingText.text = completedMessage;
            }
        }
        
        private void HandleLoadingFailed()
        {
            // Kill any existing tween
            _fillTweener?.Kill();
            
            // Animate to 0 on failure
            fillImage.DOFillAmount(0f, fillAnimationDuration).SetEase(Ease.InBack);
            loadingText.text = failedMessage;
        }
        
        private void OnDestroy()
        {
            AssetPreloader.OnLoadingProgressChanged -= UpdateLoadingProgress;
            AssetPreloader.OnLoadingComplete -= HandleLoadingComplete;
            AssetPreloader.OnLoadingFailed -= HandleLoadingFailed;
            
            // Kill any active tweens
            _fillTweener?.Kill();
        }
    }
}