using System;

namespace Idenguefy.Entities
{
    /// <summary>
    /// Represents a user-created pointer on the map containing:
    /// <list type="number">
    ///     <item><description> ID </description></item>
    ///     <item><description> Name </description></item>
    ///     <item><description> Home tag </description></item>
    ///     <item><description> Area name </description></item>
    ///     <item><description> Note </description></item>
    ///     <item><description> Coordinates </description></item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Author: Xavier
    /// Version: 1.0
    /// Notes: N/A
    /// </remarks>

    [Serializable]
    public class MapPointer
    {
        public string MapID;
        public string Name;
        public bool HomeTag;
        public string AreaName;
        public string Note;
        public float Longitude;
        public float Latitude;

        public MapPointer(string mapID, string name, bool homeTag, string areaName, string note, (float lon, float lat) coordinates)
        {
            MapID = mapID;
            Name = name;
            HomeTag = homeTag;
            AreaName = areaName;
            Note = note;
            Longitude = coordinates.lon;
            Latitude = coordinates.lat;
        }

        public MapPointer() { }

        public (float lon, float lat) Coordinates
        {
            get => (Longitude, Latitude);
            set
            {
                Longitude = value.lon;
                Latitude = value.lat;
            }
        }
    }
}
