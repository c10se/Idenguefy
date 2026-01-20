using Idenguefy.Controllers;
using Idenguefy.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;

namespace Idenguefy.Boundaries
{
    /// <summary>
    /// The main interface in which the listed UI objects are loaded onto, in order:
    /// <list type="number">
    /// <item><description>Map Tiles</description></item>
    /// <item><description>Clusters</description></item>
    /// <item><description>Saved Pointers (If any)</description></item>
    /// <item><description>Live Location</description></item>
    /// </list>
    /// Includes functionality to recenter back onto the users live location or a searched location
    /// Includes a toggleable function to read the users touch input to detect when a user is pressing and holding down on the screen
    /// </summary>
    /// <remarks>
    /// Author: Eben, Sharina, Cheuk Hong, Xavier
    /// Version: 1.2
    /// </remarks>
    public class MapView : MonoBehaviour
    {
        [Header("Controllers")]
        [Tooltip("Reference to the MapController.")]
        public MapController mapController;
        [Tooltip("Reference to the ClusterController.")]
        public ClusterController clusterController;
        [Tooltip("Reference to the LiveLocationController.")]
        public LiveLocationController liveLocationController;
        [Tooltip("Reference to the PointerController.")]
        public PointerController pointerController;

        [Header("Renderers")]
        [Tooltip("Reference to the renderer for the live location marker.")]
        public LiveLocationRenderer locationRenderer;
        [Tooltip("Reference to the renderer for dengue clusters.")]
        public ClusterRenderer clusterRenderer;
        [Tooltip("Reference to the renderer for saved user pointers.")]
        public PointerRenderer pointerRenderer;
        [Tooltip("Reference to the renderer for map tiles.")]
        public MapTileRenderer tileRenderer;

        [Header("Map View Reference")]
        [Tooltip("The RectTransform of the main map view, which is scaled for zooming.")]
        public RectTransform mapView;
        [Tooltip("The UI button used to recenter the map on the user's location.")]
        public Button recenterButton;

        [Header("Other References")]
        [Tooltip("The TMP_Text component used to display loading status (e.g., 'Loading Tiles...').")]
        public TextMeshProUGUI loadingText;
        [Tooltip("A UI panel activated during map loading to prevent user interaction.")]
        public GameObject blockingPanel;
        [Tooltip("A UI text element shown to instruct the user when 'place pointer' mode is active.")]
        public GameObject placePointerText;

        [Header("View References")]
        [Tooltip("Reference to the PointerView UI, used for handling long-press events.")]
        public PointerView pointerView;

        /*Relevant Pointer fields*/
        /// <summary>Internal state for pointer creation (e.g., home tag).</summary>
        Boolean hometagBool;
        /// <summary>Flag that becomes true when a long press is successfully detected.</summary>
        bool isHolding;
        /// <summary>The duration (in seconds) a user must press and hold to trigger a long press event.</summary>
        double longPressTime = 1;
        /// <summary>The screen coordinate (in pixels) where the long press was detected.</summary>
        public Vector2 TouchCoord;
        /// <summary>A tuple to store geographic coordinates (unused).</summary>
        (float, float) Coord;
        /// <summary>A boolean flag that enables or disables the ReadTouch method for long-press detection.</summary>
        private bool enableReadTouch = false;

        /// <summary>
        /// Initializes the map view, enables enhanced touch support,
        /// ensures the PointerController exists, and hooks up the recenter button.
        /// </summary>
        public void Start()
        {
            RefreshMap();
            EnhancedTouchSupport.Enable(); // Enable new input system for touch

            // Ensure PointerController exists
            if (pointerController == null)
            {
                GameObject pointerControllerObj = new GameObject("PointerController");
                pointerController = pointerControllerObj.AddComponent<PointerController>();
            }

            // Hook up the recenter button listener
            if (recenterButton != null)
                recenterButton.onClick.AddListener(() =>
                {
                    var location = locationRenderer.liveMarkerInstance.GetComponent<RectTransform>().anchoredPosition;
                    RecenterAndZoom((Vector3)location);
                });
        }

        /// <summary>
        /// Calls the ReadTouch method every frame to check for long-press gestures.
        /// </summary>
        private void Update()
        {
            ReadTouch();
        }

        /// <summary>
        /// Public method to start the full map rendering process.
        /// </summary>
        public void RefreshMap()
        {
            StartCoroutine(RenderMap());
        }

