using Cysharp.Threading.Tasks;
using Match3.Core;
using UnityEngine;

public class ApplicationFrameRateSetter : BaseMono
{
    public override async UniTask Init(object data = null)
    { 
        await base.Init(data);
        Application.targetFrameRate = 60;
    }
}
