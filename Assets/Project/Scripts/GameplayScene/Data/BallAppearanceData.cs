using UnityEngine;

namespace Match3.Gameplay
{
    [CreateAssetMenu(fileName = "BallPersistentData", menuName = "Match3/BallPersistentData")]
    public class BallAppearanceData : ScriptableObject
    {
        [field: SerializeField] public BallColorType BallColorType { get; private set; }
        [field: SerializeField] public Sprite Sprite { get; private set; }
        [field: SerializeField] public ParticleSystemSettingsConfig ParticleOnMatch { get; private set; }
    }
}