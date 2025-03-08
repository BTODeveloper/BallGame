using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Match3.Gameplay;


public class Ball : BaseMono, IPoolable
{
    public event Action<Ball, BallColorType> OnBallClicked;
    public BallGroupConfiguration BallGroupConfiguration { get; private set; }
    public BallAppearanceData BallData { get; private set; }

    [SerializeField] private SpriteRenderer renderer;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private CircleCollider2D circleCollider;

    private GameplayManager _gameplayManager;

    public override UniTask Init(object data = null)
    {
        if (data is BallRuntimeDataWrapper ballDataWrapper)
        {
            _gameplayManager = ballDataWrapper.GameplayManager;
            BallData = ballDataWrapper.BallAppearanceData;
            BallGroupConfiguration = ballDataWrapper.BallGroupConfiguration;

            SetSprite();
        }

        return UniTask.CompletedTask;
    }

    private void SetSprite()
    {
        renderer.sprite = BallData.Sprite;
    }

    private void OnMouseDown()
    {
        if (_gameplayManager != null && _gameplayManager.IsGameActive)
            OnBallClicked?.Invoke(this, BallData.BallColorType);
    }

    public void OnGetFromPool()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = true;
        circleCollider.enabled = true;
    }

    public void OnReturnToPool()
    {
        rb.simulated = false;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        circleCollider.enabled = false;
        OnBallClicked = null;
    }
}