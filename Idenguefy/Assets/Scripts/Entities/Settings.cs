using System;

namespace Idenguefy.Entities
{
    /// <summary>
    /// Represents user settings
    /// Contains preferences for:
    /// <list type="number">
    ///     <item><description> Proximity Alerts </description></item>
    ///     <item><description> Theme </description></item>
    ///     <item><description> Language </description></item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Author: Xavier
    /// Version: 1.0
    /// Notes: N/A
    /// </remarks>
    [Serializable]
    public class Settings
    {
        public int ProximityThreshold;
        public bool IsDarkMode;
        public string PrefLanguage;

        public Settings(int proximityThreshold, bool isDarkMode, string prefLanguage)
        {
            ProximityThreshold = proximityThreshold;
            IsDarkMode = isDarkMode;
            PrefLanguage = prefLanguage;
        }
    }

}


