using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public class ParticleSystemSettingsConfig
{
    [field: SerializeField] public float SpawnSize { get; private set; } = 1f;
    [field: SerializeField] public float RemoveAfterXSeconds { get; private set; } = 3f;
    [field: SerializeField] public AssetReference ParticleReference{ get; private set; }
    [field: SerializeField] public PoolSettingsConfig PoolSettingsConfig{ get; private set; }
}
