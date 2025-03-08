using UnityEngine;

namespace Match3.Core
{
    /// <summary>
    /// Static helper class to centralize all PlayerPrefs operations for game settings
    /// </summary>
    public static class PlayerPrefsHelper
    {
        // PlayerPrefs keys
        public static readonly string DIFFICULTY_KEY = "GameDifficulty";
        public static readonly string MUSIC_MUTED_KEY = "MusicMuted";
        public static readonly string SFX_MUTED_KEY = "SfxMuted";
        public static readonly string HIGH_SCORE_KEY = "HighScore";

        // Default values
        public static readonly int DEFAULT_DIFFICULTY = 1; // Normal
        public static readonly int DEFAULT_AUDIO_STATE = 0; // Not muted (0=unmuted, 1=muted)
        public static readonly int DEFAULT_HIGH_SCORE = 0;

        #region Get Methods

        public static int GetIntValue(string key, int defaultValue)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public static bool GetBoolValue(string key, bool defaultValue)
        {
            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
        }

        public static float GetFloatValue(string key, float defaultValue)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        public static string GetStringValue(string key, string defaultValue)
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }

        #endregion

        #region Save Methods

        public static void SaveIntValue(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
        }

        public static void SaveBoolValue(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void SaveFloatValue(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
        }

        public static void SaveStringValue(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
        }

        #endregion

        #region Utility Functions

        public static bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public static void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }

        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        #endregion
    }
}