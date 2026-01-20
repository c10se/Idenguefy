using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Idenguefy.Controllers;

namespace Idenguefy.Boundaries
{
    /// <summary>
    /// Displays the following adjustable fields to the user:
    /// <list type="number">
    /// <item><description>Proximity, adjustable through a slider</description></item>
    /// <item><description>Language, interchangeable via a dropdown menu</description></item>
    /// <item><description>Colour Theme, swapping between dark and light mode</description></item>
    /// </list>
    /// Also includes an option to revert to default settings
    /// </summary>
    /// <remarks>
    /// Author: Sharina, Xavier
    /// Version: 1.0
    /// </remarks>
    public class SettingsView : MonoBehaviour
    {
        [Tooltip("The parent GameObject for the settings panel.")]
        public GameObject optionsBox;
        [Tooltip("GameObject representing the light mode visual (e.g., a sun icon).")]
        public GameObject lightModeButton;
        [Tooltip("GameObject representing the dark mode visual (e.g., a moon icon).")]
        public GameObject darkModeButton;

        [Tooltip("The moving handle of the theme toggle UI.")]
        public RectTransform lightModeUIHandle;
        [Tooltip("The main Toggle component for theme switching.")]
        public Toggle toggle;
        /// <summary>Internal storage for the toggle's 'off' position.</summary>
        Vector2 handlePosition;
        [Tooltip("The background image of the theme toggle itself.")]
        public Image backgroundImage;
        [Tooltip("The color of the toggle's background when 'on' (Dark Mode).")]
        public Color backgroundActiveColor;
        [Tooltip("The color of the toggle's background when 'off' (Light Mode).")]
        public Color backgroundDefaultColor;

        [Header("Language Settings")]
        [Tooltip("The dropdown menu for selecting language.")]
        public TMP_Dropdown languageDropdown;
        //public Image flagImage;
        //public Sprite englishFlag;
        //public Sprite chineseFlag;

        [Header("Proximity Threshold")]
        [Tooltip("The slider component to adjust the proximity threshold.")]
        public Slider thresholdSlider;         // reference to UI slider
        [Tooltip("The text label to display the current threshold value.")]
        public TMP_Text thresholdValueLabel;   // optional, to show current value

        /// <summary>An instance of the SettingsController to manage preference data.</summary>
        private SettingsController settingsController;
        [Tooltip("Reference to the ThemeApplier to trigger theme updates.")]
        public ThemeApplier themeApplier;
        [Tooltip("Reference to the PointerRenderer to refresh markers (e.g., on theme change).")]
        public PointerRenderer pointerRenderer;
        [Tooltip("Reference to the LocalizedText component to trigger language updates.")]
        public LocalizedText localizedText;

        [Header("Theme Settings")]
        [Tooltip("A list of background panels to apply theme colors to.")]
        public Image backgroundPanel;   // e.g., your main background panel
        [Tooltip("A list of text elements to apply theme colors to.")]
        public TMP_Text[] textElements; // assign multiple texts in inspector

        [Tooltip("The background color for light mode.")]
        public Color lightBackground = Color.white;
        [Tooltip("The text color for light mode.")]
        public Color lightText = Color.black;

        [Tooltip("The background color for dark mode.")]
        public Color darkBackground = new Color(0.1f, 0.1f, 0.1f);
        [Tooltip("The text color for dark mode.")]
        public Color darkText = Color.white;

        /// <summary>
        /// Initializes the SettingsController.
        /// </summary>
        private void Awake()
        {
            settingsController = new SettingsController();

        }

        /// <summary>
        /// Initializes all UI elements with values from the SettingsController
        /// and adds listeners for changes.
        /// </summary>
        private void Start()
        {
            // Initialize dropdown + flag from saved settings
            string currentLang = settingsController.GetLangPref();
            if (currentLang == "English") languageDropdown.value = 0;
            else if (currentLang == "Chinese") languageDropdown.value = 1;

            OnLanguageChanged(languageDropdown.value);

     
            handlePosition = lightModeUIHandle.anchoredPosition;

            // Initialize threshold slider
            int savedThreshold = settingsController.GetThresholdPref();
            thresholdSlider.value = savedThreshold;
            UpdateThresholdLabel(savedThreshold);

            // Add listeners
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
            thresholdSlider.onValueChanged.AddListener(OnThresholdChanged);

            // Set the theme toggle to the saved state and apply the theme
            toggle.isOn = settingsController.GetThemePref();
            ChangeMode(toggle.isOn);
            toggle.onValueChanged.AddListener(ChangeMode);
        }

