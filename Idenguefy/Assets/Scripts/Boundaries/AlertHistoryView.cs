using Idenguefy.Controllers;
using Idenguefy.Entities;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Idenguefy.Boundaries
{
    /// <summary>
    /// Displays a scrollable list of saved alerts (from AlertManager)
    /// 
    /// Responsibilities:
    /// <list type="number">
    /// <item>Fetch saved alerts from AlertManager</item>
    /// <item>Instantiate alert UI prefabs dynamically</item>
    /// <item>Allow users to clear all alerts if desired</item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Author: Xavier
    /// Version: 1.0
    /// </remarks>

    public class AlertHistoryView : MonoBehaviour, IAlertSubscriber
    {
        [Header("UI References")]
        public RectTransform alertContainer;     //parent under a ScrollView
        public GameObject alertItemPrefab;       //prefab with TMP_Text fields
        public GameObject emptyStateText;        //“No saved alerts” text

        private List<GameObject> activeAlertItems = new();

        private void Start()
        {
            RefreshAlertList();
            StartCoroutine(EnsureRegistered());
        }
        /// <summary>
        /// Sets up and ensures AlertHistoryView Class is subscribed to and observes the AlertManager Class via an IEnumerator
        /// </summary>
        /// <returns>
        /// </returns>
        private System.Collections.IEnumerator EnsureRegistered()
        {
            //Wait until AlertManager.Instance exists because for some reason Awake() runs slower than OnEnable
            yield return new WaitUntil(() => AlertManager.Instance != null);
            AlertManager.Instance.RegisterSubscriber(this);
            Debug.Log("[AlertHistoryView] Registered successfully after AlertManager ready.");
        }

        /// <summary>
        /// Clears current UI and rebuilds it from the AlertManager’s saved list.
        /// </summary>
        public void RefreshAlertList()
        {
            // Remove existing UI items
            foreach (var obj in activeAlertItems)
                Destroy(obj);
            activeAlertItems.Clear();

            // Get saved alerts
            if (AlertManager.Instance == null)
            {
                Debug.LogWarning("[AlertHistoryView] AlertManager not ready.");
                return;
            }

            List<Alert> alerts = AlertManager.Instance.GetSavedAlerts();
            if (alerts == null || alerts.Count == 0)
            {
                if (emptyStateText != null)
                    emptyStateText.SetActive(true);
                return;
            }

            if (emptyStateText != null)
                emptyStateText.SetActive(false);

            foreach (Alert alert in alerts)
            {
                Debug.LogWarning($"{alert.Title}, {alert.Message}, {alert.Timestamp}");
            }

            //Instantiate alert items
            foreach (Alert alert in alerts)
            {
                GameObject item = Instantiate(alertItemPrefab, alertContainer);
                TMP_Text[] texts = item.GetComponentsInChildren<TMP_Text>();

                foreach (TMP_Text txt in texts)
                {
                    if (txt.name.ToLower().Contains("title"))
                        txt.text = alert.Title;
                    else if (txt.name.ToLower().Contains("message"))
                        txt.text = alert.Message;
                    else if (txt.name.ToLower().Contains("timestamp"))
                        txt.text = alert.Timestamp.ToString("dd MMM yyyy, HH:mm");
                    else if (txt.name.ToLower().Contains("type"))
                        txt.text = "Alert Type: "  + alert.Type.ToString();
                }

                activeAlertItems.Add(item);
            }

            Debug.Log($"[AlertHistoryView] Displayed {alerts.Count} saved alerts.");
        }

        /// <summary>
        /// Clears all saved alerts from memory and storage.
        /// </summary>
        public void ClearAllAlerts()
        {
            if (AlertManager.Instance == null) return;

            AlertManager.Instance.ClearSavedAlerts();
            RefreshAlertList();

            Debug.Log("[AlertHistoryView] Cleared all saved alerts and refreshed UI.");
        }
        /// <summary>
        /// Allows this instance of the subscriber to perform actions upon an alert triggering.
        /// </summary>
        /// <param name="data"></param>
        public void OnAlertTriggered(AlertData data)
        {
            RefreshAlertList();
        }
    }
}
