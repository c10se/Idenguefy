using System;
using System.Collections.Generic;
using System.IO;

namespace Idenguefy.Controllers
{
    /// <summary>
    /// A static utility class to load environment variables from a ".env" file
    /// at runtime. This allows for storing sensitive keys (like API keys)
    /// outside of source control.
    /// </summary>
    /// <remarks>
    /// Author: Napatr
    /// Version: 1.0
    /// Note: Ensure a ".env" file is present in the project's root directory.
    /// </remarks>
    public static class EnvLoader
    {
        /// <summary>
        /// The private dictionary that stores the loaded key/value pairs.
        /// </summary>
        private static readonly Dictionary<string, string> envVars = new();

        /// <summary>
        /// Loads the specified .env file, parses its contents, and stores
        /// the key/value pairs in the internal dictionary.
        /// </summary>
        /// <param name="path">The file path to the .env file. Defaults to ".env".</param>
        public static void Load(string path = ".env")
        {
            if (!File.Exists(path))
            {
                UnityEngine.Debug.LogWarning($"[EnvLoader] No .env file found at {path}");
                return;
            }

            // Read all lines from the file
            foreach (var line in File.ReadAllLines(path))
            {
                // Skip comments and empty lines
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                // Split the line at the first '='
                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                {
                    // Add the trimmed key and value to the dictionary
                    envVars[parts[0].Trim()] = parts[1].Trim();
                }
            }
        }

        /// <summary>
        /// Safely retrieves a value from the loaded environment variables.
        /// </summary>
        /// <param name="key">The key (variable name) to retrieve.</param>
        /// <param name="defaultValue">The value to return if the key is not found.</param>
        /// <returns>The stored value, or the default value if the key does not exist.</returns>
        public static string Get(string key, string defaultValue = "")
        {
            return envVars.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}