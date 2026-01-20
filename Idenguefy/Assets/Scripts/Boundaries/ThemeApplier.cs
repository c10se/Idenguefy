using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

using Idenguefy.Entities;
using Idenguefy.Controllers;

namespace Idenguefy.Boundaries
{
    /// <summary>
    /// Used to update all applicable objects colour depending on the users selected colour theme.
    /// This component finds all UI elements in the scene and applies theme colors.
    /// It also swaps sprites based on a "Light Mode" / "Dark Mode" naming convention.
    /// </summary>
    /// <remarks>
    /// Author: Napatr
    /// Version: 1.0
    /// </remarks>
    public class ThemeApplier : MonoBehaviour
    {
        /// <summary>
        /// Defines the two possible theme states.
        /// </summary>
        public enum ThemeMode { Light, Dark }

        [Header("Current Theme Mode")]
        [Tooltip("The theme currently applied by this applier.")]
        [SerializeField] private ThemeMode currentTheme = ThemeMode.Light;

        [Header("Auto-Detected Targets")]
        [Tooltip("All UI Images found in the scene (used for sprite swapping).")]
        [SerializeField] private List<Image> allImages = new List<Image>();
        [Tooltip("All SpriteRenderers found in the scene (used for sprite swapping).")]
        [SerializeField] private List<SpriteRenderer> allSpriteRenderers = new List<SpriteRenderer>();

        /// <summary>
        /// A cache of sprites loaded from the Resources folder for fast swapping.
        /// </summary>
        private readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

        [Header("UI Elements to Apply Theme To (auto-populated if needed)")]
        [Tooltip("List of all Images to apply background colors to.")]
        [SerializeField] private List<Image> imageTargets = new List<Image>();
        [Tooltip("List of all Buttons to apply button/text colors to.")]
        [SerializeField] private List<Button> buttonTargets = new List<Button>();
        [Tooltip("List of all Texts to apply text colors to.")]
        [SerializeField] private List<TMP_Text> textTargets = new List<TMP_Text>();
        [Tooltip("List of all InputFields to apply input/text/placeholder colors to.")]
        [SerializeField] private List<TMP_InputField> inputFieldTargets = new List<TMP_InputField>();
        [Tooltip("List of all Toggles to apply toggle/check/label colors to.")]
        [SerializeField] private List<Toggle> toggleTargets = new List<Toggle>();
        [Tooltip("List of all Sliders to apply fill/background/handle colors to.")]
        [SerializeField] private List<Slider> sliderTargets = new List<Slider>();
        [Tooltip("List of all Dropdowns to apply background/text/arrow colors to.")]
        [SerializeField] private List<TMP_Dropdown> dropdownTargets = new List<TMP_Dropdown>();

        /// <summary>
        /// On Start, finds all UI elements, caches sprites, and applies the initial theme.
        /// </summary>
        private void Start()
        {
            AutoPopulateAllUIElementsInScene();
            CacheAllSprites();
            // Apply Light colors
            ApplyTheme(GetThemeDataFromMode(currentTheme));
            //Apply Light Sprite
            ApplyTheme(currentTheme);
        }

        /// <summary>
        /// Registers this applier with the ThemeManager when enabled.
        /// </summary>
        private void OnEnable()
        {
            if (ThemeManager.Instance != null)
                ThemeManager.Instance.Register(this);
        }

        /// <summary>
        /// Unregisters this applier from the ThemeManager when disabled.
        /// </summary>
        private void OnDisable()
        {
            if (ThemeManager.Instance != null)
                ThemeManager.Instance.Unregister(this);
        }

