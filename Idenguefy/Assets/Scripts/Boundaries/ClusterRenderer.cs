using Idenguefy.Controllers;
using Idenguefy.Entities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Idenguefy.Boundaries
{
    /// <summary>
    /// Responsible for rendering dengue clusters on the UI map as colored circles.
    /// 
    /// Responsibilities:
    /// <list type="number">
    ///     <item><description> Convert cluster coordinates (lon/lat) to pixel positions through MapUtils Class </description></item>
    ///     <item><description> Compute approximate cluster radius from polygon points through MapUtils Class </description></item>
    ///     <item><description> Create semi-transparent colored circle UI elements inside mapPanelRect </description></item>
    ///     <item><description> Color code clusters by severity </description></item>
    ///     <item><description> Shows information about the cluster in a dedicated UI popup window </description></item>
    /// </list>
    /// Adjusts for map panel offset and pivot alignment to ensure accurate placement
    /// Creates a color coded cluster based on serverity
    /// </summary>
    /// <remarks>
    /// Author: Xavier
    /// Version: 2.0
    /// Notes: N/A
    /// </remarks>
    public class ClusterRenderer : MonoBehaviour
    {
        [Header("Map References")]
        public RectTransform mapPanelRect;   //assign in Inspector
        public Sprite circleSprite;          //assign in Inspector (circle.png)

        [Header("UI references")]
        public GameObject clusterInfoPanel;  //assign in Inspector
        public TMP_Text clusterInfoText;     //assign in Inspector (inside info panel)
        public Button closeInfoButton;       //optional close button

        private GameObject currentSelectedCircle;   //Track the currently selected cluster

        [Header("Controller Reference")]
        public MapController mapController;
        public ClusterController clusterController;

        /// <summary>
        /// Draws circles for all dengue clusters
        /// </summary>
        public void DrawClusterCircles(List<DengueCluster> clusters, int zoom, int xMin, int yMin, float tileSize)
        {
            foreach (var cluster in clusters)
            {
                CreateCircle(cluster, zoom, xMin, yMin, tileSize);
            }
        }
        /// <summary>
        /// Draws the circle for a cluster and sets its details and colors based off retrieved information.
        /// </summary>
        private void CreateCircle(DengueCluster cluster, int zoom, int xMin, int yMin, float tileSize)
        {
            //Find cluster centre
            Vector2 centre = Vector2.zero;

            foreach ((float lon, float lat) in cluster.Coordinates)
            {
                MapUtils.LonLatToPixelRatio(lon, lat, zoom, xMin, yMin,
                    out double xRatio, out double yRatio);

                float xPixel = (float)(xRatio * tileSize);
                float yPixel = (float)(yRatio * tileSize);

                centre += new Vector2(xPixel, yPixel);
            }
            centre /= cluster.Coordinates.Count;

            //Approximate radius
            float radius = 0f;
            foreach ((float lon, float lat) in cluster.Coordinates)
            {
                MapUtils.LonLatToPixelRatio(lon, lat, zoom, xMin, yMin,
                    out double xRatio, out double yRatio);

                float xPixel = (float)(xRatio * tileSize);
                float yPixel = (float)(yRatio * tileSize);

                float dist = Vector2.Distance(centre, new Vector2(xPixel, yPixel));
                if (dist > radius) radius = dist;
            }

            //Adjust for mapPanel pivot
            // If pivot is center (0.5, 0.5) [WHICH IT IS, SO DO NOT CHANGE FROM 0.5, 0.5], move cluster coords so they align
            Vector2 offset = new Vector2(mapPanelRect.sizeDelta.x / 2f, -mapPanelRect.sizeDelta.y / 2f);
            Vector2 anchoredCentre = centre - offset;

            //Create circle UI with a button
            GameObject circleObj = new GameObject($"ClusterCircle_{cluster.LocationID}", typeof(Image), typeof(Image), typeof(Button));
            circleObj.transform.SetParent(mapPanelRect, false);

            Image img = circleObj.GetComponent<Image>();
            img.sprite = circleSprite;
            img.color = GetSeverityColor(cluster.Severity);

            RectTransform rect = circleObj.GetComponent<RectTransform>();
            rect.anchoredPosition = anchoredCentre;
            rect.sizeDelta = new Vector2(radius * 2, radius * 2);


            Button button = circleObj.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            //Dynamically create a listner, it's the "OnClick" in the inspector, but cannot see since it's dyanmically created
            button.onClick.AddListener(() => OnClusterClicked(cluster, circleObj));

            Debug.Log($"Cluster {cluster.AreaName} placed at {rect.anchoredPosition}, raw {centre}, offset {offset}");  //Debug to check offset
        }

        /// <summary>
        /// Triggered when a cluster circle is clicked
        /// Shows its details in the info panel
        /// </summary>
        private void OnClusterClicked(DengueCluster cluster, GameObject circleObj)
        {
            Debug.Log($"[ClusterCircleDrawer] Cluster clicked: {cluster.AreaName}");

            // Highlight selected cluster
            if (currentSelectedCircle != null && currentSelectedCircle != circleObj)
            {
                ResetCircleColor(currentSelectedCircle);
            }
            currentSelectedCircle = circleObj;
            HighlightCircle(circleObj);

            // Display cluster details
            if (clusterInfoPanel != null && clusterInfoText != null)
            {
                if (!AnimationManager.Instance.clusterInfoIsExpanded)
                {
                    AnimationManager.Instance.ToggleClusterInfo();
                }
                //clusterInfoPanel.SetActive(true);
                clusterInfoText.text =
                    $"<b>Name: {cluster.AreaName}</b>\n\n" +
                    $"Cases: {cluster.CaseSize}\n" +
                    $"Severity: {cluster.Severity}\n" +
                    $"Location ID: {cluster.LocationID}";

                //StartCoroutine(FadeInInfoPanel());
            }
        }
        /// <summary>
        /// Brightens the Cluster circle to showcase responsiveness to user interaction
        /// </summary>
        private void HighlightCircle(GameObject circle)
        {
            var img = circle.GetComponent<Image>();
            if (img != null)
                img.color = new Color(img.color.r, img.color.g, img.color.b, 0.8f); //brighten to show selected
        }
        /// <summary>
        /// Returns the Cluster circle to its original state to indicate deselection to the user
        /// </summary>
        private void ResetCircleColor(GameObject circle)
        {
            var img = circle.GetComponent<Image>();
            if (img != null)
            {
                string id = circle.name.Replace("ClusterCircle_", "");
                //Reset to original color
                img.color = new Color(img.color.r, img.color.g, img.color.b, 0.3f);
            }
        }

        //Animation for the panel, just testing things
        private IEnumerator FadeInInfoPanel()
        {
            CanvasGroup cg = clusterInfoPanel.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = clusterInfoPanel.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
            }

            float duration = 0.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Lerp(0, 1, elapsed / duration);
                yield return null;
            }
            cg.alpha = 1;
        }

        /// <summary>
        /// Hides the info panel (for Close button)
        /// </summary>
        public void HideClusterInfo()
        {
            if (clusterInfoPanel != null)
            {
                //clusterInfoPanel.SetActive(false);
                ResetCircleColor(currentSelectedCircle);
            }
        }
        /// <summary>
        /// Determines colour to set the Cluster Circle based on given severity
        /// </summary>
        private Color GetSeverityColor(string severity)
        {
            switch (severity)
            {
                case "High": return new Color(1f, 0f, 0f, 0.3f);   //Red
                case "Medium": return new Color(1f, 0.5f, 0f, 0.3f); //Orange
                default: return new Color(0f, 1f, 0f, 0.3f);      //Green
            }
        }
        /// <summary>
        /// Retrieves a list of all clusters from the API to be handled by other methods in ClusterRenderer.
        /// </summary>
        public IEnumerator LoadAllClusters(int xMin, int yMin)
        {
            Debug.Log("Fetching preprocessed cluster data...");
            List<DengueCluster> clusterList = clusterController.Clusters;
            Debug.Log($"Total clusters to load: {clusterList.Count}");

            DrawClusterCircles(clusterList, mapController.ZOOM, xMin, yMin, mapController.TILE_SIZE);
            yield return null;
        }
    }
}
