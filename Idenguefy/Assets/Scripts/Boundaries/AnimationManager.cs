using Idenguefy.Controllers;
using UnityEngine;

namespace Idenguefy.Boundaries
{
    /// <summary>
    /// AnimationManager is where all animation related logic is kept in:
    /// <list type="number"> 
    /// <item>Holds all the animation methods that are used within the unity environment</item>
    /// <item>Holds all animators created and used as well as the properties required in each animation</item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Author: Xavier
    /// Version: 1.0
    /// Notes: This is a singleton that persists across scenes.
    /// </remarks>
    public class AnimationManager : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Animator for the main Floating Action Button (FAB) menu.")]
        public Animator fabAnimator;
        /// <summary>Internal state tracking if the FAB menu is expanded.</summary>
        private bool fabIsExpanded = false;

        [Tooltip("Animator for the cluster information panel.")]
        public Animator clusterInfoAnimator;
        /// <summary>Internal state tracking if the cluster info panel is expanded.</summary>
        public bool clusterInfoIsExpanded = false;

        [Tooltip("Animator for the pointer information panel.")]
        public Animator pointerInfoAnimator;
        /// <summary>Internal state tracking if the pointer info panel is expanded.</summary>
        private bool pointerInfoIsExpanded = false;

        [Tooltip("Animator for the indoor health tips panel.")]
        public Animator indoorHealthTipsAnimator;
        /// <summary>Internal state tracking if the indoor health tips panel is shown.</summary>
        private bool indoorHealthTipsIsExpanded = false;

        [Tooltip("Animator for the outdoor health tips panel.")]
        public Animator outdoorHealthTipsAnimator;
        /// <summary>Internal state tracking if the outdoor health tips panel is shown.</summary>
        private bool outdoorHealthTipsIsExpanded = false;

        [Tooltip("Animator for the panel that lets users choose between indoor/outdoor tips.")]
        public Animator showWhichHealthTipsAnimator;
        /// <summary>Internal state tracking if the health tip choice panel is shown.</summary>
        private bool showWhichHealthTipsIsExpanded = false;

        [Tooltip("Animator for the dengue cluster list panel.")]
        public Animator clusterListAnimator;
        /// <summary>Internal state tracking if the cluster list panel is expanded.</summary>
        private bool clusterListIsExpanded = false;

        [Tooltip("Animator for the custom pointer list panel.")]
        public Animator pointerListAnimator;
        /// <summary>Internal state tracking if the pointer list panel is expanded.</summary>
        private bool pointerListIsExpanded = false;

        [Tooltip("Animator for the slide-in notification panel.")]
        public Animator notificationAnimator;
        /// <summary>Internal state tracking if the notification panel is expanded.</summary>
        private bool notificationIsExpanded = false;

        [Tooltip("Animator for the settings menu panel.")]
        public Animator settingsAnimator;
        /// <summary>Internal state tracking if the settings panel is expanded.</summary>
        private bool settingsIsExpanded = false;

        [Tooltip("Animator for the in-app alert pop-up window.")]
        public Animator inAppAlertAnimator;
        /// <summary>Internal state tracking if the in-app alert window is shown.</summary>
        public bool inAppAlertIsExpanded = false;

        [Tooltip("Animator for the dual pointer comparison view.")]
        public Animator dualPointerAnimator;
        /// <summary>Internal state tracking if the dual pointer view is expanded.</summary>
        private bool dualPointerIsExpanded = false;

        /// <summary>
        /// Static singleton instance of the AnimationManager.
        /// </summary>
        public static AnimationManager Instance { get; private set; }

