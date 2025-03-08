using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Match3.Gameplay;

public class BallSpawnerController : BaseMono
{
    public event Action<Ball> OnBallClicked;
    public List<Ball> AllActiveBalls { get; private set; }

    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform spawnerParent;

    [Header("Init Spawn Settings")] [SerializeField]
    private int initialGridRows = 6; // How many rows to fill at the start

    [SerializeField] private int initialGridColumns = 6;
    [SerializeField] private float ballSpacing = 1.2f;

    [Header("Spawner Settings")] [SerializeField]
    private float spawnWidth = 5f;

    [Header("Balls Datas")] [SerializeField]
    private AssetReference[] ballConfigRefs;

    private const int MAX_ACTIVE_BALLS = 60;

    private readonly Dictionary<BallRootType, BallGroupConfiguration> _ballConfigs = new();
    private readonly Dictionary<BallRootType, GenericObjectPool<Ball>> _ballPools = new();
    private GlobalGameplayManagers _globalGameplayManagers;


    public override async UniTask Init(object data = null)
    {
        if (data is GlobalGameplayManagers globalManagers)
        {
            _globalGameplayManagers = globalManagers;
        }

        AllActiveBalls = new List<Ball>();
        
        await base.Init(data);
        await LoadBallConfigurations();
        await InitializePools(); // Ensure pools are ready before spawning balls
        SpawnInitialGrid().Forget(); // Spawn balls AFTER pools are confirmed ready
    }


    private async UniTask LoadBallConfigurations()
    {
        foreach (AssetReference configRef in ballConfigRefs)
        {
            BallGroupConfiguration config = await Addressables.LoadAssetAsync<BallGroupConfiguration>(configRef).Task;
            if (config != null && !_ballConfigs.ContainsKey(config.BallRootType))
            {
                Debug.Log($"Loaded Ball Config: {config.BallRootType}");
                _ballConfigs[config.BallRootType] = config;
            }
        }
    }

    //We first load and init all of the possible balls groups (curr regular and special) 
    private async UniTask InitializePools()
    {
        List<UniTask> poolInitTasks = new();

        // Collect all unique particle configs for preloading
        HashSet<string> uniqueParticleKeys = new HashSet<string>();
        List<ParticleSystemSettingsConfig> uniqueParticleConfigs = new List<ParticleSystemSettingsConfig>();

        foreach (var config in _ballConfigs.Values)
        {
            if (config.BallPrefab == null)
            {
                Debug.LogError($"No prefab assigned for {config.BallRootType} in BallGroupConfiguration!");
                continue;
            }

            var pool = new GenericObjectPool<Ball>(
                config.BallPrefab,
                config.PoolSettingsConfig.PoolSize,
                spawnerParent,
                config.PoolSettingsConfig.Expandable,
                config.PoolSettingsConfig.MaxPoolSize
            );

            _ballPools[config.BallRootType] = pool;
            poolInitTasks.Add(pool.InitializeAsync());

            // Collect particle configs from each visual data
            foreach (var visualData in config.VisualDatas)
            {
                if (visualData.ParticleOnMatch != null)
                {
                    string key = visualData.ParticleOnMatch.ParticleReference.AssetGUID;
                    if (!uniqueParticleKeys.Contains(key))
                    {
                        uniqueParticleKeys.Add(key);
                        uniqueParticleConfigs.Add(visualData.ParticleOnMatch);
                    }
                }
            }
        }

        // Wait for ball pools to initialize
        await UniTask.WhenAll(poolInitTasks);

        // Preload all unique particle effects at once
        if (uniqueParticleConfigs.Count > 0)
        {
            Debug.Log($"Preloading {uniqueParticleConfigs.Count} unique particle effects");
            await _globalGameplayManagers.ParticleSystemManager.PreloadParticles(uniqueParticleConfigs);
        }
    }

    //Spawning in a grid visual style
    private async UniTask SpawnInitialGrid()
    {
        float startX = spawnPoint.position.x - (initialGridColumns / 2f) * ballSpacing;
        float microDelay = 0.002f; // 2 milliseconds delay
    
        for (int row = 0; row < initialGridRows; row++)
        {
            for (int col = 0; col < initialGridColumns; col++)
            {
                if (AllActiveBalls.Count >= MAX_ACTIVE_BALLS)
                {
                    Debug.Log($"Reached maximum of {MAX_ACTIVE_BALLS} balls, stopping spawn");
                    return;
                }

                float x = startX + col * ballSpacing;
                float y = spawnPoint.position.y + (row * ballSpacing * 1.5f);
            
                BallRootType randomType = GetRandomBallType();
                SpawnBallAtPosition(randomType, new Vector3(x, y, 0)).Forget();
            
                // Add a tiny delay between each ball spawn
                await UniTask.Delay(TimeSpan.FromSeconds(microDelay));
            }
        }
    }
    
