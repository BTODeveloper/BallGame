using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Match3.Gameplay;
using UnityEngine;

public class MatchDetectorController : BaseMono
{
    public static event Action<Ball> OnBallExplode;
    public static event Action OnMissed;

    private const string BallLayerName = "Ball";

    [Header("Debug Visualization")] [SerializeField]
    private bool showDebugGizmos = true;

    [SerializeField] private Color debugGizmoColor = new Color(0, 1, 0, 0.3f);

    private readonly HashSet<Ball> visitedBalls = new HashSet<Ball>();
    private int _ballLayerMask;
    private Collider2D[] _colliderBuffer;
    private Vector3 _lastTappedPosition; // Stores last click position
    private float _lastTappedRadius;

    public override async UniTask Init(object data = null)
    {
        await base.Init(data);

        // Cache the layer mask
        _ballLayerMask = LayerMask.GetMask(BallLayerName);

        // Pre-allocate the collider buffer
        _colliderBuffer = new Collider2D[60]; // Max balls count
    }

    // Generic method to find any matching group based on provided match criteria
    private List<Ball> FindMatchingGroup(Ball startBall, System.Predicate<Ball> matchCriteria)
    {
        if (startBall == null || matchCriteria == null)
            return new List<Ball>();

        // Store for gizmo debugging
        var detectionRadius = startBall.BallGroupConfiguration.DetectionRadius;

        CacheValuesForGizmos(startBall.transform.position, detectionRadius);

        // Reset state for new search
        visitedBalls.Clear();
        List<Ball> matchingGroup = new List<Ball>();

        // Breadth-first search for connected matching balls
        Queue<Ball> ballsToCheck = new Queue<Ball>();
        ballsToCheck.Enqueue(startBall);
        visitedBalls.Add(startBall);

        while (ballsToCheck.Count > 0)
        {
            Ball currentBall = ballsToCheck.Dequeue();
            matchingGroup.Add(currentBall);

            // Find all nearby balls using non-allocating method
            int hitCount = Physics2D.OverlapCircleNonAlloc(
                currentBall.transform.position,
                detectionRadius,
                _colliderBuffer,
                _ballLayerMask
            );

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D collider = _colliderBuffer[i];
                Ball nearbyBall = collider.GetComponent<Ball>();

                // Skip if null or already visited
                if (nearbyBall == null || visitedBalls.Contains(nearbyBall))
                    continue;

                // Use the provided match criteria to determine if balls match
                if (!matchCriteria(nearbyBall))
                    continue;

                visitedBalls.Add(nearbyBall);
                ballsToCheck.Enqueue(nearbyBall);
            }
        }

        return matchingGroup;
    }

    // Convenience method for color matching
    public List<Ball> FindColorMatchingGroup(Ball startBall)
    {
        if (startBall == null || startBall.BallData == null)
            return new List<Ball>();

        BallColorType colorToMatch = startBall.BallData.BallColorType;

        var matchingGroup = FindMatchingGroup(startBall, ball =>
            ball.BallData != null && ball.BallData.BallColorType == colorToMatch);

        // Check if this is a miss (less than 3 matching balls)
        if (matchingGroup.Count < 3)
        {
            OnMissed?.Invoke();
        }

        return matchingGroup;
    }

    // Generic radius-based finder
    public List<Ball> FindBallsInRadius(Vector3 center, float radius)
    {
        CacheValuesForGizmos(center, radius);

        List<Ball> ballsInRadius = new List<Ball>();

        // Use non-allocating OverlapCircleNonAlloc
        int hitCount = Physics2D.OverlapCircleNonAlloc(
            center,
            radius,
            _colliderBuffer,
            _ballLayerMask
        );

        for (int i = 0; i < hitCount; i++)
        {
            Ball ball = _colliderBuffer[i].GetComponent<Ball>();
            if (ball != null)
            {
                ballsInRadius.Add(ball);
            }
        }

        return ballsInRadius;
    }

    private void CacheValuesForGizmos(Vector3 pos, float detectionRadius)
    {
        _lastTappedPosition = pos;
        _lastTappedRadius = detectionRadius;
    }

    // Explode a list of balls
    public void ExplodeBalls(List<Ball> balls, BallSpawnerController ballSpawner)
    {
        if (balls == null || balls.Count == 0) return;

        foreach (Ball ball in balls)
        {
            OnBallExplode?.Invoke(ball);
            ballSpawner.RemoveBall(ball);
        }
    }

    // Draw debug visualization
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        Gizmos.color = debugGizmoColor;

        // Draw the last clicked detection radius even if the ball was reset
        Gizmos.DrawWireSphere(_lastTappedPosition, _lastTappedRadius);
    }
}