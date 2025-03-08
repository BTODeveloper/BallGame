using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Match3.Gameplay
{
    [CreateAssetMenu(fileName = "BallTypeConfig", menuName = "Match3/BallTypeConfig")]
    public class BallGroupConfiguration  : ScriptableObject
    {
        [field: SerializeField] public BallRootType BallRootType { get; private set; }
        [field: SerializeField] public AssetReference BallPrefab { get; private set; }

        [field: SerializeField] public float DetectionRadius { get; private set; } = 0.3f;
        [field: SerializeField] public List<BallAppearanceData > VisualDatas { get; private set; }
        [field: SerializeField] public PoolSettingsConfig PoolSettingsConfig { get; private set; }
    }
}