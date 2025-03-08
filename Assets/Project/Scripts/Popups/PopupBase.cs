using System;
using Cysharp.Threading.Tasks;
using Match3.Core;
using UnityEngine;
using DG.Tweening;

public abstract class PopupBase : BaseMono
{
    // Event for when popup is closed
    public event Action<PopupBase> OnPopupClosed;

    [field: SerializeField] public PopupsManager.PopupType PopupType = PopupsManager.PopupType.None;

    [Header("Animation Settings")] [SerializeField]
    private float showDuration = 0.3f;

    [SerializeField] private float hideDuration = 0.25f;
    [SerializeField] private float scaleMultiplier = 1.05f;


    // Cache components and original scale
    private CanvasGroup _canvasGroup;
    private Vector3 _originalScale;
    private Sequence _currentSequence;

    public override async UniTask Init(object data = null)
    {
        await base.Init(data);

        // Cache original scale
        _originalScale = transform.localScale;

        // Get or add CanvasGroup
        _canvasGroup = GetComponent<CanvasGroup>();

        // Set initial state
        _canvasGroup.alpha = 0f;
        transform.localScale = Vector3.zero;
    }

    public virtual async UniTask ShowPopup()
    {
        // Kill any running animations
        _currentSequence?.Kill();

        // Reset to initial state
        _canvasGroup.alpha = 0f;
        transform.localScale = Vector3.zero;

        gameObject.SetActive(true);

        // Create show sequence
        _currentSequence = DOTween.Sequence();

        var completionSource = new UniTaskCompletionSource();

        _currentSequence.Append(transform.DOScale(_originalScale * scaleMultiplier, showDuration * 0.7f)
                .SetEase(Ease.OutBack))
            .Join(_canvasGroup.DOFade(1f, showDuration * 0.6f))
            .Append(transform.DOScale(_originalScale, showDuration * 0.3f))
            .OnComplete(() => completionSource.TrySetResult());

        await completionSource.Task;
    }


    // Method to be called when popup should close
    protected async void ClosePopup()
    {
        await HidePopupAnimation();
        OnPopupClosed?.Invoke(this);
    }

    private async UniTask HidePopupAnimation()
    {
        // Kill any running animations
        _currentSequence?.Kill();

        var completionSource = new UniTaskCompletionSource();

        _currentSequence = DOTween.Sequence();
        _currentSequence.Append(transform.DOScale(Vector3.zero, hideDuration).SetEase(Ease.InBack))
            .Join(_canvasGroup.DOFade(0f, hideDuration * 0.8f))
            .OnComplete(() => completionSource.TrySetResult());

        await completionSource.Task;
    }

    private void OnDestroy()
    {
        // Clean up any running tweens
        _currentSequence?.Kill();
    }
}