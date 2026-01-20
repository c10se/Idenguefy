namespace Idenguefy.Entities
{
    /// <summary>
    /// Represents a preventive health tip
    /// </summary>
    /// <remarks>
    /// Author: XL
    /// Version: 1.0
    /// Notes: N/A
    /// </remarks>
    public class PreventiveHealthTip
    {
        public string Context { get; set; }

        public PreventiveHealthTip(string context) {
            Context = context;
        }
    }

}
