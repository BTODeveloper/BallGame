using System;
using UnityEngine;

[Serializable]
public class PoolSettingsConfig 
{
    [field: SerializeField] public bool Expandable { get; private set; } = true;
    [field: SerializeField] public int PoolSize { get; private set; } = 10;
    [field: SerializeField] public int MaxPoolSize { get; private set; } = 60;
}
