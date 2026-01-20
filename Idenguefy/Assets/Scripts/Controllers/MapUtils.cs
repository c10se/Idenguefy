using NUnit.Framework;
using System;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace Idenguefy.Controllers
{
    /// <summary>
    /// Utility class providing coordinate conversion functions for the map system.
    /// Converts longitude/latitude into tile indices or pixel ratios according to the Web Mercator projection.
    /// </summary>
    /// <remarks>
    /// Author: Eben, Ryan, Xavier
    /// Version: 1.1
    /// Notes: N/A
    /// </remarks>
    public class MapUtils
    {
        /// <summary>
        /// Converts geographic coordinates (longitude, latitude) into a pixel ratio relative to a minimum tile boundary.
        /// This is used to calculate the precise position of a point within the map panel, offset by the top-left tile (xMin, yMin).
        /// </summary>
        /// <param name="longitude">The geographic longitude coordinate.</param>
        /// <param name="latitude">The geographic latitude coordinate.</param>
        /// <param name="zoom">The current map zoom level.</param>
        /// <param name="xMin">The minimum X tile index (left-most tile) of the map boundary.</param>
        /// <param name="yMin">The minimum Y tile index (top-most tile) of the map boundary.</param>
        /// <param name="xPixelRatio">Output: The calculated X position as a tile ratio, offset from xMin.</param>
        /// <param name="yPixelRatio">Output: The calculated Y position as a tile ratio, offset from yMin.</param>
        public static void LonLatToPixelRatio(double longitude, double latitude, int zoom, int xMin, int yMin,
        out double xPixelRatio, out double yPixelRatio)
        {
            double xNorm = calcNormalizedX(longitude);
            double yNorm = calcNormalizedY(latitude);

            int zoomFactor = 1 << zoom;
            xPixelRatio = (xNorm * zoomFactor - xMin);
            yPixelRatio = -(yNorm * zoomFactor - yMin);
        }

        /// <summary>
        /// Converts geographic coordinates (longitude, latitude) into Web Mercator tile indices (X, Y) for a given zoom level.
        /// </summary>
        /// <param name="longitude">The geographic longitude coordinate.</param>
        /// <param name="latitude">The geographic latitude coordinate.</param>
        /// <param name="zoom">The current map zoom level.</param>
        /// <param name="x">Output: The calculated tile X index.</param>
        /// <param name="y">Output: The calculated tile Y index.</param>
        public static void LonLatToTile(double longitude, double latitude, int zoom, out int x, out int y)
        {
            double latRad = latitude * Math.PI / 180.0;
            int n = 1 << zoom; // 2^zoom

            x = (int)(calcNormalizedX(longitude) * n);
            y = (int)(calcNormalizedY(latitude) * n);
        }

        /// <summary>
        /// Helper function to calculate the normalized X coordinate (0.0 to 1.0) from longitude.
        /// </summary>
        /// <param name="longitude">The geographic longitude coordinate.</param>
        /// <returns>The normalized X value (0.0 to 1.0) based on the Mercator projection.</returns>
        public static double calcNormalizedX(double longitude)
        {
            double xNorm = (longitude + 180.0) / 360.0;
            return xNorm;
        }

        /// <summary>
        /// Helper function to calculate the normalized Y coordinate (0.0 to 1.0) from latitude.
        /// </summary>
        /// <param name="latitude">The geographic latitude coordinate.</param>
        /// <returns>The normalized Y value (0.0 to 1.0) based on the Mercator projection.</returns>
        public static double calcNormalizedY(double latitude)
        {
            double latRad = latitude * Math.PI / 180.0;
            double yNorm = (1.0 - Math.Log(Math.Tan(latRad) + 1.0 / Math.Cos(latRad)) / Math.PI) / 2.0;
            return yNorm;
        }

        /// <summary>
        /// Calculates the great-circle distance between two geographic points (lon/lat) using the Haversine formula.
        /// </summary>
        /// <param name="lon1">Longitude of the first point.</param>
        /// <param name="lat1">Latitude of the first point.</param>
        /// <param name="lon2">Longitude of the second point.</param>
        /// <param name="lat2">Latitude of the second point.</param>
        /// <returns>The distance between the two points in kilometers (based on Earth radius R = 6371km).</returns>
        public static double HaversineDistance(float lon1, float lat1, float lon2, float lat2)
        {

            const double R = 6371; // Earth radius in kilometers
                                   //absolute difference in latitude and longitude, in radians. 
            double dLat = Mathf.Deg2Rad * (lat2 - lat1);
            double dLon = Mathf.Deg2Rad * (lon2 - lon1);
            //
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(Mathf.Deg2Rad * lat1) * Math.Cos(Mathf.Deg2Rad * lat2) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }

}