        /// <summary>
        /// Initializes the singleton pattern, ensuring only one instance exists
        /// and persists across scene loads.
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.LogWarning("[Animation Manager] Instance created and ready.");
            }
            else Destroy(gameObject);
        }

        /// <summary>
        /// Toggles the Floating Action Button (FAB) menu between its expanded and collapsed states.
        /// </summary>
        public void ToggleFAB()
        {
            fabIsExpanded = !fabIsExpanded;
            fabAnimator.SetBool("isExpanded", fabIsExpanded);
        }

        /// <summary>
        /// Toggles the cluster information panel between its expanded and collapsed states.
        /// </summary>
        public void ToggleClusterInfo()
        {
            clusterInfoIsExpanded = !clusterInfoIsExpanded;
            clusterInfoAnimator.SetBool("isExpanded", clusterInfoIsExpanded);
        }

        /// <summary>
        /// Toggles the pointer information panel between its expanded and collapsed states.
        /// </summary>
        public void TogglePointerInfo()
        {
            pointerInfoIsExpanded = !pointerInfoIsExpanded;
            pointerInfoAnimator.SetBool("isExpanded", pointerInfoIsExpanded);
        }

        /// <summary>
        /// Toggles the indoor health tips panel.
        /// Ensures that the outdoor tips and the selection menu are closed first.
        /// </summary>
        public void ToggleIndoorHealthTips()
        {
            if (outdoorHealthTipsIsExpanded)
            {
                ToggleOutdoorHealthTips();  //If outdoor open, close outdoor
            }

            if (showWhichHealthTipsIsExpanded)
            {
                ToggleShowWhichHealthTips(); //If show which open, close it
            }
            indoorHealthTipsIsExpanded = !indoorHealthTipsIsExpanded;
            indoorHealthTipsAnimator.SetBool("isShown", indoorHealthTipsIsExpanded);
        }

        /// <summary>
        /// Toggles the outdoor health tips panel.
        /// Ensures that the indoor tips and the selection menu are closed first.
        /// </summary>
        public void ToggleOutdoorHealthTips()
        {
            if (indoorHealthTipsIsExpanded)
            {
                ToggleIndoorHealthTips();  //If indoor open, close indoor
            }

            if (showWhichHealthTipsIsExpanded)
            {
                ToggleShowWhichHealthTips(); //If show which open, close it
            }
            outdoorHealthTipsIsExpanded = !outdoorHealthTipsIsExpanded;
            outdoorHealthTipsAnimator.SetBool("isShown", outdoorHealthTipsIsExpanded);
        }

        /// <summary>
        /// Toggles the panel that allows choosing between indoor and outdoor tips.
        /// Ensures that any open tip panels are closed first.
        /// </summary>
        public void ToggleShowWhichHealthTips()
        {
            if (indoorHealthTipsIsExpanded)
            {
                ToggleIndoorHealthTips();  //If indoor open, close indoor
            }

            if (outdoorHealthTipsIsExpanded)
            {
                ToggleOutdoorHealthTips(); //If outdoor open, close outdoor
            }
            showWhichHealthTipsIsExpanded = !showWhichHealthTipsIsExpanded;
            showWhichHealthTipsAnimator.SetBool("isShown", showWhichHealthTipsIsExpanded);
        }

        /// <summary>
        /// Toggles the cluster list panel between its expanded and collapsed states.
        /// </summary>
        public void ToggleClusterList()
        {
            clusterListIsExpanded = !clusterListIsExpanded;
            clusterListAnimator.SetBool("isExpanded", clusterListIsExpanded);
        }

        /// <summary>
        /// Toggles the pointer list panel between its expanded and collapsed states.
        /// </summary>
        public void TogglePointerList()
        {
            pointerListIsExpanded = !pointerListIsExpanded;
            pointerListAnimator.SetBool("isExpanded", pointerListIsExpanded);
        }

        /// <summary>
        /// Toggles the slide-in notification panel between its expanded and collapsed states.
        /// </summary>
        public void ToggleNotifcation()
        {
            notificationIsExpanded = !notificationIsExpanded;
            notificationAnimator.SetBool("isExpanded", notificationIsExpanded);
        }

        /// <summary>
        /// Toggles the settings panel between its expanded and collapsed states.
        /// </summary>
        public void ToggleSettings()
        {
            settingsIsExpanded = !settingsIsExpanded;
            settingsAnimator.SetBool("isExpanded", settingsIsExpanded);
        }

        /// <summary>
        /// Toggles the in-app alert pop-up window between its shown and hidden states.
        /// </summary>
        public void ToggleInAppAlertView()
        {
            inAppAlertIsExpanded = !inAppAlertIsExpanded;
            inAppAlertAnimator.SetBool("isShown", inAppAlertIsExpanded);
        }

        /// <summary>
        /// Toggles the dual pointer comparison panel between its expanded and collapsed states.
        /// </summary>
        public void ToggleDualPointerView()
        {
            dualPointerIsExpanded = !dualPointerIsExpanded;
            dualPointerAnimator.SetBool("isExpanded", dualPointerIsExpanded);
        }

        public void HomeButton()
        {
            //Close all expanded or shown panels
            ResetAnimator(fabAnimator, "isExpanded", ref fabIsExpanded);
            ResetAnimator(clusterInfoAnimator, "isExpanded", ref clusterInfoIsExpanded);
            ResetAnimator(pointerInfoAnimator, "isExpanded", ref pointerInfoIsExpanded);
            ResetAnimator(indoorHealthTipsAnimator, "isShown", ref indoorHealthTipsIsExpanded);
            ResetAnimator(outdoorHealthTipsAnimator, "isShown", ref outdoorHealthTipsIsExpanded);
            ResetAnimator(showWhichHealthTipsAnimator, "isShown", ref showWhichHealthTipsIsExpanded);
            ResetAnimator(clusterListAnimator, "isExpanded", ref clusterListIsExpanded);
            ResetAnimator(pointerListAnimator, "isExpanded", ref pointerListIsExpanded);
            ResetAnimator(notificationAnimator, "isExpanded", ref notificationIsExpanded);
            ResetAnimator(settingsAnimator, "isExpanded", ref settingsIsExpanded);
            ResetAnimator(inAppAlertAnimator, "isShown", ref inAppAlertIsExpanded);
            ResetAnimator(dualPointerAnimator, "isExpanded", ref dualPointerIsExpanded);

            Debug.Log("[Animation Manager] All animations reset to home state");
        }

        //Helper method to reset any animator bool and its corresponding field
        private void ResetAnimator(Animator animator, string boolName, ref bool state)
        {
            if (animator != null && state)
            {
                state = false;
                animator.SetBool(boolName, false);
            }
        }
    }
}