        /// <summary>
        /// Toggles between light and dark colour themes based on the toggle's state.
        /// </summary>
        /// <param name="on">The state of the theme toggle (true for Dark Mode, false for Light Mode).</param>
        public void ChangeMode(bool on)
        {
            if (on) // Dark Mode
            {
                lightModeUIHandle.anchoredPosition = handlePosition * -1;
                ApplyTheme(darkBackground, darkText);
                settingsController.SetThemePref(true);
                backgroundImage.color = backgroundActiveColor;
            }
            else // Light Mode
            {
                lightModeUIHandle.anchoredPosition = handlePosition;
                ApplyTheme(lightBackground, lightText);
                settingsController.SetThemePref(false);
                backgroundImage.color = backgroundDefaultColor;
            }

            // Notify other systems of the theme change
            themeApplier.OnToggleTheme();
            //pointerRenderer.RefreshPointers(); // Refresh pointers to use theme-appropriate sprites
            pointerRenderer.RecolorPointers();
        }

        /// <summary>
        /// Shows the settings menu.
        /// </summary>
        public void DisplaySettings()
        {
            optionsBox.SetActive(true);
            thresholdSlider.gameObject.SetActive(true);
        }

        /// <summary>
        /// Hides the settings menu.
        /// </summary>
        public void HideSettings()
        {
            optionsBox.SetActive(false);
            thresholdSlider.gameObject.SetActive(false);
        }


        /// <summary>
        /// Sets the relevant elements to the specified light/dark colour mode based on the theme.
        /// </summary>
        /// <param name="bgColor">The background color to apply.</param>
        /// <param name="txtColor">The text color to apply.</param>
        private void ApplyTheme(Color bgColor, Color txtColor)
        {
            if (backgroundPanel != null)
                backgroundPanel.color = bgColor;

            foreach (var text in textElements)
            {
                if (text != null)
                    text.color = txtColor;
            }
        }

        /// <summary>
        /// Sets the language in the settings controller based on the users selected language
        /// and triggers the translation update.
        /// </summary>
        /// <param name="index">The index from the language dropdown (0=English, 1=Chinese).</param>
        private void OnLanguageChanged(int index)
        {
            index = languageDropdown.value; //Value changed for dropdown

            switch (index)
            {
                case 0: // English
                    //flagImage.sprite = englishFlag;
                    settingsController.SetLangPref("English");
                    localizedText.OnLanguageToggleButtonPressed();
                    break;

                case 1: // Chinese
                    //flagImage.sprite = chineseFlag;
                    settingsController.SetLangPref("Chinese");
                    localizedText.OnLanguageToggleButtonPressed();
                    break;

                default:
                    //flagImage.sprite = null;
                    break;
            }
        }

        /// <summary>
        /// Updates the threshold value in the settings when the slider has been moved.
        /// </summary>
        /// <param name="value">The new value from the slider.</param>
        private void OnThresholdChanged(float value)
        {
            int intValue = Mathf.RoundToInt(value);
            settingsController.SetThresholdPref(intValue);
            UpdateThresholdLabel(intValue);
        }

        /// <summary>
        /// Updates the value threshold label on the settings UI for the user to see.
        /// </summary>
        /// <param name="value">The new threshold value to display.</param>
        private void UpdateThresholdLabel(int value)
        {
            if (thresholdValueLabel != null)
                thresholdValueLabel.text = value.ToString();
        }

        /// <summary>
        /// Resets all settings to their default values when triggered, updating it across the system.
        /// The values to be reset, and their default values are:
        /// <list type="number">
        /// <item><description>Proximity Threshold = 500</description></item>
        /// <item><description>Localisation = English</description></item>
        /// <item><description>Light/Dark Mode = Light Mode</description></item>
        /// </list>
        /// </summary>
        public void ResetSettings()
        {
            settingsController.ResetSettings();

            // Initialize dropdown + flag from saved settings
            string currentLang = settingsController.GetLangPref();
            if (currentLang == "English") languageDropdown.value = 0;
            else if (currentLang == "Chinese") languageDropdown.value = 1;
            OnLanguageChanged(languageDropdown.value);

            // Initialize threshold slider
            int savedThreshold = settingsController.GetThresholdPref();
            thresholdSlider.value = savedThreshold;
            UpdateThresholdLabel(savedThreshold);

            // Re-initialize theme toggle
            ChangeMode(settingsController.GetThemePref());
            toggle.isOn = settingsController.GetThemePref();
            ChangeMode(toggle.isOn);
            handlePosition = lightModeUIHandle.anchoredPosition;
        }
    }
}