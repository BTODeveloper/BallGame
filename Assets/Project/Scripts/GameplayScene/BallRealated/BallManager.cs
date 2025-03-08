using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Match3.Gameplay;
using UnityEngine;

public class BallManager : BaseMono
{
    public static event Action OnAnyBallTap;
    public static event Action<int> OnNewScoreCalculated;
    [SerializeField] private BallSpawnerController ballSpawner;
    [SerializeField] private MatchDetectorController matchDetector;
    private GlobalGameplayManagers _globalManagers;

    public override async UniTask Init(object data = null)
    {
        if (data is GlobalGameplayManagers globalManagers)
        {
            _globalManagers = globalManagers;
            
            globalManagers.GameplayManager.OnGameLost += RemoveAllBalls;
            globalManagers.GameplayManager.OnGameWon += RemoveAllBalls;
            ballSpawner.OnBallClicked += HandleBallTapped;
        }

        await base.Init(data);
        await matchDetector.Init();
        await ballSpawner.Init(_globalManagers);
    }

    private void RemoveAllBalls()
    {
        if (ballSpawner.AllActiveBalls?.Count > 0)
        {
            // Create a copy of the active balls to avoid collection modification errors
            List<Ball> ballsToExplode = new List<Ball>(ballSpawner.AllActiveBalls);
        
            // Create a sequence for cascading explosions
            Sequence explosionSequence = DOTween.Sequence();
            float delayBetweenExplosions = 0.02f; // 50ms between explosions
        
            // Add each ball explosion to the sequence with slight delay
            for (int i = 0; i < ballsToExplode.Count; i++)
            {
                Ball currentBall = ballsToExplode[i];
            
                // Add a callback to explode this specific ball
                explosionSequence.AppendCallback(() => {
                    // Create a temp list with just this one ball
                    List<Ball> singleBall = new List<Ball> { currentBall };
                    matchDetector.ExplodeBalls(singleBall, ballSpawner);
                });
            
                // Add a small delay before the next explosion
                explosionSequence.AppendInterval(delayBetweenExplosions);
            }
        
            // Play the sequence
            explosionSequence.Play();
        }
    }
    
    private void HandleBallTapped(Ball ball)
    {
        List<Ball> matchingBalls;
    
        // Handle different types of ball interactions
        if (ball.BallGroupConfiguration.BallRootType == BallRootType.Special)
        {
            // Get all balls in explosion radius for special balls
            float explosionRadius = ball.BallGroupConfiguration.DetectionRadius;
            matchingBalls = matchDetector.FindBallsInRadius(ball.transform.position, explosionRadius);
            Debug.Log($"Special ball exploded {matchingBalls.Count} balls!");
        }
        else
        {
            // For regular balls, find matching color group
            matchingBalls = matchDetector.FindColorMatchingGroup(ball);
        }

        // Only proceed if we have at least 3 balls (match-3 logic)
        // Special balls can explode any number of balls
        if (matchingBalls.Count >= 3 || ball.BallGroupConfiguration.BallRootType == BallRootType.Special)
        {
            int groupSize = matchingBalls.Count;

            // Explode the balls
            matchDetector.ExplodeBalls(matchingBalls, ballSpawner);
            
            int score = CalculateScore(groupSize);
            OnNewScoreCalculated?.Invoke(score);

            // Check if we need to spawn a special ball
            const int MIN_FOR_SPECIAL_BALL = 1; 
            if (groupSize > MIN_FOR_SPECIAL_BALL)
            {
                // Replace with one special ball and the rest normal
                ballSpawner.SpawnReplacementBalls(groupSize, BallRootType.Special).Forget();
            }
            else
            {
                // Just spawn regular replacement balls
                ballSpawner.SpawnReplacementBalls(groupSize).Forget();
            }
        }
        
        OnAnyBallTap?.Invoke();
    }

    private int CalculateScore(int groupSize)
    {
        int pointsPerBall = 1;

        if (groupSize > 20)
            pointsPerBall = 4;
        else if (groupSize > 10)
            pointsPerBall = 2;

        return groupSize * pointsPerBall;
    }

    private void OnDestroy()
    {
        if (ballSpawner != null)
            ballSpawner.OnBallClicked -= HandleBallTapped;

        if (_globalManagers != null)
        {
            _globalManagers.GameplayManager.OnGameLost -= RemoveAllBalls;
            _globalManagers.GameplayManager.OnGameWon -= RemoveAllBalls;
        }
    }
}