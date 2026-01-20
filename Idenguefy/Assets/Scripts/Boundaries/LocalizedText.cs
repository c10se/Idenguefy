using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Idenguefy.Controllers;

namespace Idenguefy.Boundaries
{
    /// <summary>
    /// Translates the language of relevant text fields selected within Unity's environment to a language selected from a list of options by the user.
    /// This component finds all TMP_Text objects and registers them for translation, skipping any with an "IgnoreLocalisation" tag.
    /// It also watches for new text elements added at runtime.
    /// </summary>
    /// <remarks>
    /// Author: Napatr
    /// Version: 1.0
    /// </remarks>
    [DisallowMultipleComponent]
    public class LocalizedText : MonoBehaviour
    {
        /// <summary>
        /// Holds a reference to a TMP_Text component and its original, untranslated string.
        /// </summary>
        [System.Serializable]
        public class LocalizedEntry
        {
            [Tooltip("The UI text component to be localized.")]
            public TMP_Text textComponent;
            [Tooltip("The original (English) text stored before any translation.")]
            public string originalText;
        }

        [Header("Localized Text Elements")]
        [Tooltip("A list of all text entries this component will manage and translate.")]
        [SerializeField]
        private List<LocalizedEntry> localizedEntries = new List<LocalizedEntry>();

        /// <summary>
        /// A HashSet for fast lookup to see if a text component is already being managed, to prevent duplicates.
        /// </summary>
        private HashSet<TMP_Text> trackedTexts = new HashSet<TMP_Text>();
        /// <summary>
        /// A flag to control the WatchForNewTextElements coroutine.
        /// </summary>
        private bool isWatching = true;
        /// <summary>
        /// The delay (in seconds) between scans for new, dynamically-added text elements.
        /// </summary>
        private float scanInterval = 1f;

        /// <summary>
        /// Initializes the component by auto-populating text elements, applying the current language,
        /// and starting the watcher coroutine to detect runtime-instantiated text.
        /// </summary>
        private async void Start()
        {
            AutoPopulateTextElements();

            if (LocalizationManager.Instance != null)
                await UpdateTextsAsync(LocalizationManager.Instance.currentLanguageCode);

            StartCoroutine(WatchForNewTextElements());
        }

        /// <summary>
        /// Registers this component with the LocalizationManager when enabled.
        /// </summary>
        private void OnEnable()
        {
            if (LocalizationManager.Instance != null)
                LocalizationManager.Instance.Register(this);
        }

        /// <summary>
        /// Unregisters this component from the LocalizationManager when disabled
        /// and stops the watcher coroutine.
        /// </summary>
        private void OnDisable()
        {
            if (LocalizationManager.Instance != null)
                LocalizationManager.Instance.Unregister(this);

            isWatching = false;
        }

        /// <summary>
        /// Asynchronously updates all registered text components to the target language.
        /// </summary>
        /// <param name="targetLanguage">The display name of the target language (e.g., "English", "Chinese").</param>
        /// <returns>A Task representing the asynchronous translation operation.</returns>
        public async Task UpdateTextsAsync(string targetLanguage)
        {
            if (LocalizationManager.Instance == null) return;

            string apiCode = LocalizationManager.Instance.GetAPICode(targetLanguage);

            foreach (var entry in localizedEntries)
            {
                if (entry.textComponent == null) continue;

                // Store the original text if it hasn't been stored yet
                if (string.IsNullOrEmpty(entry.originalText))
                    entry.originalText = entry.textComponent.text;

                if (targetLanguage == "English")
                {
                    // Revert to original text
                    entry.textComponent.text = entry.originalText;
                }
                else
                {
                    // Translate from the original text
                    string translated = await LocalizationManager.Instance.TranslateTextAsync(entry.originalText, apiCode);
                    entry.textComponent.text = translated;
                }
            }
        }


        /// <summary>
        /// Finds all TMP_Text components in the scene (and children) and adds them
        /// to the localizedEntries list, skipping any marked with "IgnoreLocalisation".
        /// </summary>
        [ContextMenu("Auto Populate Text Elements")]
        private void AutoPopulateTextElements()
        {
            localizedEntries.Clear();
            trackedTexts.Clear();

            TMP_Text[] allTexts = FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);

            foreach (var txt in allTexts)
            {
                if (txt == null) continue;

                // Skip if the text object or any of its parents has the "IgnoreLocalisation" tag
                // or if the text itself contains an ignore flag.
                if (HasIgnoreParent(txt.transform) || txt.text.Contains("<Ignore>"))
                    continue;

                localizedEntries.Add(new LocalizedEntry
                {
                    textComponent = txt,
                    originalText = txt.text
                });
                trackedTexts.Add(txt);
            }

            Debug.Log($"[LocalizedText] Auto-populated {localizedEntries.Count} text elements (excluding ignored parents).");
        }

        /// <summary>
        /// A coroutine that periodically scans for new TMP_Text elements
        /// that were instantiated at runtime and adds them to the localization list.
        /// </summary>
        /// <returns>An IEnumerator for the coroutine.</returns>
        private System.Collections.IEnumerator WatchForNewTextElements()
        {
            while (isWatching)
            {
                foreach (var txt in FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
                {
                    if (txt == null || trackedTexts.Contains(txt)) continue;

                    // Add the ignore check here too for runtime objects
                    if (HasIgnoreParent(txt.transform) || txt.text.Contains("<Ignore>"))
                        continue;

                    // Found a new text element
                    trackedTexts.Add(txt);
                    localizedEntries.Add(new LocalizedEntry { textComponent = txt, originalText = txt.text });

                    Debug.Log($"[LocalizedText] 🆕 Detected new text: {txt.text} on {txt.gameObject.name}");
                    // Immediately translate the new text to the current language
                    _ = ApplyCurrentLanguageToTextAsync(txt);
                }

                yield return new WaitForSeconds(scanInterval);
            }
        }

        /// <summary>
        /// Applies the *current* language translation to a single, newly detected text component.
        /// </summary>
        /// <param name="txt">The new text component to translate.</param>
        /// <returns>A Task representing the asynchronous translation operation.</returns>
        private async Task ApplyCurrentLanguageToTextAsync(TMP_Text txt)
        {
            if (LocalizationManager.Instance == null) return;
            string currentLang = LocalizationManager.Instance.currentLanguageCode;
            if (currentLang == "English") return; // No translation needed

            string apiCode = LocalizationManager.Instance.GetAPICode(currentLang);
            string translated = await LocalizationManager.Instance.TranslateTextAsync(txt.text, apiCode);
            txt.text = translated;
        }

        /// <summary>
        /// A public method intended to be used by a UI Button to trigger
        /// the language toggle in the LocalizationManager.
        /// </summary>
        public void OnLanguageToggleButtonPressed()
        {
            if (LocalizationManager.Instance != null)
                LocalizationManager.Instance.ToggleLanguage();
        }

        /// <summary>
        /// Checks if a transform or any of its ancestors have the "IgnoreLocalisation" tag.
        /// </summary>
        /// <param name="t">The transform to start the check from.</param>
        /// <returns>True if an ignored parent is found, false otherwise.</returns>
        private bool HasIgnoreParent(Transform t)
        {
            while (t != null)
            {
                if (t.CompareTag("IgnoreLocalisation"))
                    return true;
                t = t.parent;
            }
            return false;
        }

    }
}