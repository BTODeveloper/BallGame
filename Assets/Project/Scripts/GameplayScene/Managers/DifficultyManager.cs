using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Match3.Core;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class DifficultyManager : BaseMono
{
   public GameplaySettings CurrentGameplaySettings { get; private set; }
   
   // References to different difficulty ScriptableObjects in the Addressables system
   [SerializeField] private List<AssetReference> difficultySettingsRefs = new List<AssetReference>();

   public override async UniTask Init(object data = null)
   {
       await base.Init(data);

       // Load the appropriate difficulty settings based on player prefs
       CurrentGameplaySettings = await LoadDifficultySettingsAsync();
   }

   private async UniTask<GameplaySettings> LoadDifficultySettingsAsync()
   {
       // Get the saved difficulty index with validation
       int difficultyIndex = GetValidDifficultyIndex();

       try
       {
           return await LoadGameplaySettings(difficultyIndex);
       }
       catch (Exception e)
       {
           HandleSettingsLoadError(e);
           return CreateFallbackSettings();
       }
   }

   private int GetValidDifficultyIndex()
   {
       int difficultyIndex = PlayerPrefsHelper.GetIntValue(PlayerPrefsHelper.DIFFICULTY_KEY, PlayerPrefsHelper.DEFAULT_DIFFICULTY);

       // Make sure the index is valid for our settings array
       if (difficultyIndex < 0 || difficultyIndex >= difficultySettingsRefs.Count)
       {
           Debug.LogWarning($"Invalid difficulty index {difficultyIndex}, defaulting to Normal (1)");
           difficultyIndex = (int)GameplaySettings.DifficultyType.Normal;
           PlayerPrefsHelper.SaveIntValue(PlayerPrefsHelper.DIFFICULTY_KEY, difficultyIndex);
       }

       return difficultyIndex;
   }

   private async UniTask<GameplaySettings> LoadGameplaySettings(int difficultyIndex)
   {
       // Double-check index validity
       if (difficultyIndex < 0 || difficultyIndex >= difficultySettingsRefs.Count)
       {
           Debug.LogError($"Invalid difficulty index: {difficultyIndex}");
           return CreateFallbackSettings();
       }

       var settingsReference = difficultySettingsRefs[difficultyIndex];

       // Make sure the reference is valid
       if (settingsReference == null)
       {
           Debug.LogError($"No settings reference found for difficulty index {difficultyIndex}");
           return CreateFallbackSettings();
       }

       // Load the settings from Addressables
       var settingsHandle = Addressables.LoadAssetAsync<GameplaySettings>(settingsReference);
       var settings = await settingsHandle.Task;

       if (settings == null)
       {
           Debug.LogError("Failed to load game settings");
           return CreateFallbackSettings();
       }

       // Verify the loaded settings match the expected difficulty
       ValidateSettingsDifficulty(settings, difficultyIndex);

       return settings;
   }

   private void ValidateSettingsDifficulty(GameplaySettings settings, int difficultyIndex)
   {
       GameplaySettings.DifficultyType expectedDifficulty = (GameplaySettings.DifficultyType)difficultyIndex;

       if (settings.Difficulty != expectedDifficulty)
       {
           Debug.LogWarning($"Difficulty mismatch. Expected {expectedDifficulty}, got {settings.Difficulty}");
       }
   }

   private GameplaySettings CreateFallbackSettings()
   {
       Debug.LogError("Creating fallback difficulty settings");
       return ScriptableObject.CreateInstance<GameplaySettings>();
   }

   private void HandleSettingsLoadError(Exception e)
   {
       Debug.LogError($"Critical error loading game settings: {e.Message}");
   }
}