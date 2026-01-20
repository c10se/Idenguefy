using Idenguefy.Controllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Idenguefy.Controllers
{
    /// <summary>
    /// The abstract base class as interface for handling map tile API requests and responses
    /// 
    /// Responsibilities:
    /// Provides an interface to implement specific map tile API handlers
    /// </summary>
    /// <remarks>
    /// Author: Eben
    /// Version: 1.0
    /// Notes: N/A
    /// </summary>
    /// </remarks>
    public abstract class TileAPIHandler : MonoBehaviour
    {
        //The TileAPI Response
        public static TileAPIResponse response { get; protected set; } = new TileAPIResponse();

        //Initialize the empty response object to contain future results
        public void Start()
        {
            response = new TileAPIResponse();
        }

        /// <summary>
        /// To fetch the RASTER map tile 2D texture given the x, y, zoom parameters
        /// </summary>
        /// <param name="x">global x-coordinate of the tile</param>
        /// <param name="y">global y-coordinate of the tile</param>
        /// <param name="zoom">global zoom level of the map tile</param>
        /// <returns></returns>
        public abstract IEnumerator FetchMapTile(int x, int y, int zoom);

        public class TileAPIResponse
        {
            public string message; //optional message from the API
            public Texture2D texture;
        }
    }
}
