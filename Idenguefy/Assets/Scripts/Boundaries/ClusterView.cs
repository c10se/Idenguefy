using Idenguefy.Controllers;
using Idenguefy.Entities;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Idenguefy.Boundaries
{
    /// <summary>
    /// Contains all logic related to interacting with additional Cluster UI.
    /// <list type="number">
    ///     <item><description> Shows all Dengue Clusters on a popup screen</description></item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Author: Xavier
    /// Version: 1.0
    /// </remarks>
    public class ClusterView : MonoBehaviour
    {
        [Header("Controller Reference")]
        [Tooltip("Reference to the main controller holding the cluster data.")]
        public ClusterController clusterController;

        [Header("UI References")]
        [Tooltip("The ScrollRect content object that will contain the cluster entry prefabs.")]
        public RectTransform clusterEntryContainer;     // Parent under ScrollView
        [Tooltip("The prefab for a single cluster list item.")]
        public GameObject clusterEntryPrefab;         // Prefab with TMP_Texts
        [Tooltip("A text object shown when the cluster list is empty.")]
        public GameObject emptyStateText;           // “No clusters” text

        [Header("Boundary References")]
        [Tooltip("Reference to the ClusterRenderer, used for focusing on a cluster (future use).")]
        public ClusterRenderer clusterRenderer;

        /// <summary>
        /// A list of the currently instantiated cluster entry GameObjects in the view.
        /// </summary>
        private readonly List<GameObject> clusterEntries = new();

        /// <summary>
        /// When the component is enabled, automatically refresh the cluster list.
        /// </summary>
        private void OnEnable()
        {
            RefreshClusterList();
        }

        /// <summary>
        /// Show the cluster list panel and refresh contents.
        /// </summary>
        public void ShowClusterList()
        {
            RefreshClusterList();
            Debug.Log("[ClusterView] Cluster list shown.");
        }

        /// <summary>
        /// Clears old cluster entries and rebuilds from ClusterController data.
        /// </summary>
        private void RefreshClusterList()
        {
            // Clear old entries
            foreach (var obj in clusterEntries)
                Destroy(obj);
            clusterEntries.Clear();

            if (clusterController == null)
            {
                Debug.LogWarning("[ClusterView] ClusterController not assigned!");
                return;
            }

            List<DengueCluster> clusters = clusterController.Clusters;

            // Handle empty state
            if (clusters == null || clusters.Count == 0)
            {
                if (emptyStateText != null)
                    emptyStateText.SetActive(true);
                Debug.Log("[ClusterView] No clusters to display.");
                return;
            }

            if (emptyStateText != null)
                emptyStateText.SetActive(false);

            // Populate list with new entries
            foreach (DengueCluster cluster in clusters)
            {
                GameObject entry = Instantiate(clusterEntryPrefab, clusterEntryContainer);
                TMP_Text[] texts = entry.GetComponentsInChildren<TMP_Text>();

                // Find and populate text fields by name
                foreach (TMP_Text txt in texts)
                {
                    string lower = txt.name.ToLower();
                    if (lower.Contains("name"))
                    {
                        txt.text = $"<b>{cluster.AreaName}</b>\n";
                    }
                    if (lower.Contains("details"))
                    {
                        txt.text =
                            $"Cases: {cluster.CaseSize}\n" +
                            $"Severity: {cluster.Severity}\n" +
                            $"Location ID: {cluster.LocationID}";
                    }
                }

                clusterEntries.Add(entry);
            }

            Debug.Log($"[ClusterView] Displayed {clusters.Count} clusters.");
        }
    }
}