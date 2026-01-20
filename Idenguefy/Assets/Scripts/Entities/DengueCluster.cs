using System.Collections.Generic;

namespace Idenguefy.Entities
{
    /// <summary>
    /// Represents a dengue cluster containing:
    /// <list type="number">
    ///     <item><description> ID </description></item>
    ///     <item><description> Case size </description></item>
    ///     <item><description> Area name </description></item>
    ///     <item><description> Coordinates </description></item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Author: Xavier
    /// Version: 1.0
    /// Notes: N/A
    /// </remarks>

     
    public class DengueCluster
    {
        public string LocationID { get; set; }
        public int CaseSize { get; set; }
        public string Severity { get; set; }
        public string AreaName { get; set; }
        public List<(float lon, float lat)> Coordinates { get; set; }

        public DengueCluster(string locationID, int caseSize, string severity, string areaName, List<(float lon, float lat)> coordinates)
        {
            LocationID = locationID;
            CaseSize = caseSize;
            Severity = severity;
            AreaName = areaName;
            Coordinates = coordinates;
        }
    }
}


