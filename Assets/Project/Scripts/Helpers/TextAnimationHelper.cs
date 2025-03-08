using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public static class TextAnimationHelper
{
    // Cache for active tweeners
    private static Dictionary<TextMeshProUGUI, Tweener> _activeTweens = new Dictionary<TextMeshProUGUI, Tweener>();
    private static Dictionary<Image, Tweener> _activeImageTweens = new Dictionary<Image, Tweener>();
    
    // Animate a text with counting effect and optional scale bounce
    public static void AnimateNumberText(TextMeshProUGUI textComponent, int fromValue, int toValue, string format = "{0}", bool scaleAtEnd = false)
    {
        if (textComponent == null) return;
        
        // Skip animation when going from high to low numbers
        bool skipAnimation = fromValue > toValue;
        
        // Default settings
        float countDuration = skipAnimation ? 0f : 0.5f;
        float scaleBounce = 1.2f;
        float scaleDuration = 0.3f;
        
        // Store original scale
        Vector3 originalScale = textComponent.transform.localScale;
        
        // Kill any existing animation for this specific text component
        if (_activeTweens.TryGetValue(textComponent, out Tweener existingTween))
        {
            existingTween.Kill();
            _activeTweens.Remove(textComponent);
        }
        
        // Set immediate value if skipping animation
        if (skipAnimation)
        {
            textComponent.text = string.Format(format, toValue);
            
            // Optional scale animation
            if (scaleAtEnd)
            {
                Sequence scaleSequence = DOTween.Sequence();
                scaleSequence.Append(textComponent.transform.DOScale(originalScale * scaleBounce, scaleDuration * 0.5f)
                    .SetEase(Ease.OutQuad));
                scaleSequence.Append(textComponent.transform.DOScale(originalScale, scaleDuration * 0.5f)
                    .SetEase(Ease.OutBack));
            }
            return;
        }
        
        // Create number counting animation
        Tweener countTween = DOTween.To(() => fromValue, (x) => {
            textComponent.text = string.Format(format, x);
        }, toValue, countDuration)
        .SetEase(Ease.OutQuad)
        .OnComplete(() => {
            // Optional scale animation when counting finishes
            if (scaleAtEnd)
            {
                Sequence scaleSequence = DOTween.Sequence();
                scaleSequence.Append(textComponent.transform.DOScale(originalScale * scaleBounce, scaleDuration * 0.5f)
                    .SetEase(Ease.OutQuad));
                scaleSequence.Append(textComponent.transform.DOScale(originalScale, scaleDuration * 0.5f)
                    .SetEase(Ease.OutBack));
            }
            
            // Remove from cache when complete
            if (_activeTweens.ContainsKey(textComponent))
            {
                _activeTweens.Remove(textComponent);
            }
        });
        
        // Cache the tweener
        _activeTweens[textComponent] = countTween;
    }
    
    // For fraction text like "5/10"
    public static void AnimateFractionText(TextMeshProUGUI textComponent, int fromValue, int toValue, int maxValue, bool scaleAtEnd = false)
    {
        AnimateNumberText(textComponent, fromValue, toValue, "{0}/" + maxValue, scaleAtEnd);
    }
    
    // Animate fill amount of an Image component
    public static void AnimateImageFill(Image imageComponent, float fromValue, float toValue, float duration = 0.5f, Ease easeType = Ease.OutQuad, bool skipDecrease = true)
    {
        if (imageComponent == null) return;
        
        // Skip animation when going from high to low if specified
        bool skipAnimation = skipDecrease && fromValue > toValue;
        
        // Kill any existing animation for this specific image component
        if (_activeImageTweens.TryGetValue(imageComponent, out Tweener existingTween))
        {
            existingTween.Kill();
            _activeImageTweens.Remove(imageComponent);
        }
        
        // Set immediate value if skipping animation
        if (skipAnimation)
        {
            imageComponent.fillAmount = toValue;
            return;
        }
        
        // Make sure the image is in filled mode
        if (imageComponent.type != Image.Type.Filled)
        {
            Debug.LogWarning("Image component should be set to Filled type for fill animation to work properly.");
            imageComponent.type = Image.Type.Filled;
        }
        
        // Set initial fill amount
        imageComponent.fillAmount = fromValue;
        
        // Create fill amount animation
        Tweener fillTween = imageComponent.DOFillAmount(toValue, duration)
            .SetEase(easeType)
            .OnComplete(() => {
                // Remove from cache when complete
                if (_activeImageTweens.ContainsKey(imageComponent))
                {
                    _activeImageTweens.Remove(imageComponent);
                }
            });
        
        // Cache the tweener
        _activeImageTweens[imageComponent] = fillTween;
    }
    
    // Animate a progress bar (normalized 0-1 values)
    public static void AnimateProgressBar(Image imageComponent, float currentProgress, float targetProgress, float duration = 0.5f, Ease easeType = Ease.OutQuad)
    {
        AnimateImageFill(imageComponent, currentProgress, targetProgress, duration, easeType);
    }
    
    // Animate a progress bar with actual values
    public static void AnimateProgressBar(Image imageComponent, float currentValue, float targetValue, float maxValue, float duration = 0.5f, Ease easeType = Ease.OutQuad)
    {
        float normalizedCurrent = Mathf.Clamp01(currentValue / maxValue);
        float normalizedTarget = Mathf.Clamp01(targetValue / maxValue);
        
        AnimateImageFill(imageComponent, normalizedCurrent, normalizedTarget, duration, easeType);
    }
    
    // Use this when components are being destroyed to avoid memory leaks
    public static void CleanupTweens(TextMeshProUGUI textComponent)
    {
        if (_activeTweens.TryGetValue(textComponent, out Tweener tween))
        {
            tween.Kill();
            _activeTweens.Remove(textComponent);
        }
    }
    
    public static void CleanupTweens(Image imageComponent)
    {
        if (_activeImageTweens.TryGetValue(imageComponent, out Tweener tween))
        {
            tween.Kill();
            _activeImageTweens.Remove(imageComponent);
        }
    }
}