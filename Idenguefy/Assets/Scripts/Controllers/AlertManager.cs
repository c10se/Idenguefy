using Idenguefy.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Idenguefy.Controllers
{
    /// <summary>
    /// Defines a subscriber that listens for alert notifications triggered by the AlertManager.
    /// </summary>
    /// <remarks>
    /// Author: Xavier
    /// Version: 1.0  
    /// </remarks>
    public interface IAlertSubscriber
    {
        /// <summary>
        /// Called when an alert is triggered by the AlertManager.
        /// </summary>
        /// <param name="data">The alert data containing title, message, timestamp, and alert type.</param>
        void OnAlertTriggered(AlertData data);
    }

    /// <summary>
    /// Manages all alert-related functionalities, including evaluating dengue clusters,
    /// monitoring user and pointer locations, saving alerts, and notifying subscribers.
    /// </summary>
    public class AlertManager : MonoBehaviour
    {
        /// <summary>
        /// Singleton instance of the AlertManager.
        /// </summary>
        public static AlertManager Instance { get; private set; }

        /// <summary>
        /// List of subscribers that are notified when alerts are triggered.
        /// </summary>
        private List<IAlertSubscriber> subscribers = new();

        /// <summary>
        /// Reference to the ClusterController managing dengue clusters.
        /// </summary>
        public ClusterController clusterController;

        /// <summary>
        /// Reference to the LiveLocationController tracking the user's location.
        /// </summary>
        public LiveLocationController locationController;

        [Header("Saving Alerts")]
        /// <summary>
        /// List storing all saved alert instances.
        /// </summary>
        private List<Alert> savedAlerts = new List<Alert>();

        /// <summary>
        /// Handles saving and loading alert data persistently.
        /// </summary>
        private DataManager<Alert> alertDataManager = new DataManager<Alert>("Idenguefy_Alerts");

        [Header("Background Checks")]
        /// <summary>
        /// Coroutine responsible for continuously evaluating alerts at set intervals.
        /// </summary>
        private Coroutine backgroundCheckCoroutine;

        /// <summary>
        /// Time interval (in seconds) between each background evaluation.
        /// </summary>
        public float evalInterval = 10f;

        /// <summary>
        /// Tracks the last time an alert was triggered for each pointer or user location.
        /// </summary>
        private readonly Dictionary<string, DateTime> lastNotifyTimes = new();

        /// <summary>
        /// Minimum duration (in seconds) before a repeated alert can be triggered for the same source.
        /// </summary>
        public double durationThreshold = 60.0;

        /// <summary>
        /// Initializes the singleton instance and ensures persistence between scenes.
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.LogWarning("[AlertManager] Instance created and ready.");
            }
            else Destroy(gameObject);
        }

        /// <summary>
        /// Called on start. Loads saved alerts and begins periodic background evaluation.
        /// </summary>
        private void Start()
        {
            LoadSavedAlerts();

            backgroundCheckCoroutine = StartCoroutine(BackgroundEvaluator());
        }

        /// <summary>
        /// Periodically evaluates nearby dengue clusters relative to pointers and user location.
        /// </summary>
        private IEnumerator BackgroundEvaluator()
        {
            while (true)
            {
                EvaluateAlerts();
                yield return new WaitForSeconds(evalInterval);
            }
        }

        /// <summary>
        /// Loads saved alerts from persistent storage.
        /// </summary>
        private void LoadSavedAlerts()
        {
            savedAlerts = alertDataManager.LoadData();
            Debug.Log($"[AlertManager] Loaded {savedAlerts.Count} saved alerts.");
        }

        /// <summary>
        /// Registers a subscriber to receive alert notifications.
        /// </summary>
        /// <param name="sub">The subscriber to be added.</param>
        public void RegisterSubscriber(IAlertSubscriber sub)
        {
            if (!subscribers.Contains(sub)) subscribers.Add(sub);
            Debug.LogWarning($"[AlertManager] Registered {sub.GetType().Name}. Total now: {subscribers.Count}");
        }

        /// <summary>
        /// Unregisters a subscriber from receiving alert notifications.
        /// </summary>
        /// <param name="sub">The subscriber to be removed.</param>
        public void UnregisterSubscriber(IAlertSubscriber sub)
        {
            subscribers.Remove(sub);
            Debug.LogWarning($"Subscriber count (after remove) {subscribers.Count}");
        }

        /// <summary>
        /// Evaluates all active clusters and triggers alerts based on user location and map pointers.
        /// </summary>
        public void EvaluateAlerts()
        {
            Debug.Log($"[AlertManager] Evaluating alerts...");

            // Check dependencies
            if (clusterController == null || clusterController.Clusters.Count == 0)
            {
                Debug.LogWarning("[AlertManager] No clusters available yet.");
                return;
            }

            if (locationController == null || !locationController.IsReady)
            {
                Debug.LogWarning("[AlertManager] LocationController not ready.");
                return;
            }

            // Load pointers
            PointerController pointerController = FindAnyObjectByType<PointerController>();
            if (pointerController != null)
                Debug.Log("[AlertManager] PointerController found dynamically.");
            else
                Debug.LogWarning("[AlertManager] PointerController not found at Start!");

            List<MapPointer> allPointers = new List<MapPointer>();
            if (pointerController != null)
            {
                allPointers = pointerController.ListPointers();
                Debug.Log($"[AlertManager] Loaded {allPointers.Count} pointers for alert evaluation.");
            }
            else
            {
                Debug.LogError($"[PointerController] Missing!");
            }

            (float userLon, float userLat) = locationController.CurrentCoordinates;
            int threshold = new SettingsController().GetThresholdPref();
            Debug.LogWarning($"Threshold = {threshold}");
            double thresholdKm = threshold / 1000.0;

            // Evaluate all pointers
            foreach (var pointer in allPointers)
            {
                foreach (var cluster in clusterController.Clusters)
                {
                    double distance = MapUtils.HaversineDistance(
                        pointer.Coordinates.Item1, pointer.Coordinates.Item2,
                        cluster.Coordinates[0].lon, cluster.Coordinates[0].lat);

                    if (distance < thresholdKm)
                    {
                        DateTime now = DateTime.Now;
                        string key = pointer.Name;

                        if (!lastNotifyTimes.TryGetValue(key, out DateTime lastTime))
                            lastTime = DateTime.MinValue;

                        if ((now - lastTime).TotalSeconds >= durationThreshold)
                        {
                            lastNotifyTimes[key] = now;

                            AlertType type = pointer.HomeTag ? AlertType.Indoor : AlertType.Outdoor;

                            AlertData data = new AlertData
                            {
                                Title = pointer.HomeTag ? "Home Alert!" : "Pointer Alert!",
                                Message = $"{cluster.AreaName} cluster detected within {threshold}m of pointer '{pointer.Name}'",
                                Timestamp = now,
                                Type = type
                            };

                            Debug.Log($"[Alert Manager] Triggering alert for {pointer.Name}");
                            NotifySubscribers(data);
                        }
                    }
                }
            }

            // Evaluate user's live location (Outdoor only)
            foreach (var cluster in clusterController.Clusters)
            {
                double distance = MapUtils.HaversineDistance(
                    userLon, userLat,
                    cluster.Coordinates[0].lon, cluster.Coordinates[0].lat);

                Debug.Log($"LIVE LOC coords: {userLon}, {userLat} compare with " +
                    $"cluster coords: {cluster.Coordinates[0].lon}, {cluster.Coordinates[0].lat}");

                Debug.LogError($"DISTANCE LIVELOC ARGHHHHHHH {distance}, THRESHOLDKM {thresholdKm}");

                if (distance < thresholdKm)
                {
                    DateTime now = DateTime.Now;
                    string key = "LIVE";

                    if (!lastNotifyTimes.TryGetValue(key, out DateTime lastTime))
                        lastTime = DateTime.MinValue;

                    if ((now - lastTime).TotalSeconds >= durationThreshold)
                    {
                        lastNotifyTimes[key] = now;

                        AlertData data = new AlertData
                        {
                            Title = "Nearby Dengue Cluster!",
                            Message = $"{cluster.AreaName} cluster detected within {threshold}m of your current location",
                            Timestamp = now,
                            Type = AlertType.Outdoor
                        };

                        Debug.Log($"[Alert Manager] Triggering live location alert");
                        NotifySubscribers(data);
                    }
                }
            }

            Debug.Log("[AlertManager] Evaluation complete.");
        }

        /// <summary>
        /// Notifies all registered subscribers about a triggered alert.
        /// </summary>
        /// <param name="data">The alert data to broadcast to subscribers.</param>
        private void NotifySubscribers(AlertData data)
        {
            SaveAlert(data);

            foreach (var sub in subscribers)
                sub.OnAlertTriggered(data);
        }

        /// <summary>
        /// Saves the given alert data to memory and persistent storage.
        /// </summary>
        /// <param name="data">The alert data to be saved.</param>
        private void SaveAlert(AlertData data)
        {
            Alert newAlert = new Alert(
                alertID: Guid.NewGuid().ToString(),
                title: data.Title,
                message: data.Message,
                timestamp: data.Timestamp,
                type: data.Type
            );

            savedAlerts.Add(newAlert);
            alertDataManager.SaveData(savedAlerts);
            Debug.Log($"[AlertManager] Saved alert: {newAlert.Title} - {newAlert.Message} - Type:{newAlert.Type}");
        }

        /// <summary>
        /// Retrieves a copy of all saved alerts.
        /// </summary>
        /// <returns>A list of all saved alerts.</returns>
        public List<Alert> GetSavedAlerts()
        {
            if (savedAlerts == null)
                savedAlerts = new List<Alert>();

            if (savedAlerts.Count == 0 && PlayerPrefs.HasKey("Idenguefy_Alerts"))
            {
                savedAlerts = alertDataManager.LoadData();
                Debug.LogWarning($"[AlertManager] Loaded {savedAlerts} alerts from PlayerPrefs (fallback).");
            }

            return new List<Alert>(savedAlerts);
        }

        /// <summary>
        /// Clears all saved alerts from memory and persistent storage.
        /// </summary>
        public void ClearSavedAlerts()
        {
            savedAlerts.Clear();
            alertDataManager.SaveData(savedAlerts);
            Debug.Log("[AlertManager] Cleared all saved alerts.");
        }
    }

    /// <summary>
    /// Represents the data structure for alerts, including title, message, timestamp, and alert type.
    /// </summary>
    public struct AlertData
    {
        public string Title;
        public string Message;
        public DateTime Timestamp;
        public AlertType Type;
    }
}
