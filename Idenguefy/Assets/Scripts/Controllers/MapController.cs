using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using static Idenguefy.Controllers.MapUtils;

using System.Collections;
using Idenguefy.Entities;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System;
using System.Threading.Tasks;

namespace Idenguefy.Controllers
{
    /// <summary>
    /// Handles the overall flow of map rendering, tile management, and dengue cluster visualization.
    /// 
    /// Workflow Overview:
    /// <list type="number">
    /// <item>Compute map tile boundaries based on Singapore’s geographical coordinates.</item>
    /// <item>Sequentially load and render all map tiles from the MapTiler API.</item>
    /// <item>Cache downloaded tiles to improve performance and reduce API calls.</item>
    /// <item>Ensure all tiles are loaded before rendering dengue cluster overlays.</item>
    /// <item>Integrate with <see cref="ClusterController"/> and <see cref="ClusterCircleDrawer"/> to display real-time cluster data.</item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Author: Eben, Xavier  
    /// Version: 2.0  
    /// Note: Map tiles are capped at zoom level 15 to avoid API quota errors.
    /// </remarks>
    public class MapController : MonoBehaviour
    {
  
        [Header("API Handler")]
        [Tooltip("Handles requests to the MapTiler API for fetching map tiles.")]
        [SerializeField]
        private TileAPIHandler tileAPIHandler;

        [Header("Cache Manager")]
        [Tooltip("Responsible for caching map tiles to minimize redundant API calls.")]
        [SerializeField]
        private TileCacheManager tileCacheManager;

        [Header("Map Tile Metadata")]
        [Tooltip("The pixel size of each map tile.")]
        public readonly float TILE_SIZE = 256f;

        [Tooltip("Singapore's geographic bounds: {LON_MIN, LAT_MIN}, {LON_MAX, LAT_MAX}.")]
        public readonly float[,] BOUNDARY = {
            {103.6f, 1.17f},
            {104.11667f, 1.48333f}
        };

        [Tooltip("Zoom level for tile rendering. Do not exceed 15 due to API constraints.")]
        public readonly int ZOOM = 15;

        [Header("Map References")]
        [Tooltip("Prefab template for individual map tiles.")]
        public GameObject mapTilePrefab;

        [Tooltip("Parent container (UI RectTransform) for displaying map tiles.")]
        public RectTransform mapPanelRect;

        [Header("Singleton")]
        [Tooltip("Global static instance of the MapController.")]
        public static MapController Instance { get; private set; }

        /// <summary>
        /// Initializes the MapController singleton and ensures persistence across scenes.
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);  // Keep the map persistent between scene transitions
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Loads a single map tile either from cache or directly from the MapTiler API.
        /// </summary>
        /// <param name="x">The X coordinate of the tile.</param>
        /// <param name="y">The Y coordinate of the tile.</param>
        /// <param name="xMin">Minimum X boundary tile index.</param>
        /// <param name="yMin">Minimum Y boundary tile index.</param>
        /// <returns>An enumerator for coroutine execution.</returns>
        public IEnumerator LoadMapTile(int x, int y, int xMin, int yMin)
        {
            Texture2D tileTexture = new Texture2D(2, 2);
            Task<Texture2D> task = tileCacheManager.RetrieveCachedTile(x, y);
            yield return new WaitUntil(() => task.IsCompleted);

            // Load from cache if available
            if (task.Result != null)
            {
                tileTexture = task.Result;
                Debug.Log($"[MapController] Loaded tile ({x}, {y}) from cache.");
            }
            else
            {
                // Otherwise fetch from the API
                Debug.Log("[MapController] Cache miss, fetching from API...");
                yield return tileAPIHandler.FetchMapTile(x, y, ZOOM);
                tileTexture = TileAPIHandler.response.texture;

                if (tileTexture && !tileCacheManager.checkAnyPendingLoad(x, y))
                    tileCacheManager.CacheTileData(tileTexture, x, y);

                Debug.Log($"[MapController] Loaded tile ({x}, {y}) from API.");
            }

            // Construct and render the tile in the map panel
            GameObject mapTile = ConstructMapTile(x, y, xMin, yMin);
            RawImage tileRawImage = mapTile.GetComponent<RawImage>();
            tileRawImage.texture = tileTexture;
        }

