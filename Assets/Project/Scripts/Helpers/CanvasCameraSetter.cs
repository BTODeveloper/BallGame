using Cysharp.Threading.Tasks;
using UnityEngine;

public class CanvasCameraSetter : BaseMono
{
    private Canvas _canvas;

    private async void Start()
    {
        await Init();
    }

    public override async UniTask Init(object data = null)
    {
        var managers = GlobalManagers.Instance;
        if(managers != null)
        {
            _canvas = GetComponent<Canvas>();
            _canvas.worldCamera = managers.Camera;
        }
        
        await base.Init(data);
    }
}
