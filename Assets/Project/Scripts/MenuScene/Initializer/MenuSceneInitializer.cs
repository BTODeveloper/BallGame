using Match3.Core;
using UnityEngine;

public class MenuSceneInitializer : MonoBehaviour
{
    [SerializeField] private BaseMono[] baseMonos;

    private async void Start()
    {
        foreach (var mono in baseMonos)
        {
            await mono.Init();
        }
    }
}
