using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ParticleSystemManager : BaseMono
{
    [SerializeField] private Transform particlesParentObject;
    
    private readonly Dictionary<AssetReference, GenericObjectPool<ParticleSystem>> _particlePools =
        new Dictionary<AssetReference, GenericObjectPool<ParticleSystem>>();

    private List<ParticleSystemSettingsConfig> _particleConfigs;
    private HashSet<string> _preloadedParticleKeys = new HashSet<string>();

    public override async UniTask Init(object data = null)
    {
        base.Init(data);

        if (data is List<ParticleSystemSettingsConfig> configs)
        {
            _particleConfigs = configs;
            await PreloadParticles(_particleConfigs);
        }

        MatchDetectorController.OnBallExplode += OnBallExplode;
    }

    public async UniTask PreloadParticles(List<ParticleSystemSettingsConfig> configs)
    {
        if (configs == null || configs.Count == 0) return;

        List<UniTask> preloadTasks = new List<UniTask>();

        foreach (var settings in configs)
        {
            if (settings == null || settings.ParticleReference == null)
                continue;
            
            string key = settings.ParticleReference.AssetGUID;
            if (_preloadedParticleKeys.Contains(key))
                continue;
            
            _preloadedParticleKeys.Add(key);
            preloadTasks.Add(PreloadParticlePool(settings));
        }

        await UniTask.WhenAll(preloadTasks);
    }

    private async UniTask PreloadParticlePool(ParticleSystemSettingsConfig settings)
    {
        if (_particlePools.ContainsKey(settings.ParticleReference)) return;

        // Create a pool for this particle type
        var pool = new GenericObjectPool<ParticleSystem>(
            settings.ParticleReference,
            settings.PoolSettingsConfig.PoolSize,
            particlesParentObject,  // Parent to manager to keep hierarchy clean
            settings.PoolSettingsConfig.Expandable,
            settings.PoolSettingsConfig.MaxPoolSize
        );

        await pool.InitializeAsync();
        _particlePools[settings.ParticleReference] = pool;

        // Preload objects in an inactive state
        for (int i = 0; i < settings.PoolSettingsConfig.PoolSize; i++)
        {
            var particle = await pool.GetAsync();
            if (particle != null)
            {
                particle.gameObject.SetActive(false);
                pool.Return(particle); // Immediately return to make it ready
            }
        }
    }

    private async void OnBallExplode(Ball obj)
    {
        await SpawnParticleAsync(obj.BallData.ParticleOnMatch, obj.transform.position);
    }

    private async UniTask<ParticleSystem> SpawnParticleAsync(ParticleSystemSettingsConfig settings, Vector3 position,
        Transform parent = null)
    {
        if (settings == null || settings.ParticleReference == null)
        {
            Debug.LogError("Invalid ParticleSystemSettingsConfig provided.");
            return null;
        }

        if (!_particlePools.TryGetValue(settings.ParticleReference, out var pool))
        {
            Debug.LogError($"No pool found for {settings.ParticleReference.RuntimeKey}. Make sure it's preloaded.");
            return null;
        }

        ParticleSystem particle = await pool.GetAsync();
        if (particle == null) return null;

        // Set position, parent, and scale
        particle.transform.position = position;
        if (parent != null) particle.transform.SetParent(parent);
        particle.transform.localScale = Vector3.one * settings.SpawnSize;

        // Activate and play particle
        particle.gameObject.SetActive(true);
        particle.Play();

        // Return the particle to the pool after it finishes
        ReturnToPoolAfterDelay(particle, pool, settings.RemoveAfterXSeconds).Forget();

        return particle;
    }

    private async UniTaskVoid ReturnToPoolAfterDelay(ParticleSystem particle, GenericObjectPool<ParticleSystem> pool,
        float delay)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay));

        if (particle != null)
        {
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particle.gameObject.SetActive(false);
            pool.Return(particle);
        }
    }

    private void ClearAllPools()
    {
        foreach (var pool in _particlePools.Values)
        {
            pool.Clear();
        }

        _particlePools.Clear();
    }

    private void OnDestroy()
    {
        MatchDetectorController.OnBallExplode -= OnBallExplode;
        ClearAllPools();
    }
}
