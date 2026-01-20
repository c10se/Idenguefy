using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Idenguefy.Entities;

namespace Idenguefy.Controllers
{
    /// <summary>
    /// An abstract base class defining the contract for all Dengue Cluster API handlers.
    /// It provides a common interface for fetching and preprocessing data from different sources.
    /// </summary>
    /// <remarks>
    /// Author: Napatr
    /// Version: 1.0
    /// </remarks>
    public abstract class Cluster_API_Handler : MonoBehaviour
    {
        /// <summary>
        /// Gets the list of <see cref="DengueCluster"/> objects that have been parsed by the handler.
        /// </summary>
        public List<DengueCluster> Clusters { get; protected set; } = new List<DengueCluster>();

        /// <summary>
        /// When implemented in a derived class, begins fetching cluster data from the specific API.
        /// </summary>
        /// <returns>An IEnumerator for coroutine execution.</returns>
        public abstract IEnumerator FetchClusterData();

        /// <summary>
        /// When implemented in a derived class, converts a raw JSON string into the <see cref="Clusters"/> list.
        /// </summary>
        /// <param name="datasetJson">The raw dataset JSON string from the API download.</param>
        protected abstract void PreprocessData(string datasetJson);

        /// <summary>
        /// A virtual helper method to determine a cluster's severity based on its case size.
        /// Can be overridden by derived classes if different logic is needed.
        /// </summary>
        /// <param name="caseSize">The number of cases in the cluster.</param>
        /// <returns>A string representing the severity ("High", "Medium", or "Low").</returns>
        protected virtual string DetermineSeverity(int caseSize)
        {
            if (caseSize >= 10) return "High";
            if (caseSize >= 5) return "Medium";
            return "Low";
        }
    }
}