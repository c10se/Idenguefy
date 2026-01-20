using System.Collections;
using UnityEngine;

namespace Idenguefy.Controllers
{
    /// <summary>
    /// Manages live GPS location data for the Idenguefy application.
    /// 
    /// Responsibilities:
    /// <list type="number">
    /// <item>Retrieves the user’s real-time GPS coordinates using Unity’s location services.</item>
    /// <item>Provides mock location support for testing within the Unity Editor.</item>
    /// <item>Exposes a public interface to access the user’s current location and readiness state.</item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Author: Xavier
    /// Version: 1.0  
    /// This controller provides a single point of access for location-based data, ensuring
    /// that other systems (e.g., AlertManager, Map utilities) can safely query the user’s coordinates.
    /// </remarks>
    public class LiveLocationController : MonoBehaviour
    {
        /// <summary>
        /// The most recently retrieved coordinates of the device, represented as (longitude, latitude).
        /// </summary>
        public (float lon, float lat) CurrentCoordinates { get; private set; }

        /// <summary>
        /// Indicates whether the location service or mock data is ready for use.
        /// </summary>
        public bool IsReady { get; private set; } = false;

        [Header("Testing Options")]
        /// <summary>
        /// Enables the use of mock coordinates when testing within the Unity Editor.
        /// </summary>
        public bool useMockLocationInEditor = true;

        /// <summary>
        /// The mock GPS coordinates (longitude, latitude) used for testing purposes in the Editor.
        /// Defaults to Singapore’s approximate central coordinates.
        /// </summary>
        public Vector2 mockCoordinates = new Vector2(103.6831f, 1.3483f); // Singapore (test coordinates)

        /// <summary>
        /// Coroutine that initializes the device’s location service and retrieves the initial coordinates.
        /// Falls back to mock coordinates when location access is unavailable or disabled.
        /// </summary>
        /// <returns>An IEnumerator coroutine for Unity’s location initialization sequence.</returns>
        private IEnumerator Start()
        {
#if UNITY_EDITOR
            if (useMockLocationInEditor)
            {
                Debug.LogWarning("[LiveLocationController] Using mock coordinates in Editor.");
                CurrentCoordinates = (mockCoordinates.x, mockCoordinates.y);
                IsReady = true;
                yield break;
            }
#endif
            // Verify that location services are enabled
            if (!Input.location.isEnabledByUser)
            {
                Debug.LogWarning("[LiveLocationController] Location services not enabled by user. Using fallback coordinates.");
                CurrentCoordinates = (mockCoordinates.x, mockCoordinates.y);
                IsReady = true;
                yield break;
            }

            Input.location.Start();

            int maxWait = 20;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                yield return new WaitForSeconds(1);
                maxWait--;
            }

            // Handle failure to acquire location
            if (Input.location.status == LocationServiceStatus.Failed)
            {
                Debug.LogError("[LiveLocationController] Unable to determine device location. Using fallback coordinates.");
                CurrentCoordinates = (mockCoordinates.x, mockCoordinates.y);
                IsReady = true;
                yield break;
            }

            IsReady = true;
            UpdateLocation();
        }

        /// <summary>
        /// Updates the current location data from Unity’s location service.
        /// This method should be called periodically to refresh coordinates.
        /// </summary>
        public void UpdateLocation()
        {
            if (!IsReady) return;

            LocationInfo loc = Input.location.lastData;
            CurrentCoordinates = (loc.longitude, loc.latitude);

            Debug.Log($"[LiveLocationController] Current coords: ({loc.longitude}, {loc.latitude})");
        }
    }
}