        /// <summary>
        /// Applies the selected colour theme data to all applicable UI elements.
        /// </summary>
        /// <param name="theme">The ThemeData object containing the colors to apply.</param>
        public void ApplyTheme(ThemeData theme)
        {
            if (theme == null) return;

            // --- IMAGES --- (Applying background color)
            foreach (var img in imageTargets)
                if (img != null) img.color = theme.backgroundColor;

            // --- TEXTS ---
            foreach (var txt in textTargets)
                if (txt != null) txt.color = theme.textColor;

            // --- BUTTONS ---
            foreach (var btn in buttonTargets)
            {
                if (btn == null) continue;

                var btnImage = btn.GetComponent<Image>();
                if (btnImage != null)
                    btnImage.color = theme.buttonColor;

                TMP_Text btnText = btn.GetComponentInChildren<TMP_Text>();
                if (btnText != null)
                    btnText.color = theme.buttonTextColor;
            }

            // --- INPUT FIELDS ---
            foreach (var field in inputFieldTargets)
            {
                if (field == null) continue;

                var bg = field.GetComponent<Image>();
                if (bg != null)
                    bg.color = theme.inputBackgroundColor;

                if (field.textComponent != null)
                    field.textComponent.color = theme.inputTextColor;

                if (field.placeholder is TMP_Text placeholder)
                    placeholder.color = theme.placeholderTextColor;
            }

            // --- TOGGLES ---
            foreach (var toggle in toggleTargets)
            {
                if (toggle == null) continue;

                if (toggle.targetGraphic is Image bg)
                    bg.color = toggle.isOn ? theme.toggleOnColor : theme.toggleOffColor;

                if (toggle.graphic is Image check)
                    check.color = theme.textColor;

                TMP_Text label = toggle.GetComponentInChildren<TMP_Text>();
                if (label != null)
                    label.color = theme.textColor;
            }

            // --- SLIDERS ---
            foreach (var slider in sliderTargets)
            {
                if (slider == null) continue;

                if (slider.fillRect != null)
                {
                    var fillImg = slider.fillRect.GetComponent<Image>();
                    if (fillImg != null)
                        fillImg.color = theme.sliderFillColor;
                }

                var bg = slider.GetComponent<Image>();
                if (bg != null)
                    bg.color = theme.sliderBackgroundColor;

                if (slider.handleRect != null)
                {
                    var handleImg = slider.handleRect.GetComponent<Image>();
                    if (handleImg != null)
                        handleImg.color = theme.sliderFillColor;
                }
            }

            // --- DROPDOWNS ---
            foreach (var dropdown in dropdownTargets)
            {
                if (dropdown == null) continue;

                // Background
                var bg = dropdown.GetComponent<Image>();
                if (bg != null)
                    bg.color = theme.dropdownBackgroundColor;

                // Caption Text
                if (dropdown.captionText != null)
                    dropdown.captionText.color = theme.dropdownTextColor;

                // Arrow Icon (if exists)
                var arrow = dropdown.transform.Find("Arrow")?.GetComponent<Image>();
                if (arrow != null)
                    arrow.color = theme.dropdownArrowColor;

                // Highlight color in dropdown template (if open)
                if (dropdown.template != null)
                {
                    // This logic might need refinement to correctly target the highlight graphic
                    var itemBg = dropdown.template.GetComponentInChildren<Image>();
                    if (itemBg != null)
                        itemBg.color = theme.dropdownHighlightColor;
                }
            }
        }

        /// <summary>
        /// Called by UI elements (like SettingsView) to signal a theme toggle.
        /// This fetches the new theme, applies colors, and applies sprite swaps.
        /// </summary>
        public void OnToggleTheme()
        {
            if (ThemeManager.Instance != null)
                ThemeManager.Instance.ToggleTheme();

            bool currentThemeBool = new SettingsController().GetThemePref();

            currentTheme = currentThemeBool ? ThemeMode.Dark : ThemeMode.Light;

            ApplyTheme(GetThemeDataFromMode(currentTheme)); // Apply colors
            ApplyTheme(currentTheme); // Apply sprites
            Debug.Log($"[ThemeSwapper] Theme toggled to {currentTheme}");
        }

        /// <summary>
        /// Auto-populates all UI element lists from the entire scene using Object.FindObjectsByType.
        /// </summary>
        [ContextMenu("Auto Populate All UI Elements In Scene")]
        public void AutoPopulateAllUIElementsInScene()
        {
            //Auto populate all UI elements in the scene
            imageTargets = new List<Image>(Object.FindObjectsByType<Image>(FindObjectsSortMode.None));
            buttonTargets = new List<Button>(Object.FindObjectsByType<Button>(FindObjectsSortMode.None));
            textTargets = new List<TMP_Text>(Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None));
            inputFieldTargets = new List<TMP_InputField>(Object.FindObjectsByType<TMP_InputField>(FindObjectsSortMode.None));
            toggleTargets = new List<Toggle>(Object.FindObjectsByType<Toggle>(FindObjectsSortMode.None));
            sliderTargets = new List<Slider>(Object.FindObjectsByType<Slider>(FindObjectsSortMode.None));
            dropdownTargets = new List<TMP_Dropdown>(Object.FindObjectsByType<TMP_Dropdown>(FindObjectsSortMode.None));

