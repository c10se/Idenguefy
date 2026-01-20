using Idenguefy.Controllers;
using Idenguefy.Entities;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Idenguefy.Boundaries
{
    /// <summary>
    /// Displays the alerts detected in an InApp popup window
    /// The number of alerts that can be displayed in a window at once can be adjusted
    /// Includes options to view different preventative health tips based on the alert type (indoor/outdoor)
    /// </summary>
    /// <remarks>
    /// Author: Xavier
    /// Version: 1.3
    /// </remarks>
    public class InAppAlertService : MonoBehaviour, IAlertSubscriber
    {
        [Header("UI References")]
        [Tooltip("The ScrollRect content object that will contain the alert prefabs.")]
        public RectTransform alertContainer;    //ScrollView Content
        [Tooltip("The prefab for a single alert item (must contain TMP_Text and Button children).")]
        public GameObject alertItemPrefab;     //Prefab with texts + button
        [Tooltip("Reference to the view that shows health tips.")]
        public PreventiveHealthTipsView healthTipsView;

        [Header("Config")]
        [Tooltip("Maximum number of visible alerts in the scroll list")]
        public int maxVisibleAlerts = 10;

        /// <summary>
        /// A list of the currently instantiated alert GameObjects in the view.
        /// </summary>
        private readonly List<GameObject> activeAlerts = new();

        /// <summary>
        /// When enabled, starts a coroutine to register with the AlertManager.
        /// </summary>
        private void OnEnable()
        {
            StartCoroutine(EnsureRegistered());
        }

        /// <summary>
        /// Coroutine that waits until the AlertManager instance is available,
        /// then registers this object as a subscriber.
        /// </summary>
        private System.Collections.IEnumerator EnsureRegistered()
        {
            yield return new WaitUntil(() => AlertManager.Instance != null);
            AlertManager.Instance.RegisterSubscriber(this);
            Debug.Log("[InAppAlertService] Registered successfully.");
        }

        /// <summary>
        /// When disabled or destroyed, unregisters from the AlertManager
        /// to prevent null reference exceptions.
        /// </summary>
        private void OnDisable()
        {
            if (AlertManager.Instance != null)
                AlertManager.Instance.UnregisterSubscriber(this);
        }

        /// <summary>
        /// Callback method triggered by the AlertManager when a new alert is received.
        /// This method instantiates and populates a new alert item prefab.
        /// </summary>
        /// <param name="data">The AlertData object containing information about the alert.</param>
        public void OnAlertTriggered(AlertData data)
        {
            // If the alert panel isn't open, open it and clear any old alerts.
            if (!AnimationManager.Instance.inAppAlertIsExpanded)
            {
                //Refresh alert items to prevent duplicates
                foreach (var obj in activeAlerts)
                    Destroy(obj);
                activeAlerts.Clear();

                AnimationManager.Instance.ToggleInAppAlertView();
            }

            // Instantiate new alert card
            GameObject alertObj = Instantiate(alertItemPrefab, alertContainer);
            TMP_Text[] texts = alertObj.GetComponentsInChildren<TMP_Text>();

            // Find and populate text fields by name
            foreach (TMP_Text txt in texts)
            {
                string name = txt.name.ToLower();
                if (name.Contains("title"))
                    txt.text = data.Title;
                else if (name.Contains("message"))
                    txt.text = data.Message;
                else if (name.Contains("timestamp"))
                    txt.text = data.Timestamp.ToString("dd MMM yyyy, HH:mm");
                else if (txt.name.ToLower().Contains("type"))
                    txt.text = "Alert Type: " + data.Type.ToString();
            }

            // Hook up Show More button
            Button showMoreButton = alertObj.GetComponentInChildren<Button>();
            if (showMoreButton != null)
            {
                showMoreButton.onClick.RemoveAllListeners();
                showMoreButton.onClick.AddListener(() =>
                {
                    // Show the correct health tips based on the alert type
                    if (data.Type == AlertType.Indoor)
                        AnimationManager.Instance.ToggleIndoorHealthTips();
                    //healthTipsView.ShowIndoorTips();
                    else
                        AnimationManager.Instance.ToggleOutdoorHealthTips();
                    //healthTipsView.ShowOutdoorTips();
                });
            }

            activeAlerts.Add(alertObj);
            EnforceMaxVisibleAlerts();

            Debug.Log($"[InAppAlertService] Displayed {data.Type} alert: {data.Title}");
        }

        /// <summary>
        /// Sets a limit on the number of alerts visible via the window at a given time.
        /// Removes the oldest alerts if the count exceeds maxVisibleAlerts.
        /// </summary>
        private void EnforceMaxVisibleAlerts()
        {
            while (activeAlerts.Count > maxVisibleAlerts)
            {
                GameObject oldest = activeAlerts[0];
                activeAlerts.RemoveAt(0);
                Destroy(oldest);
            }
        }

        /// <summary>
        /// Clearing the window of all in app alerts.
        /// Destroys all instantiated alert GameObjects.
        /// </summary>
        public void ClearAllInAppAlerts()
        {
            foreach (var alertObj in activeAlerts)
                Destroy(alertObj);
            activeAlerts.Clear();
        }
    }
}