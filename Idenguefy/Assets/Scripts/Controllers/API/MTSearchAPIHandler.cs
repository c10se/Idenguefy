using Idenguefy.Controllers;
using Idenguefy.Entities;
using Newtonsoft.Json.Linq;

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Idenguefy.Controllers
{
    /// <summary>
    /// The SearchAPIHandler implementation for MapTiler's Geocoding API
    /// 
    /// Responsibilities:
    /// <list type="number">
    ///     <item><description> Request relevant GeocodingAPI location data based on the user's search query </description></item>
    ///     <item><description> Download the raw Geocoding dataset </description></item>
    ///     <item><description> Parse and convert features into MapSearchResult entities </description></item>
    ///     <item><description> Add preprocessed search results into an array </description></item>
    ///     <item><description> Wrap array with appropriate SearchAPIResponse Wrapper </description></item>
    /// </summary>
    /// <remarks>
    /// Author: Eben
    /// Version: 1.0
    /// Notes: N/A
    /// </summary>
    /// </remarks>
    public class MTSearchAPIHandler : SearchAPIHandler
    {

        private static readonly string base_url = "https://api.maptiler.com/geocoding/";
        private string key;
        private static readonly string country = "sg";
        private static readonly int limit = 5;
        void Awake()
        {
            EnvLoader.Load();
            key = EnvLoader.Get("MTSearchAPIHandler_API_Key");
        }

        //Avoid creating a new searchResultList on the APIHandler side since SearchController already has one

        /// <summary>
        /// Run the entire search flow: fetch raw data from API, preprocess into MapSearchResult list, assign to response
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public override IEnumerator FetchSearchByQuery(string query)
        {
            List<MapSearchResult> searchResultList = new List<MapSearchResult>();
            Debug.Log($"[MapTilerSearchAPIHandler] SearchByQuery called with query: {query}");
            string url = getSearchURL(query);
            Debug.Log($"[MapTilerSearchAPIHandler] Constructed URL: {url}");

            string fetchedJson = null;
            yield return StartCoroutine(FetchSearchByURL(url, s => fetchedJson = s));
            Debug.Log($"[MapTilerSearchAPIHandler] Fetched JSON: {fetchedJson}");

            if(string.IsNullOrEmpty(fetchedJson))
            {
                Debug.Log($"[MapTilerSearchAPIHandler] No data fetched from API.");
                response.message = "No data fetched from API.";
                response.results = searchResultList; //empty list
                yield break;
            }


            Debug.Log($"[MapTilerSearchAPIHandler] Preprocessing search results...");
            searchResultList = PreprocessSearchData(fetchedJson);
            Debug.Log($"[MapTilerSearchAPIHandler] Preprocessing completed. Found {searchResultList.Count} results.");

            response.message = "Success";
            response.results = searchResultList;
        }

        /// <summary>
        /// To fetch raw Geocoding JSON data from MapTilerAPI given the constructed URL
        /// </summary>
        /// <param name="url">The base URL of API</param>
        /// <param name="onComplete">The action to pass in the onComplete method</param>
        /// <returns></returns>
        public IEnumerator FetchSearchByURL(string url, Action<string> onComplete)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[MapTilerSearchAPIHandler] Search API error: {request.error}");
                    yield break;
                }
                string json = request.downloadHandler.text;
                onComplete?.Invoke(json);
            }
        }

        /// <summary>
        /// Parse raw JSON into list of MapSearchResult objects
        /// </summary>
        /// <param name="json">The json string to be parsed</param>
        /// <returns></returns>
        private List<MapSearchResult> PreprocessSearchData(string json)
        {
            List<MapSearchResult> searchResultList = new List<MapSearchResult>();
            JObject geoData = JObject.Parse(json);
            JArray features = (JArray)geoData["features"];
            foreach (var feature in features)
            {
                JObject geo = (JObject)feature["geometry"];
                JArray coord = (JArray)geo["coordinates"];

                float relevance = feature["relevance"].ToObject<float>();

                MapSearchResult result = new MapSearchResult
                (
                    name: feature["text"].ToString(),
                    detail: feature["place_name"].ToString(),
                    relevance: feature["relevance"].ToObject<float>(),
                    coord: (
                        coord[0].ToObject<float>(),
                        coord[1].ToObject<float>()
                    )

                );
                searchResultList.Add(result);
                Debug.Log($"[MTSearchAPIHandler] name={result.name}, coord=({result.coord.lon}, {result.coord.lat})");
            }
            return searchResultList;
        }

        /// <summary>
        /// Construct
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private string getSearchURL(string query)
        {
            return $"{base_url}{query}.json?key={key}&country={country}&limit={limit}";
        }
    }
}
