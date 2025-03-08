using Cysharp.Threading.Tasks;
using UnityEngine;

// Central access point for game-wide systems and managers
// Lightweight singleton pattern that avoids dependency injection frameworks
// while still providing easy access to critical game systems
public class GlobalManagers : BaseMono
{
    public static GlobalManagers Instance { get; private set; }
    
    [field: SerializeField] public Camera Camera { get; private set; }
    [field: SerializeField] public PopupsManager PopupsManager { get; private set; }
    [field: SerializeField] public SceneLoaderManager SceneLoaderManager { get; private set; }
    
    public override async UniTask Init(object data = null)
    { 
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
            
        await base.Init(data);
    }
}