            Debug.Log($"[ThemeApplier] Scene scan complete: " +
                        $"{imageTargets.Count} Images, {buttonTargets.Count} Buttons, " +
                        $"{textTargets.Count} Texts, {inputFieldTargets.Count} InputFields, " +
                        $"{toggleTargets.Count} Toggles, {sliderTargets.Count} Sliders, " +
                        $"{dropdownTargets.Count} Dropdowns.");

            // Auto populate sprite targets (including inactive objects)
            allImages = new List<Image>(FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            allSpriteRenderers = new List<SpriteRenderer>(FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None));

            Debug.Log($"[ThemeSwapper] Auto-populated {allImages.Count} Images and {allSpriteRenderers.Count} SpriteRenderers for sprite swapping.");
        }

        /// <summary>
        /// Applies the selected theme for Sprites (UI Images and SpriteRenderers).
        /// </summary>
        /// <param name="theme">The target theme mode (Light or Dark).</param>
        private void ApplyTheme(ThemeMode theme)
        {
            int changed = 0;

            // Update all UI Images (includes icons inside buttons, toggles, etc.)
            foreach (var img in allImages)
            {
                if (img == null || img.sprite == null) continue;
                Sprite swapped = GetSwappedSprite(img.sprite, theme);
                if (swapped != null)
                {
                    img.sprite = swapped;
                    changed++;
                }
            }

            // Update all SpriteRenderers (for in-world sprites)
            foreach (var sr in allSpriteRenderers)
            {
                if (sr == null || sr.sprite == null) continue;
                Sprite swapped = GetSwappedSprite(sr.sprite, theme);
                if (swapped != null)
                {
                    sr.sprite = swapped;
                    changed++;
                }
            }

            Debug.Log($"[ThemeSwapper] Applied {theme} theme — {changed} sprites swapped.");
        }


        /// <summary>
        /// Caches all Sprites from all "Resources" folders to prepare for swapping.
        /// </summary>
        private void CacheAllSprites()
        {
            spriteCache.Clear();

            // Load all sprites from Resources (so we can swap at runtime)
            Sprite[] allSprites = Resources.LoadAll<Sprite>("");

            foreach (var s in allSprites)
            {
                if (!spriteCache.ContainsKey(s.name))
                    spriteCache.Add(s.name, s);
            }

            Debug.Log($"[ThemeSwapper] Cached {spriteCache.Count} sprites from Resources/");
        }

        /// <summary>
        /// Finds the theme-appropriate version of a sprite based on its name.
        /// (e.g., "icon_Light Mode" -> "icon_Dark Mode").
        /// </summary>
        /// <param name="currentSprite">The sprite currently in use.</param>
        /// <param name="targetTheme">The theme to swap to.</param>
        /// <returns>The swapped sprite if found in the cache, otherwise null.</returns>
        private Sprite GetSwappedSprite(Sprite currentSprite, ThemeMode targetTheme)
        {
            if (currentSprite == null) return null;

            string currentName = currentSprite.name;
            string targetName;

            // Naming convention:
            if (targetTheme == ThemeMode.Dark)
                targetName = currentName.Replace("Light Mode", "Dark Mode");
            else
                targetName = currentName.Replace("Dark Mode", "Light Mode");

            if (targetName == currentName)
                return null; // Already the correct sprite or not part of the theme system

            // Try cache first
            if (spriteCache.TryGetValue(targetName, out Sprite swapped))
                return swapped;

            // Fallback to direct Resources load (slower, but a good safety net)
            swapped = Resources.Load<Sprite>(targetName);
            if (swapped == null)
            {
                // Only log this warning if the sprite *should* have been swapped
                Debug.LogWarning($"[ThemeSwapper] Missing sprite: '{targetName}' (from '{currentName}')");
            }

            return swapped;
        }

        /// <summary>
        /// Retrieves the theme data (colors) from the ThemeManager based on the mode.
        /// </summary>
        /// <param name="mode">The theme mode (Light or Dark).</param>
        /// <returns>The corresponding ThemeData object, or null if the manager isn't found.</returns>
        private ThemeData GetThemeDataFromMode(ThemeMode mode)
        {
            if (ThemeManager.Instance == null) return null;
            return (mode == ThemeMode.Dark) ? ThemeManager.Instance.darkTheme : ThemeManager.Instance.lightTheme;
        }

    }
}