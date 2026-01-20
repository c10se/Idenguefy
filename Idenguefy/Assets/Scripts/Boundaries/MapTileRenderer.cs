using Idenguefy.Controllers;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Idenguefy.Boundaries
{
    /// <summary>
    /// Handles the rendering of all map tiles within the calculated boundaries.
    /// It iterates through the required tile coordinates and initiates the loading for each one.
    /// </summary>
    /// <remarks>
    /// Author: Eben, Xavier
    /// Version: 1.2
    /// </remarks>
    public class MapTileRenderer : MonoBehaviour
    {
        [Tooltip("Reference to the main MapController for data and loading methods.")]
        public MapController mapController;
        [Tooltip("A list of instantiated map tile GameObjects.")]
        public List<GameObject> mapTileList = new List<GameObject>();

        /// <summary>
        /// Coroutine that calculates map boundaries, resizes the map panel,
        /// and then iterates through all (x, y) tile coordinates,
        /// starting a LoadMapTile coroutine for each.
        /// </summary>
        /// <returns>An IEnumerator for the coroutine.</returns>
        public IEnumerator RenderMapTiles()
        {
            mapController.FindTilesBoundary(out int xMin, out int xMax, out int yMin, out int yMax);
            int xRange = xMax - xMin + 1;
            int yRange = yMax - yMin + 1;

            Debug.Log($"Tile boundary at zoom {mapController.ZOOM}: x in [{xMin}, {xMax}], y in [{yMin}, {yMax}]");

            // Resize the map panel to fit all the tiles
            mapController.mapPanelRect.sizeDelta = new Vector2(xRange * mapController.TILE_SIZE, yRange * mapController.TILE_SIZE);
            //mapPanelRect.anchoredPosition = new Vector2(-xRange*TILE_SIZE/2, -yRange*TILE_SIZE/2);

            int counter = 0;
            for (int x = xMin; x <= xMax; x++)
            {
                // To prevent exceeding API rate limits, insert a small delay
                if (counter % 1000 == 0)
                {
                    StartCoroutine(mapController.Delayer(1)); //to prevent exceeding API rate limit
                }
                for (int y = yMin; y <= yMax; y++)
                {

                    //Debug.Log($"Loading tile x={x}, y={y}");
                    StartCoroutine(mapController.LoadMapTile(x, y, xMin, yMin));
                    counter++;
                }
            }

            yield return null;
        }
    }
}