        /// <summary>
        /// Instantiates a map tile GameObject at the correct anchored position.
        /// </summary>
        /// <param name="x">Tile X coordinate.</param>
        /// <param name="y">Tile Y coordinate.</param>
        /// <param name="xMin">Minimum X index for offset calculation.</param>
        /// <param name="yMin">Minimum Y index for offset calculation.</param>
        /// <returns>The created map tile GameObject.</returns>
        public GameObject ConstructMapTile(int x, int y, int xMin, int yMin)
        {
            GameObject mapTile = Instantiate(mapTilePrefab, mapPanelRect);
            mapTile.transform.SetAsFirstSibling(); // Ensures correct draw order below overlay elements

            RectTransform tileRect = mapTile.GetComponent<RectTransform>();
            tileRect.sizeDelta = new Vector2(TILE_SIZE, TILE_SIZE);
            tileRect.anchoredPosition = new Vector2((x - xMin) * TILE_SIZE, -(y - yMin) * TILE_SIZE);

            return mapTile;
        }

        /// <summary>
        /// Calculates the tile coordinate boundaries for the current Singapore map view.
        /// </summary>
        /// <param name="xMin">Minimum tile X index.</param>
        /// <param name="xMax">Maximum tile X index.</param>
        /// <param name="yMin">Minimum tile Y index.</param>
        /// <param name="yMax">Maximum tile Y index.</param>
        public void FindTilesBoundary(out int xMin, out int xMax, out int yMin, out int yMax)
        {
            LonLatToTile(BOUNDARY[0, 0], BOUNDARY[0, 1], ZOOM, out xMin, out yMax);
            LonLatToTile(BOUNDARY[1, 0], BOUNDARY[1, 1], ZOOM, out xMax, out yMin);
        }

        /// <summary>
        /// Simple coroutine delay helper.
        /// </summary>
        /// <param name="seconds">Duration to wait in seconds.</param>
        public IEnumerator Delayer(int seconds)
        {
            yield return new WaitForSeconds(seconds);
        }


        /// <summary>
        /// Converts a given screen position to geographic coordinates (longitude, latitude).
        /// </summary>
        /// <param name="screenPos">Screen position in pixels.</param>
        /// <returns>Tuple containing (longitude, latitude).</returns>
        public (float lon, float lat) ScreenToGeo(Vector2 screenPos)
        {
            // Convert from screen space to local map coordinates
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mapPanelRect,
                screenPos,
                null, // For Screen Space - Overlay canvas; otherwise use the camera reference
                out Vector2 local
            );

            // Normalize local coordinates to a [0,1] range
            Vector2 size = mapPanelRect.rect.size;
            float x01 = Mathf.Clamp01((local.x + size.x * 0.5f) / size.x);
            float y01 = Mathf.Clamp01((local.y + size.y * 0.5f) / size.y);

            // Map normalized coordinates into Singapore’s longitude/latitude bounds
            float lon = Mathf.Lerp(BOUNDARY[0, 0], BOUNDARY[1, 0], x01);
            float lat = Mathf.Lerp(BOUNDARY[0, 1], BOUNDARY[1, 1], y01);

            return (lon, lat);
        }

        /// <summary>
        /// Converts geographic coordinates (longitude, latitude) to local UI screen coordinates.
        /// </summary>
        /// <param name="lon">Longitude value.</param>
        /// <param name="lat">Latitude value.</param>
        /// <returns>A <see cref="Vector2"/> representing on-screen pixel position within the map panel.</returns>
        public Vector2 GeoToScreen(float lon, float lat)
        {
            FindTilesBoundary(out int xMin, out int xMax, out int yMin, out int yMax);
            MapUtils.LonLatToPixelRatio(lon, lat, ZOOM, xMin, yMin, out double xPixelRatio, out double yPixelRatio);

            float xPixel = (float)(xPixelRatio * TILE_SIZE);
            float yPixel = (float)(yPixelRatio * TILE_SIZE);

            return new Vector2(xPixel, yPixel);
        }
    }
}
