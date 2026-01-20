using Idenguefy.Entities;
using System.Collections.Generic;
using UnityEngine;

namespace Idenguefy.Controllers
{
    /// <summary>
    /// Handles all interactions with the Settings entity.
    /// Manages loading, saving, and providing access to user preferences
    /// like theme, language, and proximity threshold.
    /// </summary>
    /// <remarks>
    /// Author: Sharina
    /// Version: 1.0
    /// </remarks>
    public class SettingsController
    {
        /// <summary>
        /// The private Settings object instance currently holding the application's preferences.
        /// </summary>
        private Settings settings;

        /// <summary>
        /// The generic data manager responsible for serializing/deserializing the settings object.
        /// </summary>
        private DataManager<Settings> dataManager;

        /// <summary>
        /// Initializes a new instance of the SettingsController
        /// and loads existing settings from storage.
        /// </summary>
        public SettingsController()
        {
            dataManager = new DataManager<Settings>("Idenguefy_Settings");
            LoadSettings();
        }

        // ------------------------
        // Threshold Prefs
        // ------------------------

        /// <summary>
        /// Gets the current proximity threshold preference.
        /// </summary>
        /// <returns>The proximity threshold value.</returns>
        public int GetThresholdPref()
        {
            return settings.ProximityThreshold;
        }

        /// <summary>
        /// Sets and saves the proximity threshold preference.
        /// </summary>
        /// <param name="value">The new threshold value.</param>
        public void SetThresholdPref(int value)
        {
            settings.ProximityThreshold = value;
            SaveSettings();
        }

        // ------------------------
        // Theme Prefs
        // ------------------------

        /// <summary>
        /// Gets the current theme preference (Dark Mode on/off).
        /// </summary>
        /// <returns>True if Dark Mode is enabled, false otherwise.</returns>
        public bool GetThemePref()
        {
            return settings.IsDarkMode;
        }

        /// <summary>
        /// Sets and saves the theme preference.
        /// </summary>
        /// <param name="isDarkMode">The new theme state (true for Dark Mode).</param>
        public void SetThemePref(bool isDarkMode)
        {
            settings.IsDarkMode = isDarkMode;
            SaveSettings();
        }

        // ------------------------
        // Language Prefs
        // ------------------------

        /// <summary>
        /// Gets the current language preference.
        /// </summary>
        /// <returns>The language identifier string.</returns>
        public string GetLangPref()
        {
            return settings.PrefLanguage;
        }

        /// <summary>
        /// Sets and saves the language preference.
        /// </summary>
        /// <param name="langCode">The new language identifier string.</param>
        public void SetLangPref(string langCode)
        {
            settings.PrefLanguage = langCode;
            SaveSettings();
        }

        // ------------------------
        // Save/Load Settings
        // ------------------------

        /// <summary>
        /// Saves the current settings object to persistent storage using the DataManager.
        /// </summary>
        private void SaveSettings()
        {
            //PlayerPrefs.SetInt("Threshold", settings.ProximityThreshold);
            //PlayerPrefs.SetInt("IsDarkMode", settings.IsDarkMode ? 1 : 0);
            //PlayerPrefs.SetString("Language", settings.PrefLanguage);
            //PlayerPrefs.Save();
            List<Settings> settingsList = new List<Settings> { settings };
            dataManager.SaveData(settingsList);
        }

        /// <summary>
        /// Loads the settings object from persistent storage.
        /// If no settings are found, it initializes with default values.
        /// </summary>
        private void LoadSettings()
        {
            //int threshold = PlayerPrefs.GetInt("Threshold", 5);
            //bool isDarkMode = PlayerPrefs.GetInt("IsDarkMode", 0) == 1;
            //string language = PlayerPrefs.GetString("Language", "en");

            //settings = new Settings(threshold, isDarkMode, language);
            List<Settings> loadedSettings = dataManager.LoadData();
            if (loadedSettings.Count > 0)
            {
                settings = loadedSettings[0];
            }
            else
            {
                // If no settings found, use default values
                settings = new Settings(500, false, "English");
            }
        }

        /// <summary>
        /// Resets all settings to their default values and saves them.
        /// </summary>
        public void ResetSettings()
        {
            //SetLangPref("en");
            //SetThemePref(false);
            //SetThresholdPref(5);
            settings = new Settings(500, false, "English");
            SaveSettings();
        }
    }
}