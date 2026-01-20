using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Idenguefy.Boundaries;

namespace Idenguefy.Controllers
{
    /// <summary>
    /// Manages runtime text localization for the Idenguefy application.
    /// 
    /// Responsibilities:
    /// <list type="number">
    /// <item>Registers UI elements and updates their language dynamically.</item>
    /// <item>Communicates with Microsoft Translator API for translation.</item>
    /// <item>Applies throttling, caching, and retry logic to prevent rate limits.</item>
    /// <item>Persists the current language state globally across scenes.</item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Author: Napatr  
    /// Version: 1.0  
    /// This controller ensures consistent and efficient translation handling throughout the app.
    /// </remarks>
    public class LocalizationManager : MonoBehaviour
    {
        private string API_KEY;

        /// <summary>
        /// Singleton instance of the LocalizationManager for global access.
        /// </summary>
        public static LocalizationManager Instance;

        [Header("Current Language State")]
        /// <summary>
        /// The currently active language code (e.g., "English", "Chinese").
        /// Loaded from the user’s settings during initialization.
        /// </summary>
        [SerializeField]
        public string currentLanguageCode = "English";

        /// <summary>
        /// List of all registered <see cref="LocalizedText"/> objects that require translation updates.
        /// </summary>
        private readonly List<LocalizedText> registeredObjects = new List<LocalizedText>();

        /// <summary>
        /// Caches translated text to prevent redundant API calls for the same content.
        /// </summary>
        private readonly Dictionary<string, string> translationCache = new Dictionary<string, string>();

        /// <summary>
        /// Delay (in milliseconds) between sequential translation requests to prevent API rate limits.
        /// </summary>
        private const int DelayBetweenRequestsMs = 100;

        /// <summary>
        /// Maximum number of retry attempts for HTTP 429 (Too Many Requests) responses.
        /// </summary>
        private const int MaxRetries = 3;

        /// <summary>
        /// Initializes the LocalizationManager singleton and loads user language preferences.
        /// </summary>
        private void Awake()
        {
            EnvLoader.Load();
            API_KEY = EnvLoader.Get("Localization_API_Key");
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            currentLanguageCode = new SettingsController().GetLangPref();
        }

        /// <summary>
        /// Registers a <see cref="LocalizedText"/> object to be managed by the LocalizationManager.
        /// Automatically updates the text to the current language.
        /// </summary>
        /// <param name="applier">The <see cref="LocalizedText"/> instance to register.</param>
        public void Register(LocalizedText applier)
        {
            if (!registeredObjects.Contains(applier))
            {
                registeredObjects.Add(applier);
                _ = applier.UpdateTextsAsync(currentLanguageCode);
            }
        }

        /// <summary>
        /// Unregisters a <see cref="LocalizedText"/> object from translation management.
        /// </summary>
        /// <param name="applier">The <see cref="LocalizedText"/> instance to remove.</param>
        public void Unregister(LocalizedText applier)
        {
            registeredObjects.Remove(applier);
        }

       
        /// <summary>
        /// Toggles between supported languages (English/Chinese).
        /// Reads the new preference from <see cref="SettingsController"/> and applies updates.
        /// </summary>
        public void ToggleLanguage()
        {
            currentLanguageCode = new SettingsController().GetLangPref();
            Debug.Log($"[LocalizationManager] Toggled language to {currentLanguageCode}");
            _ = ApplyLanguageToAllAsync();
        }

        /// <summary>
        /// Applies the currently selected language to all registered UI elements asynchronously,
        /// throttled to prevent hitting the translation API rate limit.
        /// </summary>
        private async Task ApplyLanguageToAllAsync()
        {
            var objectsToUpdate = new List<LocalizedText>(registeredObjects);

            foreach (var applier in objectsToUpdate)
            {
                if (applier != null)
                {
                    await applier.UpdateTextsAsync(currentLanguageCode);
                    await Task.Delay(DelayBetweenRequestsMs);
                }
            }
        }

        
        /// <summary>
        /// Converts a human-readable language name to the corresponding API language code.
        /// </summary>
        /// <param name="language">Display name (e.g., "English", "Chinese").</param>
        /// <returns>Language code used by the translation API (e.g., "en", "zh").</returns>
        public string GetAPICode(string language)
        {
            return language switch
            {
                "English" => "en",
                "Chinese" => "zh",
                _ => "en"
            };
        }

