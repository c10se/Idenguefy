using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Idenguefy.Entities;

namespace Idenguefy.Controllers
{
    /// <summary>
    /// An implementation of <see cref="Cluster_API_Handler"/> that fetches dengue cluster data
    /// specifically from the Singapore NEA dataset via data.gov.sg.
    /// 
    /// This handler manages the two-step API call process:
    /// 1. Polls the main dataset API to get a temporary download URL.
    /// 2. Downloads the actual GeoJSON dataset from that URL.
    /// 3. Parses the GeoJSON into <see cref="DengueCluster"/> objects.
    /// </summary>
    /// <remarks>
    /// Author: Napatr
    /// Version: 1.0
    /// </remarks>
    public class NEA_Cluster_API_Handler : Cluster_API_Handler
    {
        /// <summary>
        /// The specific dataset ID for the NEA Dengue Clusters on data.gov.sg.
        /// </summary>
        private readonly string datasetId = "d_dbfabf16158d1b0e1c420627c0819168";
        /// <summary>
        /// The base URL for the data.gov.sg public API.
        /// </summary>
        private readonly string baseUrl = "https://api-open.data.gov.sg/v1/public/api/datasets/";

        /// <summary>
        /// Overrides the base method to fetch cluster data from the data.gov.sg API.
        /// Performs a two-step request as required by the API.
        /// </summary>
        /// <returns>An IEnumerator for the coroutine execution.</returns>
        public override IEnumerator FetchClusterData()
        {
            string url = baseUrl + datasetId + "/poll-download";

            // --- Step 1: Poll for the download URL ---
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("[NEA Handler] Error: " + request.error);
                    yield break;
                }

                ApiResponse apiResponse = JsonConvert.DeserializeObject<ApiResponse>(request.downloadHandler.text);

                if (apiResponse.code != 0)
                {
                    Debug.LogError("[NEA Handler] API Error: " + apiResponse.errMsg);
                    yield break;
                }

                // --- Step 2: Download the actual dataset from the retrieved URL ---
                using (UnityWebRequest datasetReq = UnityWebRequest.Get(apiResponse.data.url))
                {
                    yield return datasetReq.SendWebRequest();

                    if (datasetReq.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError("[NEA Handler] Download Error: " + datasetReq.error);
                        yield break;
                    }

                    // On success, preprocess the downloaded JSON
                    string datasetJson = datasetReq.downloadHandler.text;
                    PreprocessData(datasetJson);
                }
            }
        }

        /// <summary>
        /// Parses the raw GeoJSON string from the NEA dataset into the <see cref="Clusters"/> list.
        /// </summary>
        /// <param name="datasetJson">The raw GeoJSON data as a string.</param>
        protected override void PreprocessData(string datasetJson)
        {
            JObject geoData = JObject.Parse(datasetJson);
            JArray features = (JArray)geoData["features"];
            int idCounter = 0;

            Clusters.Clear();

            foreach (var feature in features)
            {
                var props = feature["properties"];
                var geometry = feature["geometry"];
                string geomType = geometry["type"].ToString();

                List<(float lon, float lat)> polygon = new List<(float lon, float lat)>();

                if (geomType == "Polygon")
                {
                    JArray coordinatesArray = (JArray)geometry["coordinates"][0];
                    foreach (JArray coord in coordinatesArray)
                    {
                        float lon = coord[0].ToObject<float>();
                        float lat = coord[1].ToObject<float>();
                        polygon.Add((lon, lat));
                    }
                }
                else if (geomType == "MultiPolygon")
                {
                    // Iterate through multiple polygons
                    foreach (JArray poly in (JArray)geometry["coordinates"])
                    {
                        foreach (JArray ring in poly)
                        {
                            foreach (JArray coord in ring)
                            {
                                float lon = coord[0].ToObject<float>();
                                float lat = coord[1].ToObject<float>();
                                polygon.Add((lon, lat));
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[NEA Handler] Unsupported geometry type: {geomType}");
                    continue;
                }

                DengueCluster cluster = new DengueCluster(
                    locationID: idCounter.ToString(),
                    caseSize: props["CASE_SIZE"].ToObject<int>(),
                    severity: DetermineSeverity(props["CASE_SIZE"].ToObject<int>()),
                    areaName: props["LOCALITY"].ToString(),
                    coordinates: polygon
                );

                Clusters.Add(cluster);
                idCounter++;
            }

            Debug.Log($"[NEA Handler] Parsed {Clusters.Count} clusters successfully.");
        }


        /// <summary>
        /// A serializable class to map the JSON response from the initial API poll.
        /// </summary>
        [System.Serializable]
        public class ApiResponse
        {
            /// <summary>
            /// The API response code (0 indicates success).
            /// </summary>
            public int code;
            /// <summary>
            /// An error message if the API call was unsuccessful.
            /// </summary>
            public string errMsg;
            /// <summary>
            /// The nested data object containing the download URL.
            /// </summary>
            public ApiData data;
        }

        /// <summary>
        /// A nested serializable class representing the "data" object in the <see cref="ApiResponse"/>.
        /// </summary>
        [System.Serializable]
        public class ApiData
        {
            /// <summary>
            /// The direct download URL for the dataset file.
            /// </summary>
            public string url;
        }
    }
}