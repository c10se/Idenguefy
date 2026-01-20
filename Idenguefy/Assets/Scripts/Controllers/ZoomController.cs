using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;


namespace Idenguefy.Controllers
{
    /// <summary>
    /// Handles zoom functionality for the map view, compatible with both desktop and mobile
    /// 
    /// Features:
    /// - Mouse wheel zoom (for Editor testing)
    /// - Pinch zoom (multi-touch support using the new Input System)
    /// - Integrated with ScrollRect for smooth panning + zooming
    /// - Adjustable zoom limits (minZoom, maxZoom) to prevent excessive scaling
    /// - Automatically disables ScrollRect while pinch zooming to avoid input conflicts
    /// 
    /// Works best when attached to the ScrollRect content (MapView)
    /// </summary>
    /// <remarks>
    /// Author: Xavier
    /// Version: 1.0
    /// </remarks>
    public class ZoomController : MonoBehaviour
    {
        [Header("Zoom Target")]
        [Tooltip("Assign MapView (the ScrollRect content) here. This is the object that will be scaled.")]
        public RectTransform mapView;

        [Header("Zoom Controls")]
        [Tooltip("The speed multiplier for pinch-to-zoom gestures.")]
        public float zoomSpeed;

        [Header("Zoom Limits")]
        [Tooltip("The minimum scale factor (e.g., 0.5 for 50% zoom out).")]
        public float minZoom;  //Furthest out (e.g., whole Singapore fits)
        [Tooltip("The maximum scale factor (e.g., 3.0 for 300% zoom in).")]
        public float maxZoom;   //Closest in

        /// <summary>
        /// The current scale factor of the map view.
        /// </summary>
        private float currentZoom = 1f;

        [Header("Dependencies")]
        [Tooltip("Assign the parent ScrollRect component. This is disabled during pinch-zoom to prevent conflicts.")]
        public ScrollRect scrollRect;

        /// <summary>
        /// Called every frame. Checks for mouse scroll or multi-touch input to apply zoom.
        /// </summary>
        void Update()
        {
            // --- Mouse wheel (dev testing) ---
            if (Mouse.current != null)
            {
                float scroll = Mouse.current.scroll.ReadValue().y;
                if (scroll != 0)
                {
                    // Zoom in (1.1x) or out (0.9x) based on scroll direction
                    Zoom(scroll > 0 ? 1.1f : 0.9f);
                }
            }

            // --- Pinch zoom (mobile) ---
            if (Touchscreen.current != null)
            {
                List<TouchControl> activeTouches = new List<TouchControl>();
                foreach (var t in Touchscreen.current.touches)
                {
                    if (t.press.isPressed) activeTouches.Add(t);
                }

                //Debug.Log($"Active touches: {activeTouches.Count}");

                // Pinch zoom when at least 2 touches are active
                if (activeTouches.Count >= 2)
                {
                    // Disable the ScrollRect to prevent panning while pinching
                    scrollRect.enabled = false;

                    var t0 = activeTouches[0];
                    var t1 = activeTouches[1];

                    // Get current and previous touch positions
                    Vector2 pos0 = t0.position.ReadValue();
                    Vector2 pos1 = t1.position.ReadValue();
                    Vector2 prev0 = pos0 - t0.delta.ReadValue();
                    Vector2 prev1 = pos1 - t1.delta.ReadValue();

                    // Calculate the distance between touches in this frame and the previous frame
                    float prevDist = Vector2.Distance(prev0, prev1);
                    float curDist = Vector2.Distance(pos0, pos1);

                    // Calculate the change in distance and apply zoom
                    float delta = curDist - prevDist;
                    Zoom(1 + delta * zoomSpeed * Time.deltaTime);
                }
                else
                {
                    // Re-enable the ScrollRect when not pinching
                    scrollRect.enabled = true;
                }
            }

        }

        /// <summary>
        /// Applies a zoom factor to the map view, clamping the result within min/max zoom limits.
        /// </summary>
        /// <param name="factor">The multiplier for the zoom (e.g., 1.1 for 10% zoom in).</param>
        private void Zoom(float factor)
        {
            // Clamp the new zoom level between the defined min and max
            currentZoom = Mathf.Clamp(currentZoom * factor, minZoom, maxZoom);
            mapView.localScale = new Vector3(currentZoom, currentZoom, 1);
        }
    }
}