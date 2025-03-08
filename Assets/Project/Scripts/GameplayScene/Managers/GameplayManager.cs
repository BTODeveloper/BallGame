using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Match3.Core;

public class GameplayManager : BaseMono
{
    public bool  IsGameActive { get; private set; }

    public event Action<int, int> OnScoreChanged;    // current, target
    public event Action<int, int> OnTapsChanged;     // current, max
    public event Action<float, float> OnTimeChanged; // current, max
    public event Action OnGameStarted;
    public event Action OnGameWon;
    public event Action OnGameLost;
    public event Action OnMissed;

    [SerializeField] private DifficultyManager difficultyManager;

    private GameplaySettings _currentSettings;
    private GlobalGameplayManagers _globalGameplayManagers;
    private GlobalManagers _globalManagers;

    // Game state
    private int _currentScore = 0;
    private int _tapsLeft;
    private float _timeRemaining;

    public override async UniTask Init(object data = null)
    {
        _globalManagers = GlobalManagers.Instance;
        
        if (data is GlobalGameplayManagers globalManagers)
        {
            _globalGameplayManagers = globalManagers;
        }
        
        RegisterToEvents();
        await base.Init(data);
        
        //Set diffuclity manager and grab it when finished
        await difficultyManager.Init();
        SetGameplaySettings(difficultyManager.CurrentGameplaySettings);

        //Awaits for start popup to show
        await ShowStartPopup();
    }

    private void RegisterToEvents()
    {
        BallManager.OnNewScoreCalculated += OnNewScoreCalculated;
        BallManager.OnAnyBallTap += DecrementTaps;
        MatchDetectorController.OnMissed += OnMissed;
    }

    private void OnNewScoreCalculated(int addedScore)
    {
        if (!IsGameActive) return;

        // Store previous score to check if it changed
        int previousScore = _currentScore;
    
        // Add score
        _currentScore += addedScore;

        // Clamp to target score
        _currentScore = Mathf.Min(_currentScore, _currentSettings.TargetScore);

        // Only notify listeners if the score actually changed
        if (_currentScore != previousScore)
        {
            OnScoreChanged?.Invoke(_currentScore, _currentSettings.TargetScore);
        }
    }

    private async UniTask ShowStartPopup()
    {
        var startPopup = await _globalManagers.PopupsManager.ShowPopup(PopupsManager.PopupType.Start_Popup);
        if (startPopup != null)
        {
           startPopup.OnPopupClosed += StartPopupOnOnPopupClosed;
        }
    }

    //Start popup closed = game start
    private void StartPopupOnOnPopupClosed(PopupBase popupBase)
    {
        popupBase.OnPopupClosed -= StartPopupOnOnPopupClosed;
        
        OnGameStarted?.Invoke();
        IsGameActive = true;
    }

    private void SetGameplaySettings(GameplaySettings settings)
    {
        if (settings != null)
        {
            _currentSettings = settings;
            Debug.Log($"Injected gameplay settings for {_currentSettings.Difficulty} difficulty");
        }

        // Reset the game state with the new settings
        ResetGameState();
    }
    
    private void ResetGameState()
    {
        _currentScore = 0;
        _tapsLeft = _currentSettings.TapsLimit;
        _timeRemaining = _currentSettings.GameDuration;
        IsGameActive = false;

        // Notify listeners with both current and max values
        OnScoreChanged?.Invoke(_currentScore, _currentSettings.TargetScore);
        OnTapsChanged?.Invoke(_tapsLeft, _currentSettings.TapsLimit);
        OnTimeChanged?.Invoke(_timeRemaining, _currentSettings.GameDuration);
    }
    

    protected override void Update()
    {
        if (!IsGameActive) return;

        // Update timer
        _timeRemaining -= Time.deltaTime;
        OnTimeChanged?.Invoke(_timeRemaining, _currentSettings.GameDuration);

        // Check for time-based end condition
        if (_timeRemaining <= 0)
        {
            EndGame();
        }
    }
    private void DecrementTaps()
    {
        if (!IsGameActive) return;

        _tapsLeft--;
        OnTapsChanged?.Invoke(_tapsLeft, _currentSettings.TapsLimit);

        // Check for tap-based end condition
        if (_tapsLeft <= 0)
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        IsGameActive = false;
    
        // Save high score if we've beaten the previous record
        SaveHighScore();
    
        // Check win/lose condition according to requirements:
        // Win: Reach target score AND THEN run out of taps/time
        // Lose: Run out of taps/time WITHOUT reaching target score
        if (_currentScore >= _currentSettings.TargetScore)
        {
            // Player reached target score and now ran out of taps/time = WIN
            OnGameWon?.Invoke();
        }
        else
        {
            // Player ran out of taps/time without reaching target score = LOSE
            OnGameLost?.Invoke();
        }
    }

    private void SaveHighScore()
    {
        int currentHighScore = PlayerPrefsHelper.GetIntValue(PlayerPrefsHelper.HIGH_SCORE_KEY, 0);
    
        if (_currentScore > currentHighScore)
        {
            // New high score achieved
            PlayerPrefsHelper.SaveIntValue(PlayerPrefsHelper.HIGH_SCORE_KEY, _currentScore);
            Debug.Log($"New high score saved: {_currentScore}");
        }
    }
    
    private void OnDestroy()
    {
        BallManager.OnAnyBallTap -= DecrementTaps;
        MatchDetectorController.OnMissed -= OnMissed;
        BallManager.OnNewScoreCalculated -= OnNewScoreCalculated;
    }
}