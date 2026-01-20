using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Idenguefy.Entities;

namespace Idenguefy.Controllers
{
    /// <summary>
    /// Handles the management and retrieval of dengue cluster data in the Idenguefy system.
    /// 
    /// Responsibilities:
    /// - Interfaces with <see cref="Cluster_API_Handler"/> to fetch cluster data.
    /// - Provides centralized access to the current list of dengue clusters.
    /// - Supports manual and automatic data refresh.
    /// </summary>
    /// <remarks>
    /// Author: Napatr  
    /// Version: 1.0  
    /// </remarks>
    public class ClusterController : MonoBehaviour
    {
        [Header("Cluster API Handler")]
        [SerializeField] public Cluster_API_Handler apiHandler;

        public List<DengueCluster> Clusters => apiHandler?.Clusters;

        private void Awake()
        {
            if (apiHandler == null)
                apiHandler = GetComponent<Cluster_API_Handler>();

            if (apiHandler == null)
                Debug.LogError("[ClusterController] No API handler found!");
        }

        private void Start()
        {
            // Automatically fetch data on start
            if (apiHandler != null)
                StartCoroutine(FetchClusterData());
        }

        /// <summary>
        /// Public method to fetch cluster data using the attached API handler.
        /// This wraps the API call and can be safely called by other controllers.
        /// </summary>
        public IEnumerator FetchClusterData()
        {
            if (apiHandler == null)
            {
                Debug.LogError("[ClusterController] Cannot fetch data — API handler is missing.");
                yield break;
            }

            Debug.Log("[ClusterController] Fetching cluster data via API handler...");
            yield return StartCoroutine(apiHandler.FetchClusterData());

            Debug.Log($"[ClusterController] Finished fetching clusters: {apiHandler.Clusters.Count} loaded.");
        }

        /// <summary>
        /// Manually refresh cluster data.
        /// </summary>
        public void RefreshClusters()
        {
            if (apiHandler == null)
            {
                Debug.LogError("[ClusterController] No API handler assigned.");
                return;
            }

            apiHandler.Clusters.Clear();
            StartCoroutine(FetchClusterData());
        }

        /// <summary>
        /// Retrieves a specific cluster by ID.
        /// </summary>
        public DengueCluster ReadCluster(string locationID)
        {
            return apiHandler?.Clusters.Find(c => c.LocationID == locationID);
        }
    }
}
