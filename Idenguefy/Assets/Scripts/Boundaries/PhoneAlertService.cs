using Unity.Notifications.Android;
using UnityEngine;
using Idenguefy.Controllers;

namespace Idenguefy.Boundaries
{
    /// <summary>
    /// A standard phone notification class, to alert the user outisde of the app if an alert had been triggered.
    /// Implements IAlertSubscriber to listen to events from the AlertManager.
    /// </summary>
    /// <remarks>
    /// Author: Cheuk Hong, Xavier, Sharina
    /// Version: 1.0
    /// </remarks>
    public class PhoneAlertService : MonoBehaviour, IAlertSubscriber
    {
        /// <summary>
        /// Initializes the service by registering with the AlertManager
        /// and creating the necessary Android notification channel for alerts.
        /// </summary>
        private void Start()
        {
            AlertManager.Instance?.RegisterSubscriber(this);

            // Define the notification channel
            var channel = new AndroidNotificationChannel()
            {
                Id = "alert_channel",
                Name = "Alert Notifications",
                Importance = Importance.High,
                Description = "Dengue alerts"
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);
        }

        /// <summary>
        /// Callback triggered by the AlertManager when a new alert is received.
        /// Sends a phone notification *only* if the application is not currently in focus.
        /// </summary>
        /// <param name="data">The AlertData object containing details about the alert.</param>
        public void OnAlertTriggered(AlertData data)
        {
            if (Application.isPlaying)
            {
                return;
            }

            // Build the notification
            var notification = new AndroidNotification
            {
                Title = data.Title,
                Text = data.Message + "\n" + data.Type.ToString(),
                CustomTimestamp = data.Timestamp,
                FireTime = System.DateTime.Now.AddSeconds(2) // Send 2 seconds from now
            };

            // Send the notification to the "alert_channel"
            AndroidNotificationCenter.SendNotification(notification, "alert_channel");
        }
    }
}