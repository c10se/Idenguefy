using Idenguefy.Controllers;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;

namespace Idenguefy.Controllers
{
    /// <summary>
    /// The SearchAPIHandler implementation for MapTiler's Geocoding API
    /// 
    /// Responsibilities:
    /// <list type="number">
    ///     <item><description> Request relevant tile (x, y, zoom) data to obtain texture data</description></item>
    ///     <item><description> Download the given raw 2d texture of the raster tiles</description></item>
    ///     <item><description> Parse and convert features into Texture2D entity</description></item>
    ///     <item><description> Wrap array with appropriate TileAPIResponse Wrapper </description></item>
    /// </summary>
    /// <remarks>
    /// Author: Eben
    /// Version: 1.0
    /// Notes: N/A
    /// </summary>
    /// </remarks>
    public class MTTileAPIHandler : TileAPIHandler
    {
        [Header("API Attributes")]
        private string API_KEY;
        private static readonly string BASE_URL = "https://api.maptiler.com/maps/";
        private static readonly string FORMAT = "png";
        private static readonly string STYLE_ID = "dataviz";
        void Awake()
        {
            EnvLoader.Load();
            API_KEY = EnvLoader.Get("MTMapTileHandler_API_Key");
        }

        // TileAPIResponse 'response' is inherited from TileAPIHandler
        // Avoid creating a new searchResultList on the APIHandler side since SearchController already has one

        /// <summary>
        /// Run the entire tile-fetch flow: fetch taster map tiles from API, create a GameObject and assign texture
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>

        public override IEnumerator FetchMapTile(int x, int y, int zoom)
        {
            //Debug.Log($"[MTTileAPIHandler] FetchMapTile called with x={x}, y={y}, zoom={zoom}");
            string url = constructURL(x, y, zoom);
            //Debug.Log($"[MTTileAPIHandler] Constructed URL: {url}");

            yield return FetchTileByURL(url);
            //Debug.Log($"[MTTileAPIHandler] Fetched texture assigned to response (TileAPIResponse).");

        }

        /// <summary>
        /// To fetch the RASTER map tile 2D texture given the x, y, zoom parameters via the constructed URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private IEnumerator FetchTileByURL(string url)
        {
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
            {
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {   
                    // Temp comment removal to avoid flooding console
                    //Debug.LogError($"[MTTileAPIHandler] Error fetching tile: {request.error}");
                    response.message = request.error;
                    response.texture = null;
                }
                else
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(request);
                    Debug.Log($"[MTTileAPIHandler] Successfully fetched tile texture.");
                    response.message = "Success";
                    response.texture = texture;
                }
            }
        }

        /// <summary>
        /// To construct the MapTiler API URL given the input parameter x, y, zoom parameters
        /// </smmary>
        /// <param name="x">global y-coordinate of the tile</param>
        /// <param name="y">global y-coordinate of the tile</param>
        /// <param name="zoom">global zoom level of the map tile</param>
        /// <returns></returns>
        private string constructURL(int x, int y, int zoom)
        {
            return $"{BASE_URL}{STYLE_ID}/{zoom}/{x}/{y}.{FORMAT}?key={API_KEY}";
        }
    }
}
