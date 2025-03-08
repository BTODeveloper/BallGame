using Cysharp.Threading.Tasks;
using Match3.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuUIManager : BaseMono
{
    [SerializeField] private TextMeshProUGUI highestScoreText;
    [SerializeField] private Button playBtn;
    [SerializeField] private Button settingsBtn;
    private GlobalManagers _globalManagers;
    
    private int _displayedHighScore = 0;

    public override async UniTask Init(object data = null)
    {
        _globalManagers = GlobalManagers.Instance;
        
        // Register button click handlers
        RegisterButtonListeners();
        
        // Update high score display
        UpdateHighScoreDisplay();

        await base.Init(data);
    }
    
    private void OnEnable()
    {
        // Update high score display whenever the menu becomes active
        UpdateHighScoreDisplay();
    }

    private void RegisterButtonListeners()
    {
        playBtn.onClick.AddListener(OnPlayBtnClicked);
        settingsBtn.onClick.AddListener(OnSettingsBtnClicked);
    }
    
    private void UpdateHighScoreDisplay()
    {
        if (highestScoreText != null)
        {
            // Get the current high score from PlayerPrefs
            int highScore = PlayerPrefsHelper.GetIntValue(PlayerPrefsHelper.HIGH_SCORE_KEY, 0);
            
            // Use proper grammar in the display text
            string displayFormat = highScore == 1 ? "Highest Score: {0} point" : "Highest Score: {0}";
            
            // Use the text animation helper to animate the score change
            TextAnimationHelper.AnimateNumberText(highestScoreText, _displayedHighScore, highScore, displayFormat, true);
            
            // Update the cached displayed score
            _displayedHighScore = highScore;
        }
    }

    private void OnPlayBtnClicked()
    {
        if (_globalManagers != null)
        {
            _globalManagers.SceneLoaderManager.LoadSceneAsync(SceneLoaderManager.SceneTypes.GameplayScene).Forget();
        }
    }

    private async void OnSettingsBtnClicked()
    {
        if (_globalManagers != null && _globalManagers.PopupsManager != null)
        {
            void SetSettingsRaycast(bool state)
            {
                settingsBtn.image.raycastTarget = state;
            }
            
            //Should be a generic solution with button base
            SetSettingsRaycast(false);
            
            var popup = await _globalManagers.PopupsManager.ShowPopup(PopupsManager.PopupType.Settings_Popup);
            if (popup != null)
            {
                popup.OnPopupClosed += OnPopupClosed;

                void OnPopupClosed(PopupBase obj)
                {
                    SetSettingsRaycast(true);
                }
            }
            else
            {
                SetSettingsRaycast(true);
            }
        }
        else
        {
            Debug.LogError("Cannot show settings popup - PopupsManager not available");
        }
    }

    private void OnDestroy()
    {
        if (highestScoreText != null)
        {
            TextAnimationHelper.CleanupTweens(highestScoreText);
        }
        
        playBtn.onClick.RemoveListener(OnPlayBtnClicked);
        settingsBtn.onClick.RemoveListener(OnSettingsBtnClicked);
    }
}