using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using Match3.Core;

public class SettingsPopup : PopupBase
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Dropdown difficultyDropdown;
    [SerializeField] private Toggle muteMusic;
    [SerializeField] private Toggle muteSfx;
    [SerializeField] private Button closeButton;
    
    public override async UniTask Init(object data = null)
    {
        // Setup dropdown options from enum
        SetupDifficultyDropdown();
        
        // Load saved settings
        LoadSavedSettings();
        
        // Register UI event handlers
        RegisterUIListeners();
        
        await base.Init(data);
    }
    
    private void SetupDifficultyDropdown()
    {
        // Clear any existing options
        difficultyDropdown.ClearOptions();
        
        // Get all values from the DifficultyType enum
        string[] difficultyNames = Enum.GetNames(typeof(GameplaySettings.DifficultyType));
        
        // Format the enum values to be more readable (e.g., "Easy", "Normal", "Hard")
        var options = difficultyNames.Select(name => new TMP_Dropdown.OptionData(name)).ToList();
        
        // Add options to dropdown
        difficultyDropdown.AddOptions(options);
    }
    
    private void LoadSavedSettings()
    {
        // Load difficulty setting
        int savedDifficulty = PlayerPrefsHelper.GetIntValue(PlayerPrefsHelper.DIFFICULTY_KEY, PlayerPrefsHelper.DEFAULT_DIFFICULTY);
        difficultyDropdown.value = savedDifficulty;
        
        // Load audio settings
        bool musicMuted = PlayerPrefsHelper.GetBoolValue(PlayerPrefsHelper.MUSIC_MUTED_KEY, false);
        bool sfxMuted = PlayerPrefsHelper.GetBoolValue(PlayerPrefsHelper.SFX_MUTED_KEY, false);
        
        muteMusic.isOn = musicMuted;
        muteSfx.isOn = sfxMuted;
    }
    
    private void RegisterUIListeners()
    {
        // Dropdown changed event
        difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
        
        // Toggle changed events
        muteMusic.onValueChanged.AddListener(OnMusicMuteChanged);
        muteSfx.onValueChanged.AddListener(OnSfxMuteChanged);
        
        // Close button
        closeButton.onClick.AddListener(OnCloseButtonClicked);
    }
    
    private void OnDifficultyChanged(int difficultyIndex)
    {
        // Save difficulty setting
        PlayerPrefsHelper.SaveIntValue(PlayerPrefsHelper.DIFFICULTY_KEY, difficultyIndex);
        
        Debug.Log($"Difficulty changed to: {(GameplaySettings.DifficultyType)difficultyIndex}");
    }
    
    private void OnMusicMuteChanged(bool isMuted)
    {
        // Save music mute setting
        PlayerPrefsHelper.SaveBoolValue(PlayerPrefsHelper.MUSIC_MUTED_KEY, isMuted);

        Debug.Log($"Music muted: {isMuted}");
    }
    
    private void OnSfxMuteChanged(bool isMuted)
    {
        // Save SFX mute setting
        PlayerPrefsHelper.SaveBoolValue(PlayerPrefsHelper.SFX_MUTED_KEY, isMuted);
        
        Debug.Log($"SFX muted: {isMuted}");
    }
    
    private void OnCloseButtonClicked()
    {
        UnregisterUIListeners();
        base.ClosePopup();
    }
    
    private void UnregisterUIListeners()
    {
        difficultyDropdown.onValueChanged.RemoveListener(OnDifficultyChanged);
        muteMusic.onValueChanged.RemoveListener(OnMusicMuteChanged);
        muteSfx.onValueChanged.RemoveListener(OnSfxMuteChanged);
        closeButton.onClick.RemoveListener(OnCloseButtonClicked);
    }
    
    private void OnDestroy()
    {
        // Ensure cleanup if object is destroyed
        UnregisterUIListeners();
    }
}