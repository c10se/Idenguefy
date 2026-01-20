using Idenguefy.Entities;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Idenguefy.Controllers {
    /// <summary>
    /// The abstract base class as interface for handling map location search API requests and responses
    /// 
    /// Responsibilities:
    /// Provides an interface to implement specific searhc API handlers
    /// </summary>
    /// <remarks>
    /// Author: Eben
    /// Version: 1.0
    /// Notes: N/A
    /// </summary>
    /// </remarks>
    public abstract class SearchAPIHandler : MonoBehaviour
    {
        //The API Response
        public static SearchAPIResponse response { get; protected set; }

        //Initialize the empty response object to contain future results
        public void Start()
        {
            response = new SearchAPIResponse();
        }

        /// <summary>
        /// To search for locations based on user query
        /// </summary>
        /// <param name="query">The user search query</param>
        /// <returns></returns>
        public abstract IEnumerator FetchSearchByQuery(string query);
        public class SearchAPIResponse
        {
            public string message; //optional message from the API
            public List<MapSearchResult> results;
        }
    }
}
