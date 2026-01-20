using UnityEngine;

namespace Idenguefy.Entities
{
    /// <summary>
    /// Represents a location search item result:
    /// <list type="number">
    ///     <item><description> Name </description></item>
    ///     <item><description> Detail </description></item>
    ///     <item><description> Relevance </description></item>
    ///     <item><description> Coordinates </description></item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Author: Eben
    /// Version: 1.0
    /// Notes: N/A
    /// </remarks>
  
    public class MapSearchResult
    {
        public string name { get; set; }
        public string detail { get; set; }
        public float relevance { get; set; }
        public (float lon, float lat) coord { get; set; }

        public MapSearchResult(string name, string detail, float relevance, (float, float) coord)
        {
            this.name = name;
            this.detail = detail;
            this.relevance = relevance;
            this.coord = coord;
        }
    }
}
