using UnityEngine;
using Cysharp.Threading.Tasks;

public abstract class BaseMono : MonoBehaviour
{
    public virtual UniTask Init(object data = null)
    {
        return UniTask.CompletedTask;
    }


    protected virtual void Update() { }
}