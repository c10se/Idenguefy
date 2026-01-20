using Idenguefy.Controllers;
using System.Collections;
using UnityEngine;

namespace Idenguefy.Boundaries
{
    /// <summary>
    /// Takes in GPS information to identify users current location and display it on the UI map 
    /// </summary>
    /// <remarks>
    /// Author: Xavier
    /// Version: 1.0
    /// </remarks>
    public class LiveLocationRenderer : MonoBehaviour
    {
        [Tooltip("The controller responsible for fetching the device's GPS coordinates.")]
        public LiveLocationController liveLocationController;
        [Tooltip("The prefab used to represent the user's live location on the map.")]
        public GameObject liveMarkerPrefab;
        [Tooltip("A reference to the instantiated live marker, if one exists.")]
        public GameObject liveMarkerInstance;
        [Tooltip("Reference to the main MapController for coordinate conversion and parenting.")]
        public MapController mapController;

        /// <summary>
        /// Coroutine to render the live location marker on the map.
        /// Waits for the LiveLocationController to be ready, then calculates
        /// the marker's position and instantiates it.
        /// </summary>
        /// <param name="xMin">The minimum X tile index of the current map view.</param>
        /// <param name="yMin">The minimum Y tile index of the current map view.</param>
        /// <returns>An IEnumerator for coroutine execution.</returns>
        public IEnumerator RenderLiveLocation(int xMin, int yMin)
        {
            // Wait until the controller is initialized and has a valid location
            yield return new WaitUntil(() => liveLocationController != null && liveLocationController.IsReady);

            (float lon, float lat) = liveLocationController.CurrentCoordinates;

            // Convert GPS coordinates to pixel coordinates relative to the map tiles
            MapUtils.LonLatToPixelRatio(lon, lat, mapController.ZOOM, xMin, yMin,
                out double xRatio, out double yRatio);

            float xPixel = (float)(xRatio * mapController.TILE_SIZE);
            float yPixel = (float)(yRatio * mapController.TILE_SIZE);

            // Calculate the final anchored position within the map panel
            // Note: This offset calculation seems specific to a center-anchored panel.
            Vector2 offset = new Vector2(mapController.mapPanelRect.sizeDelta.x / 2f, -mapController.mapPanelRect.sizeDelta.y / 2f);
            Vector2 anchoredPos = new Vector2(xPixel, yPixel) - offset;

            // Instantiate the marker as a child of the map panel
            liveMarkerInstance = Instantiate(liveMarkerPrefab, mapController.mapPanelRect);
            liveMarkerInstance.GetComponent<RectTransform>().anchoredPosition = anchoredPos;

            Debug.Log($"[MapController] Live location rendered at {anchoredPos}");
        }
    }
}