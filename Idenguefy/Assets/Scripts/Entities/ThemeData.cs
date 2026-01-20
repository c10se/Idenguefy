using UnityEngine;


namespace Idenguefy.Entities
{
    /// <summary>
    /// Represents theme data
    /// Contains configuration for:
    /// <list type="number">
    ///     <item><description> All text colors </description></item>
    ///     <item><description> All input field colors </description></item>
    ///     <item><description> All slider colors </description></item>
    ///     <item><description> All toggle colors </description></item>
    ///     <item><description> All dropdown colors </description></item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Author: Napatr
    /// Version: 1.0
    /// Notes: N/A
    /// </remarks>
    [System.Serializable]

    public class ThemeData
    {
        public Color backgroundColor;
        public Color textColor;
        public Color buttonColor;
        public Color buttonTextColor;
        public Color inputBackgroundColor;
        public Color inputTextColor;
        public Color placeholderTextColor;
        public Color sliderFillColor;
        public Color sliderBackgroundColor;
        public Color toggleOnColor;
        public Color toggleOffColor;

        // New dropdown colors
        public Color dropdownBackgroundColor;
        public Color dropdownTextColor;
        public Color dropdownArrowColor;
        public Color dropdownHighlightColor;
    }
}