    public UniTask SpawnReplacementBalls(int count, BallRootType? forceTypeForFirst = null)
    {
        Debug.Log(
            $"Trying to spawn {count} replacement balls{(forceTypeForFirst.HasValue ? $" with first ball of type {forceTypeForFirst}" : "")}");

        // Check pool availability first
        foreach (var poolEntry in _ballPools)
        {
            Debug.Log(
                $"Pool for {poolEntry.Key}: Available: {poolEntry.Value.AvailableCount}, Total: {poolEntry.Value.TotalCount}");
        }

        // Adaptive spacing - reduce spacing when too many balls
        float maxWidth = spawnWidth; // Use the serialized spawnWidth to define play area
        float adaptiveSpacing = ballSpacing;

        // If balls would exceed the width, reduce the spacing accordingly
        if (count * ballSpacing > maxWidth)
        {
            adaptiveSpacing = maxWidth / count;
            Debug.Log($"Adapting ball spacing from {ballSpacing} to {adaptiveSpacing} for {count} balls");
        }

        // Calculate positioning with adapted spacing
        float startX = spawnPoint.position.x - (count * adaptiveSpacing / 2f);

        for (int i = 0; i < count && AllActiveBalls.Count < MAX_ACTIVE_BALLS; i++)
        {
            // Evenly distribute across the width with adaptive spacing
            float x = startX + (i * adaptiveSpacing);

            // Clamp position to ensure it stays within boundaries
            x = Mathf.Clamp(x, spawnPoint.position.x - (maxWidth / 2f), spawnPoint.position.x + (maxWidth / 2f));

            // Add some vertical offset for visual variety
            float yOffset = (i % 3) * adaptiveSpacing * 0.5f;
            float y = spawnPoint.position.y + yOffset;

            // Determine type: forced type for first ball if specified, otherwise random
            BallRootType ballType;
            if (i == 0 && forceTypeForFirst.HasValue)
            {
                ballType = forceTypeForFirst.Value;
            }
            else
            {
                ballType = GetRandomBallType();
            }

            Debug.Log($"Attempting to spawn ball type: {ballType} at position ({x}, {y})");
            SpawnBallAtPosition(ballType, new Vector3(x, y, 0)).Forget();
        }

        return UniTask.CompletedTask;
    }

    private async UniTask<Ball> SpawnBallAtPosition(BallRootType ballType, Vector3 position)
    {
        if (!_ballConfigs.TryGetValue(ballType, out var config) ||
            !_ballPools.TryGetValue(ballType, out var pool))
            return null;

        Ball ball = await pool.GetAsync();
        if (ball != null)
        {
            Debug.Log("SpawnBallAtPosition same count?");

            ball.OnBallClicked += HandleBallTapped;

            int randomIndex = UnityEngine.Random.Range(0, config.VisualDatas.Count);
            ball.transform.position = position;

            var initData = new BallRuntimeDataWrapper(config, config.VisualDatas[randomIndex], _globalGameplayManagers.GameplayManager);
            await ball.Init(initData); // Wait for initialization to complete

           // ball.ResetState();
            ball.gameObject.SetActive(true); // Ensure it's active

            AllActiveBalls.Add(ball);
            return ball;
        }

        return null;
    }

    private BallRootType GetRandomBallType()
    {
        // Same as GetRandomBallType but filter out special balls
        List<BallRootType> availableTypes = new List<BallRootType>();

        foreach (var entry in _ballPools)
        {
            if (entry.Value.AvailableCount > 0 && entry.Key != BallRootType.Special)
            {
                availableTypes.Add(entry.Key);
            }
        }

        if (availableTypes.Count == 0)
        {
            Debug.LogWarning("No regular ball types available in any pool!");
            return BallRootType.Regular; // Fallback to regular
        }

        return availableTypes[UnityEngine.Random.Range(0, availableTypes.Count)];
    }

    public void RemoveBall(Ball ball)
    {
        if (ball == null || !AllActiveBalls.Contains(ball)) return;

        ball.OnBallClicked -= HandleBallTapped;
        AllActiveBalls.Remove(ball);

        if (_ballPools.TryGetValue(ball.BallGroupConfiguration.BallRootType, out var pool))
        {
            pool.Return(ball);
        }
    }

    private void HandleBallTapped(Ball ball, BallColorType type)
    {
        OnBallClicked?.Invoke(ball);
    }
}