        /// <summary>
        /// A coroutine that renders all map layers sequentially:
        /// Tiles, Clusters, Pointers, and finally Live Location.
        /// Updates a loading text and blocks UI interaction during this process.
        /// </summary>
        /// <returns>An IEnumerator for the coroutine.</returns>
        public IEnumerator RenderMap()
        {
            loadingText.gameObject.SetActive(true);
            blockingPanel.SetActive(true);

            mapController.FindTilesBoundary(out int xMin, out int xMax, out int yMin, out int yMax);

            loadingText.text = "Loading Tiles...";
            yield return StartCoroutine(tileRenderer.RenderMapTiles());
            yield return null; // Wait a frame for tiles to begin loading

            loadingText.text = "Fetching Dengue Clusters";
            yield return StartCoroutine(clusterController.FetchClusterData());

            loadingText.text = "Drawing Clusters...";
            yield return StartCoroutine(clusterRenderer.LoadAllClusters(xMin, yMin));

            loadingText.text = "Drawing Pointers...";
            yield return StartCoroutine(pointerRenderer.LoadAllPointers(xMin, yMin));

            loadingText.text = "Map Ready!";

            //Live Location rendering (runs concurrently after main load)
            StartCoroutine(locationRenderer.RenderLiveLocation(xMin, yMin));

            loadingText.gameObject.SetActive(false);
            blockingPanel.SetActive(false);
        }

        /// <summary>
        /// Recenters the map panel to a specific geographic coordinate (longitude, latitude).
        /// </summary>
        /// <param name="lon">The target longitude.</param>
        /// <param name="lat">The target latitude.</param>
        public void RecenterAndZoom(float lon, float lat)
        {
            RectTransform mapPanel = mapController.mapPanelRect;
            // Convert geo-coordinate to a pixel position on the map
            Vector3 pixelCoord = (Vector3)mapController.GeoToScreen(lon, lat);

            // Adjust for the panel's center pivot
            Vector3 mapPanelCenter = new Vector3(mapPanel.sizeDelta.x / 2f, -mapPanel.rect.height / 2f, 0);
            Vector3 mapViewScale = mapView.localScale; // Not used here, but relevant for zoom
            pixelCoord = pixelCoord - mapPanelCenter;

            // Set the panel's position to the negative of the target coordinate
            mapPanel.transform.localPosition = -pixelCoord;
            Debug.Log($"[MapView] Recentering map to lon:{lon}, lat:{lat} at pixel coord {pixelCoord}");
        }

        /// <summary>
        /// Overload method that recenters the map panel to a specific *local pixel position*
        /// relative to the map panel's center.
        /// </summary>
        /// <param name="position">The target local anchored position (relative to center).</param>
        public void RecenterAndZoom(Vector3 position)
        {
            RectTransform mapPanel = mapController.mapPanelRect;
            mapPanel.transform.localPosition = -position;
            Debug.Log($"[MapView] Recentering map to pixel coord {position}");
        }

        /// <summary>
        /// Detects a long press (press and hold) on the touchscreen
        /// if enableReadTouch is true. On detection, it calls PointerView.OnLongPressDetected.
        /// </summary>
        public void ReadTouch()
        {
            if (!enableReadTouch)
            {
                return; // Do nothing if touch reading is disabled
            }

            if (Touchscreen.current != null)
            {
                foreach (var t in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
                {
                    //Debug.Log($"Value of touch phase upon touch: {t.phase}");
                    switch (t.phase)
                    {
                        case UnityEngine.InputSystem.TouchPhase.Began:
                            isHolding = false;
                            break;
                        case UnityEngine.InputSystem.TouchPhase.Moved:
                        case UnityEngine.InputSystem.TouchPhase.Stationary:
                            double holdTimer = t.time - t.startTime;
                            if (holdTimer >= longPressTime && !isHolding)
                            {
                                // Long press detected
                                isHolding = true;
                                TouchCoord = t.screenPosition;
                                //Coord = (TouchCoord.x, TouchCoord.y); // Unused
                                pointerView.OnLongPressDetected(TouchCoord);
                            }
                            break;
                        case UnityEngine.InputSystem.TouchPhase.Ended:
                        case UnityEngine.InputSystem.TouchPhase.Canceled:
                            // Reset hold state
                            isHolding = false;
                            holdTimer = 0;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Toggles the enableReadTouch flag and updates the visibility
        /// of the placePointerText UI element.
        /// </summary>
        public void ToggleReadTouch()
        {
            enableReadTouch = !enableReadTouch;
            placePointerText.SetActive(!placePointerText.activeSelf);
        }

    }
}