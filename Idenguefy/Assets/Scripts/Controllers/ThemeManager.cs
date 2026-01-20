using System.Collections.Generic;
using UnityEngine;
using Idenguefy.Entities;
using Idenguefy.Boundaries;

namespace Idenguefy.Controllers
{
    /// <summary>
    /// Singleton manager for handling the application's visual theme.
    /// It allows toggling between light and dark modes and applies the
    /// selected theme to all registered UI elements via ThemeApplier.
    /// </summary>
    /// <remarks>
    /// Author: Napatr
    /// Version: 1.0
    /// Notes: Ensures persistence across scenes.
    /// </remarks>
    public class ThemeManager : MonoBehaviour
    {
        /// <summary>
        /// The static singleton instance of the ThemeManager.
        /// </summary>
        public static ThemeManager Instance;

        [Header("Current Theme State")]
        [Tooltip("Is the dark mode currently active?")]
        [SerializeField] private bool isDarkMode = false;

        [Header("Light Theme Colors")]
        [Tooltip("Color definitions for the light theme.")]
        public ThemeData lightTheme = new ThemeData
        {
            backgroundColor = new Color(0.9f, 0.9f, 0.9f),
            textColor = Color.black,
            buttonColor = new Color(0.75f, 0.75f, 0.75f),
            buttonTextColor = Color.black,
            inputBackgroundColor = new Color(0.95f, 0.95f, 0.95f),
            inputTextColor = Color.black,
            placeholderTextColor = new Color(0.4f, 0.4f, 0.4f),
            sliderFillColor = new Color(0.3f, 0.3f, 0.3f),
            sliderBackgroundColor = new Color(0.8f, 0.8f, 0.8f),
            toggleOnColor = new Color(0.2f, 0.6f, 1f),
            toggleOffColor = new Color(0.7f, 0.7f, 0.7f),

            // Dropdown-specific colors
            dropdownBackgroundColor = new Color(0.95f, 0.95f, 0.95f),
            dropdownTextColor = Color.black,
            dropdownArrowColor = new Color(0.25f, 0.25f, 0.25f),
            dropdownHighlightColor = new Color(0.8f, 0.8f, 0.8f)
        };

        [Header("Dark Theme Colors")]
        [Tooltip("Color definitions for the dark theme.")]
        public ThemeData darkTheme = new ThemeData
        {
            backgroundColor = new Color(0.08f, 0.08f, 0.08f),
            textColor = Color.white,
            buttonColor = new Color(0.25f, 0.25f, 0.25f),
            buttonTextColor = Color.white,
            inputBackgroundColor = new Color(0.18f, 0.18f, 0.18f),
            inputTextColor = Color.white,
            placeholderTextColor = new Color(0.6f, 0.6f, 0.6f),
            sliderFillColor = new Color(0.7f, 0.7f, 0.7f),
            sliderBackgroundColor = new Color(0.3f, 0.3f, 0.3f),
            toggleOnColor = new Color(0.3f, 0.8f, 1f),
            toggleOffColor = new Color(0.3f, 0.3f, 0.3f),

            // Dropdown-specific colors
            dropdownBackgroundColor = new Color(0.18f, 0.18f, 0.18f),
            dropdownTextColor = Color.white,
            dropdownArrowColor = new Color(0.8f, 0.8f, 0.8f),
            dropdownHighlightColor = new Color(0.3f, 0.3f, 0.3f)
        };

        /// <summary>
        /// A private list of all ThemeApplier components that need to be updated when the theme changes.
        /// </summary>
        private readonly List<ThemeApplier> registeredObjects = new List<ThemeApplier>();

        /// <summary>
        /// Initializes the singleton pattern, ensuring only one ThemeManager
        /// exists and persists across scenes.
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // --- Registration ---

        /// <summary>
        /// Registers a ThemeApplier to receive theme updates.
        /// Immediately applies the current theme upon registration.
        /// </summary>
        /// <param name="applier">The UI component to register.</param>
        public void Register(ThemeApplier applier)
        {
            if (!registeredObjects.Contains(applier))
            {
                registeredObjects.Add(applier);
                applier.ApplyTheme(GetCurrentTheme());
            }
        }

        /// <summary>
        /// Unregisters a ThemeApplier to stop it from receiving theme updates.
        /// </summary>
        /// <param name="applier">The UI component to unregister.</param>
        public void Unregister(ThemeApplier applier)
        {
            registeredObjects.Remove(applier);
        }

        // --- Theme Management ---

        /// <summary>
        /// Toggles the theme between light and dark mode and applies the new theme
        /// to all registered objects.
        /// </summary>
        public void ToggleTheme()
        {
            isDarkMode = !isDarkMode;
            ApplyThemeToAll();
            Debug.Log($"[ThemeManager] Toggled theme: {(isDarkMode ? "Dark" : "Light")}");
        }

        /// <summary>
        /// Gets the ThemeData object for the currently active theme.
        /// </summary>
        /// <returns>The ThemeData (light or dark) that is currently active.</returns>
        public ThemeData GetCurrentTheme()
        {
            return isDarkMode ? darkTheme : lightTheme;
        }

        /// <summary>
        /// Iterates through all registered ThemeApplier objects and instructs them
        /// to apply the current theme.
        /// </summary>
        private void ApplyThemeToAll()
        {
            ThemeData theme = GetCurrentTheme();
            foreach (var applier in registeredObjects)
            {
                if (applier != null)
                    applier.ApplyTheme(theme);
            }
        }
    }
}