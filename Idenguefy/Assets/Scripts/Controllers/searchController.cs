using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Idenguefy.Entities;

namespace Idenguefy.Controllers
{
    /// <summary>
    /// Handles user input for map searches, interacts with the SearchAPIHandler,
    /// and stores the retrieved search results.
    /// </summary>
    /// <remarks>
    /// Author: Eben, Sharina
    /// Version: 1.0
    /// Notes: N/A
    /// </remarks>
    public class SearchController : MonoBehaviour
    {
        [Header("API Handler")]
        [Tooltip("Handles requests to the search API.")]
        [SerializeField]
        private SearchAPIHandler searchAPIHandler;

        [Header("Search Parameters")]
        [Tooltip("The minimum relevance score required for a result to be included.")]
        public float minSearchRelevance = 0;

        [Header("Search Results")]
        [Tooltip("Stores the results from the last executed search.")]
        public List<MapSearchResult> searchResultList;

        /// <summary>
        /// Initializes the search controller and the search result list on start.
        /// </summary>
        void Start()
        {
            Debug.Log("[SearchController] SearchController initialized.");
            searchResultList = new List<MapSearchResult>();
        }

        /// <summary>
        /// Public coroutine to initiate a search.
        /// It clears previous results, calls the SearchAPIHandler, and populates the searchResultList.
        /// </summary>
        /// <param name="query">The search string entered by the user.</param>
        /// <returns>An IEnumerator for coroutine execution.</returns>
        public IEnumerator HandleSearch(string query)
        {
            searchResultList.Clear();
            if (query == null || query.Trim() == "") yield break;

            Debug.Log($"[SearchController] Initiated search for query: {query}");
            yield return searchAPIHandler.FetchSearchByQuery(query);
            Debug.Log($"[SearchController] SearchCompleted! There are {searchResultList.Count} found result(s).");

            foreach (var item in SearchAPIHandler.response.results)
            {
                // Note: Consider filtering based on minSearchRelevance here if needed
                searchResultList.Add(item);
                Debug.Log($"[SearchController] Result: {item.name} - {item.detail} (Relevance: {item.relevance})");
            }
        }
    }
}