using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LostPopup : PopupBase
{
    [Header("Buttons")]
    [SerializeField] private Button replayButton;
    [SerializeField] private Button backToMenuButton;
    private SceneLoaderManager _sceneLoaderManager;
    
    public override async UniTask Init(object data = null)
    {
        if (GlobalManagers.Instance != null)
        {
            _sceneLoaderManager = GlobalManagers.Instance.SceneLoaderManager;
        }
        
        await base.Init(data);
        InitializeButtons();
    }
    
    private void InitializeButtons()
    {
        backToMenuButton?.onClick.AddListener(HandleBackToMenu);
        replayButton?.onClick.AddListener(HandleReplay);
    }

    private void HandleReplay()
    {
        base.ClosePopup();
        
        _sceneLoaderManager.LoadSceneAsync(SceneLoaderManager.SceneTypes.GameplayScene).Forget();
    }

    private void HandleBackToMenu()
    {
        base.ClosePopup();
        
        _sceneLoaderManager.LoadSceneAsync(SceneLoaderManager.SceneTypes.MenuScene).Forget();
    }
}
