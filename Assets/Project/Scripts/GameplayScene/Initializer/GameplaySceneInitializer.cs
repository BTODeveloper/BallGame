using Match3.Core;
using UnityEngine;

public class GameplaySceneInitializer : MonoBehaviour
{
    [SerializeField] private BaseMono entryPoint;

    private async void Start()
    {
        // Initialize the main manager/entry point
        await entryPoint.Init();
    }
}