        /// <summary>
        /// Translates a source text string into the specified target language.
        /// Uses the Microsoft Translator API with local caching, placeholder protection, and retry logic.
        /// </summary>
        /// <param name="sourceText">The original text to translate.</param>
        /// <param name="targetLanguageCode">The target language code (e.g., "en", "zh").</param>
        /// <returns>A task returning the translated text, or the original if translation fails.</returns>
        public async Task<string> TranslateTextAsync(string sourceText, string targetLanguageCode)
        {
            if (string.IsNullOrWhiteSpace(sourceText))
                return sourceText;

            // Ignore text containing only punctuation or symbols
            if (System.Text.RegularExpressions.Regex.IsMatch(sourceText, @"^[\p{P}\p{S}\s]+$"))
                return sourceText;

            sourceText = sourceText.Replace("\"", "\\\"");
            string cacheKey = $"{sourceText}|{targetLanguageCode}";

            if (translationCache.TryGetValue(cacheKey, out string cachedResult))
                return cachedResult;

            // Step 1: Temporarily replace punctuation with placeholders to preserve format
            var punctuationRegex = new System.Text.RegularExpressions.Regex(@"[!?,.:;()/:]");
            var punctuationMap = new Dictionary<string, string>();
            int index = 0;

            string textWithPlaceholders = punctuationRegex.Replace(sourceText, match =>
            {
                string token = $"<P{index++}>"; // e.g., <P0>, <P1>
                punctuationMap[token] = match.Value;
                return token;
            });

            // Step 2: Send the placeholder text to the Translator API
            string endpoint = $"https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&to={targetLanguageCode}";
            string bodyJson = "[{\"Text\": \"" + textWithPlaceholders + "\"}]";

            int retryCount = 0;
            while (retryCount < MaxRetries)
            {
                using (UnityWebRequest request = new UnityWebRequest(endpoint, "POST"))
                {
                    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(bodyJson);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.SetRequestHeader("Content-Type", "application/json");
                    request.SetRequestHeader("Ocp-Apim-Subscription-Key", API_KEY);
                    request.SetRequestHeader("Ocp-Apim-Subscription-Region", "southeastasia");

                    await request.SendWebRequest();

                    // Handle rate limiting with exponential backoff
                    if (request.responseCode == 429)
                    {
                        int waitTime = (int)Mathf.Pow(2, retryCount) * 1000;
                        Debug.LogWarning($"[LocalizationManager] 429 Too Many Requests. Retrying in {waitTime}ms...");
                        await Task.Delay(waitTime);
                        retryCount++;
                        continue;
                    }

                    // Handle other API errors
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"[LocalizationManager] Translation API error: {request.error}");
                        return sourceText;
                    }

                    // Step 3: Parse and restore translated text
                    string json = request.downloadHandler.text;
                    try
                    {
                        string wrapped = "{\"items\":" + json + "}";
                        TranslationWrapper wrapper = JsonUtility.FromJson<TranslationWrapper>(wrapped);
                        if (wrapper.items.Length > 0 && wrapper.items[0].translations.Length > 0)
                        {
                            string translatedText = wrapper.items[0].translations[0].text;

                            // Restore punctuation
                            foreach (var kvp in punctuationMap)
                                translatedText = translatedText.Replace(kvp.Key, kvp.Value);

                            translationCache[cacheKey] = translatedText;
                            return translatedText;
                        }
                    }
                    catch
                    {
                        Debug.LogError($"[LocalizationManager] JSON parse failed: {json}");
                    }

                    break;
                }
            }

            return sourceText;
        }

        
        [System.Serializable]
        private class TranslationWrapper { public TranslationItem[] items; }

        [System.Serializable]
        private class TranslationItem { public TranslationEntry[] translations; }

        [System.Serializable]
        private class TranslationEntry { public string text; public string to; }
    }
}
