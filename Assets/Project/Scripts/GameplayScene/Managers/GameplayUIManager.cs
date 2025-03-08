using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class GameplayUIManager : BaseMono
{
    [Header("Score References")]
    [SerializeField] private Image scoreFillAmount;
    [SerializeField] private TextMeshProUGUI currentScoreText;

    [Header("Game Progress References")]
    [SerializeField] private TextMeshProUGUI tapsLeftText;
    [SerializeField] private TextMeshProUGUI timerText;
    
    [Header("Missed animator")]
    [SerializeField] private Animator missedAnimator;
    
    private Tweener _missTextTweener;
    private GlobalGameplayManagers _globalManagers;
    private int _lastScore = 0;

    public override async UniTask Init(object data = null)
    {
        if (data is GlobalGameplayManagers globalManagers)
        {
            _globalManagers = globalManagers;
            
            // Subscribe to events
            _globalManagers.GameplayManager.OnScoreChanged += UpdateScore;
            _globalManagers.GameplayManager.OnGameLost += HandleGameLost;
            _globalManagers.GameplayManager.OnTapsChanged += UpdateTapsLeft;
            _globalManagers.GameplayManager.OnTimeChanged += UpdateTimer;
            _globalManagers.GameplayManager.OnGameWon += HandleGameWon;
            _globalManagers.GameplayManager.OnMissed += OnMissed;
        }
        
        await base.Init(data);
    }

    private void OnMissed()
    {
        const string MISSED_CLIP_NAME = "Missed";
        missedAnimator.Play(MISSED_CLIP_NAME);
    }

    private void HandleGameWon()
    {
        // Show win popup
        _globalManagers.PopupsManager.ShowPopup(PopupsManager.PopupType.Win_Popup).Forget();
    }

    private void HandleGameLost()
    {
        // Show lose popup
        _globalManagers.PopupsManager.ShowPopup(PopupsManager.PopupType.Lose_Popup).Forget();
    }

    private void UpdateScore(int currentScore, int targetScore)
    {
        // Animate the text
        TextAnimationHelper.AnimateFractionText(currentScoreText, _lastScore, currentScore, targetScore, true);
    
        // Animate the fill
        TextAnimationHelper.AnimateProgressBar(
            scoreFillAmount,
            _lastScore,
            currentScore,
            targetScore,
            0.5f,
            Ease.OutQuad
        );
    
        _lastScore = currentScore;
    }

    private void UpdateTapsLeft(int currentTaps, int maxTaps)
    {
        tapsLeftText.text = $"{currentTaps}";
    }

    private void UpdateTimer(float currentTime, float maxTime)
    {
        if (maxTime >= 60f)
        {
            // Format as minutes:seconds with proper padding
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
        else
        {
            // Format as seconds with one decimal point
            timerText.text = currentTime.ToString("0.0") + "s";
        }
    }


    // Cleanup method to stop any ongoing tweens
    private void OnDestroy()
    {
        _missTextTweener?.Kill();
        
        if (_globalManagers != null && _globalManagers.GameplayManager != null)
        {
            _globalManagers.GameplayManager.OnScoreChanged -= UpdateScore;
            _globalManagers.GameplayManager.OnGameLost -= HandleGameLost;
            _globalManagers.GameplayManager.OnTapsChanged -= UpdateTapsLeft;
            _globalManagers.GameplayManager.OnTimeChanged -= UpdateTimer;
            _globalManagers.GameplayManager.OnGameWon -= HandleGameWon;
            _globalManagers.GameplayManager.OnMissed -= OnMissed;
        }
    }
}