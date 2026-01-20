using Idenguefy.Controllers;
using Idenguefy.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Idenguefy.Boundaries
{
    /// <summary>
    /// Manages all the interactions related to the search bar and its dropdown results list.
    /// </summary>
    /// <remarks>
    /// Author: Eben, Xavier
    /// Version: 1.0
    /// </remarks>
    public class SearchView : MonoBehaviour
    {
        [Header("Controllers")]
        [Tooltip("Reference to the SearchController that handles API calls.")]
        public SearchController searchController;

        [Header("Map View")]
        [Tooltip("Reference to the MapView to allow recentering on a search result.")]
        public MapView mapView;

        [Header("UI References")]
        [Tooltip("The ScrollRect content object that will contain the search result prefabs.")]
        public RectTransform searchResultContainer;    //parent under scrollview viewport
        [Tooltip("The entire search results list panel (to be shown/hidden).")]
        public GameObject searchResultsView;        //the entire search results list box
        [Tooltip("The prefab for a single search result item.")]
        public GameObject searchResultPrefab;       //prefab with TMP_Text fields
        [Tooltip("A text object shown when a search yields no results.")]
        public GameObject emptyStateText;           //“No saved alerts” 
        [Tooltip("The main input field for the search query.")]
        public TMP_InputField searchInputField;

        /// <summary>
        /// Initializes the search view by adding a listener to the search input field.
        /// </summary>
        public void Start()
        {
            Debug.Log("[SearchView] Initializing Search View");
            // Trigger OnSearchSubmit when the user finishes editing (presses Enter or clicks away)
            searchInputField.onEndEdit.AddListener(OnSearchSubmit);
        }

        /// <summary>
        /// Called when a user clicks a specific search result from the list.
        /// Receneters the map to that result's location and hides the results list.
        /// </summary>
        /// <param name="result">The MapSearchResult data object that was clicked.</param>
        public void OnSearchResultClicked(MapSearchResult result)
        {
            mapView.RecenterAndZoom(result.coord.lon, result.coord.lat);
            searchResultsView.gameObject.SetActive(false);
        }

        /// <summary>
        /// Public entry point to start the search coroutine when the user submits a query.
        /// </summary>
        /// <param name="query">The text from the search input field.</param>
        public void OnSearchSubmit(string query)
        {
            Debug.Log($"[SearchView] Search submitted: {query}");
            StartCoroutine(OnSearchSubmitCoroutine(query));
        }

        /// <summary>
        /// Coroutine that handles the full search process:
        /// 1. Clears old results.
        /// 2. Calls the SearchController to fetch new results.
        /// 3. Displays the new results or an empty state message.
        /// </summary>
        /// <param name="query">The search query.</param>
        /// <returns>An IEnumerator for coroutine execution.</returns>
        public IEnumerator OnSearchSubmitCoroutine(string query)
        {
            query = query.Trim().ToLower();
            Debug.Log($"[SearchView] Processed query: {query}");
            searchResultsView.gameObject.SetActive(false);

            DestroyAllSearchResults();
            yield return searchController.HandleSearch(query);

            foreach (MapSearchResult item in searchController.searchResultList)
            {
                Debug.Log($"[SearchView] Displaying search result: {item.name} - {item.detail}");
                DisplaySearchSuggestion(item);
            }

            // Show the results list only if we found something
            if (searchController.searchResultList.Count > 0)
                searchResultsView.gameObject.SetActive(true);

            // TODO: Show emptyStateText if searchResultList.Count == 0
        }

        /// <summary>
        /// Reveals the search input bar to the user (legacy or future use).
        /// </summary>
        /// <param name="searchBar">The input field to show.</param>
        public void DisplaySearchBar(TMP_InputField searchBar)
        {
            searchBar.gameObject.SetActive(true);
        }

        /// <summary>
        /// Instantiates and populates a single search result prefab in the results list.
        /// </summary>
        /// <param name="searchItem">The MapSearchResult data to display.</param>
        public void DisplaySearchSuggestion(MapSearchResult searchItem)
        {
            GameObject searchResult = Instantiate(searchResultPrefab, searchResultContainer);
            Debug.Log($"Results: {searchItem}");

            // Find text components by name
            TMP_Text header = searchResult.transform.Find("Location Name").GetComponent<TMP_Text>();
            TMP_Text address = searchResult.transform.Find("Location Detail").GetComponent<TMP_Text>();
            Button button = searchResult.GetComponent<Button>();

            // Add listener and populate text
            button.onClick.AddListener(() => OnSearchResultClicked(searchItem));
            header.text = searchItem.name.ToString();
            address.text = searchItem.detail.ToString();
            searchResult.SetActive(true);
        }

        /// <summary>
        /// Clears all displayed search results from the container.
        /// </summary>
        public void DestroyAllSearchResults()
        {
            foreach (Transform child in searchResultContainer)
            {
                // Do not destroy the prefab template itself if it's a child
                if (child.gameObject != searchResultPrefab)
                    Destroy(child.gameObject);
            }
        }
    }
}