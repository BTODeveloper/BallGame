using Cysharp.Threading.Tasks;
using UnityEngine;

public class GlobalGameplayManagers : BaseMono
{
    // Serialized references to all managers with public getters for easy access
    [field: SerializeField] public GameplayManager GameplayManager { get; private set; }
    [field: SerializeField] public GameplayUIManager GameplayUIManager { get; private set; }
    [field: SerializeField] public ParticleSystemManager ParticleSystemManager { get; private set; }
    [field: SerializeField] public BallManager BallManager { get; private set; }


    public override async UniTask Init(object data = null)
    {
        base.Init(data);

        await GameplayUIManager.Init(this);
        await GameplayManager.Init(this);
        await ParticleSystemManager.Init(this);
        await BallManager.Init(this);
    }
}




















