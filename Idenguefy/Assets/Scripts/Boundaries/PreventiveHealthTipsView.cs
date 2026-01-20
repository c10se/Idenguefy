using Idenguefy.Controllers;
using Idenguefy.Entities;
using UnityEngine;
using TMPro;

namespace Idenguefy.Boundaries
{
    /// <summary>
    /// Displays either Indoor or Outdoor Dengue preventative health tips to the user.
    /// </summary>
    /// <remarks>
    /// Author: Sharina, Xavier
    /// Version: 2.0
    /// </remarks>
    public class PreventiveHealthTipsView : MonoBehaviour
    {
        [Header("Containers")]
        [Tooltip("The ScrollRect content object for indoor tips.")]
        public RectTransform indoorTipsContainer;   //assign in Inspector → Indoor ScrollView Content
        [Tooltip("The ScrollRect content object for outdoor tips.")]
        public RectTransform outdoorTipsContainer;  //assign in Inspector → Outdoor ScrollView Content

        [Header("Prefabs")]
        [Tooltip("The prefab for a single tip item (e.g., a small card with TMP_Text).")]
        public GameObject tipItemPrefab;            //assign prefab (e.g., a small card with TMP_Text)

        [Header("Panels")]
        [Tooltip("The parent panel for the indoor tips ScrollRect.")]
        public GameObject indoorPanel;              //assign Indoor panel (ScrollRect parent)
        [Tooltip("The parent panel for the outdoor tips ScrollRect.")]
        public GameObject outdoorPanel;             //assign Outdoor panel (ScrollRect parent)

        /// <summary>
        /// An instance of the TipController to access the lists of tips.
        /// </summary>
        private TipController tipController;

        /// <summary>
        /// Initializes the TipController.
        /// </summary>
        private void Awake()
        {
            tipController = new TipController();
        }

        /// <summary>
        /// Populates both the indoor and outdoor tip containers on start.
        /// </summary>
        private void Start()
        {
            //Auto-load both sets of tips at startup
            PopulateTips(indoorTipsContainer, tipController.IndoorTips);
            PopulateTips(outdoorTipsContainer, tipController.OutdoorTips);
        }

        /// <summary>
        /// Fills a specific container with a list of health tips by instantiating prefabs.
        /// </summary>
        /// <param name="container">The RectTransform parent for the tip objects.</param>
        /// <param name="tips">The list of PreventiveHealthTip data to display.</param>
        private void PopulateTips(RectTransform container, System.Collections.Generic.List<PreventiveHealthTip> tips)
        {
            //Clear any existing children (in case of re-entry)
            foreach (Transform child in container)
                Destroy(child.gameObject);

            // Instantiate tips
            foreach (PreventiveHealthTip tip in tips)
            {
                GameObject tipObj = Instantiate(tipItemPrefab, container);
                TMP_Text textField = tipObj.GetComponentInChildren<TMP_Text>();
                if (textField != null)
                    textField.text = "• " + tip.Context; // Add a bullet point
            }
        }

    }
}