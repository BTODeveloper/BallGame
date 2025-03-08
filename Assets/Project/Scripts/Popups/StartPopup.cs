using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class StartPopup : PopupBase
{
    [Header("Buttons")]
    [SerializeField] private Button startGameButton;
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
        startGameButton?.onClick.AddListener(HandleStartGame);
    }

    private async void HandleStartGame()
    {
         base.ClosePopup();
    }

    private void HandleBackToMenu()
    {
        base.ClosePopup();
        _sceneLoaderManager.LoadSceneAsync(SceneLoaderManager.SceneTypes.MenuScene).Forget();
    }
}
