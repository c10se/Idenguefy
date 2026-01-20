using Idenguefy.Controllers;
using Idenguefy.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;

namespace Idenguefy.Boundaries
{
    /// <summary>
    /// Responsible for rendering the saved map pointers additional UI interfaces.
    /// 
    /// Responsibilities:
    /// <list type="number">
    ///     <item><description>Displays respective popup windows for Creating/Editing/Deleting a pointer</description></item>
    ///     <item><description>Displays confirmation for deleting a pointer</description></item>
    ///     <item><description>Displays a list of all pointers created by the user</description></item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Author: Cheuk Hong
    /// Version: 1.0
    /// </remarks>
    public class PointerView : MonoBehaviour
    {
        [Header("Controllers")]
        [Tooltip("Reference to the PointerController for data operations.")]
        public PointerController pointerController;
        [Tooltip("Reference to the MapController for coordinate conversion.")]
        public MapController mapController;
        [Tooltip("Reference to the PointerRenderer to refresh map markers after changes.")]
        public PointerRenderer pointerRenderer; 
        [Header("Create/Edit UI")]
        [Tooltip("The main panel for both creating and editing a pointer.")]
        public GameObject dualPurposePanel;
        [Tooltip("The title text for the dual-purpose panel (e.g., 'Create Pointer' or 'Edit Pointer').")]
        public TMP_Text dualPurposeTitle;
        [Tooltip("Input field for the pointer's name.")]
        public TMP_InputField dualNameInput;
        [Tooltip("Input field for the pointer's area/location name.")]
        public TMP_InputField dualAreaInput;
        [Tooltip("Input field for user notes about the pointer.")]
        public TMP_InputField dualNoteInput;
        [Tooltip("The toggle component to mark a pointer as 'Home'.")]
        public Toggle homeToggles;

        /// <summary>
        /// Stores the MapID of the pointer currently being interacted with (e.g., clicked, edited, or deleted).
        /// </summary>
        public string clickedPointerId;

        [Header("Delete UI")]
        [Tooltip("The confirmation panel that appears when deleting a pointer.")]
        public GameObject deletePanel;

        [Header("Other UI References")]
        [Tooltip("The GameObject containing the list of all pointers.")]
        public GameObject pointerList;
        [Tooltip("The ScrollRect content object that will contain the pointer entry prefabs.")]
        public RectTransform pointerEntryContainer;     //parent under a ScrollView
        [Tooltip("The prefab for a single pointer list item.")]
        public GameObject pointerEntryPrefab;      //prefab with TMP_Text fields and button
        [Tooltip("A text object shown when the pointer list is empty.")]
        public GameObject emptyStateText;       //“No saved pointers” text
        [Tooltip("Sprite for a standard pointer in the list.")]
        public Sprite pointerSprite;
        [Tooltip("Sprite for a 'Home' pointer in the list.")]
        public Sprite pointerHomeSprite;

        /// <summary>
        /// Internal list of MapPointer data, (unused, data is fetched on demand).
        /// </summary>
        private List<MapPointer> pointers;
        /// <summary>
        /// A list of all instantiated pointer entry GameObjects in the scrollable list.
        /// </summary>
        private List<GameObject> pointerEntries = new();
        /// <summary>
        /// Flag to determine if the dual-purpose panel is in 'Edit' mode (true) or 'Create' mode (false).
        /// </summary>
        private bool isEditMode = false;
        /// <summary>
        /// The duration (in seconds) a user must press and hold (unused, logic is in MapView).
        /// </summary>
        private double longPressTime = 1;
        /// <summary>
        /// The screen coordinate from a long press, used for creating a new pointer.
        /// </summary>
        private Vector2 touchCoord;
        /// <summary>
        /// The geographic coordinate (lon, lat) from a long press (unused).
        /// </summary>
        private (float lon, float lat) geoCoord;

        /// <summary>
        /// Runs when the application detects a user is pressing and holding on the screen.
        /// This is called by MapView.ReadTouch().
        /// </summary>
        /// <param name="touchCoord">The screen position (in pixels) of the long press.</param>
        public void OnLongPressDetected(Vector2 touchCoord)
        {
            this.touchCoord = touchCoord;
            ShowCreatePanel();
        }

        /// <summary>
        /// Brings up the Create/Edit interface, configured for 'Create' mode.
        /// </summary>
        public void ShowCreatePanel() // part of dual panel logic
        {
            //Debug.Log($"[PointerView] ShowCreatePanel() Func Triggered");
            isEditMode = false;
            dualPurposeTitle.text = "Create Pointer";
            //dualPurposePanel.SetActive(true);
            AnimationManager.Instance.ToggleDualPointerView(); // brings in create/edit ui
        }

        /// <summary>
        /// Carries out Pointer Creation logic based on the data in the input fields.
        /// </summary>
        public void ConfirmCreate()
        {
            isEditMode = false;
            string MapId = null; // Let the controller generate a new ID
            string PointerName = dualNameInput.text.Trim();
            string AreaName = dualAreaInput.text.Trim();
            string Note = dualNoteInput.text.Trim();
            bool hometagBool = homeToggles.isOn;
            try
            {
                // Create the pointer using the screen coordinate
                pointerController.CreatePointer(MapId, PointerName, hometagBool, AreaName, Note, touchCoord);
                Debug.Log($"[PointerView] ConfirmCreate() - Created pointer {PointerName} at {AreaName}. Coord: lon={touchCoord.x}, lat={touchCoord.y}");
                HideDualPanel();
            }
            catch (Exception)
            {
                Debug.Log("Yea buddy... Whatever that was in there, it didn't work.");
                throw;
            }
        }

        /// <summary>
        /// Brings up the Create/Edit interface, configured for 'Edit' mode
        /// and pre-filled with the pointer's existing data.
        /// </summary>
        /// <param name="pointer">The MapPointer data object to edit.</param>
        public void ShowEditPanel(MapPointer pointer) // part of dual panel logic, include arg for MapPointer?
        {
            //dualPurposePanel.SetActive(true);
            AnimationManager.Instance.ToggleDualPointerView(); // brings in create/edit ui
            AnimationManager.Instance.TogglePointerInfo(); // hides the pointer info window

            isEditMode = true; // Should trigger when user selects edit button
            Debug.Log($"[PointerView] ShowEditPanel() - Showing a {isEditMode} Edit Interface for Pointer: {pointer.Name}");

            // Populate fields with existing data
            dualPurposeTitle.text = "Edit Pointer";
            dualNameInput.text = pointer.Name;
            dualAreaInput.text = pointer.AreaName;
            dualNoteInput.text = pointer.Note;
            homeToggles.isOn = pointer.HomeTag;
        }

        /// <summary>
        /// Carries out edit operations when user confirms the edit,
        /// using the data from the input fields.
        /// </summary>
        public void ConfirmEdit()
        {
            string PointerName = dualNameInput.text.Trim();
            string AreaName = dualAreaInput.text.Trim();
            string Note = dualNoteInput.text.Trim();
            bool homeTag = homeToggles.isOn;

            // Edit the pointer using the stored clickedPointerId
            bool edited = pointerController.EditPointer(clickedPointerId, newName: PointerName, newHomeTag: homeTag, newAreaName: AreaName, newNote: Note);
            HideDualPanel();
            Debug.Log($"[PointerView] ConfirmEdit() - Editing: {PointerName}, MapID: {clickedPointerId}, now located in {AreaName}.");
            Debug.Log($"[PointerView] ConfirmEdit() - Edit status:{edited}");
        }

        /// <summary>
        /// Hides the create/edit panel and resets all input fields and listeners.
        /// </summary>
        public void HideDualPanel() // include logic to clear text, jic
        {
            isEditMode = false;
            //dualPurposePanel.SetActive(false);
            AnimationManager.Instance.ToggleDualPointerView(); // closes create/edit ui

            // Clean up listeners from the info panel (which is now hidden)
            if (pointerRenderer.editButton != null)
                pointerRenderer.editButton.onClick.RemoveAllListeners();

            // Clear input fields
            dualNameInput.text = null;
            dualAreaInput.text = null;
            dualNoteInput.text = null;
            homeToggles.isOn = false;

            Debug.Log($"[PointerView] HideDualPanel() Func Triggered");
        }

        /// <summary>
        /// Utilises the corresponding Create or Edit confirmation logic
        /// based on the isEditMode flag, then refreshes the pointers on the map.
        /// </summary>
        public void ConfirmCreateOrEdit()
        {
            if (isEditMode) { ConfirmEdit(); }
            else { ConfirmCreate(); }
            pointerRenderer.RefreshPointers(); // Update map markers
        }

        /// <summary>
        /// Shows the delete confirmation screen.
        /// </summary>
        public void ShowDeletePanel()
        {
            // shows when user clicks the delete button, a simple UI for confirmation.
            deletePanel.SetActive(true);
            // AnimationManager.Instance.TogglePointerDeleteUI(); //Shows Delete UI
        }

        /// <summary>
        /// Hides the delete confirmation screen.
        /// </summary>
        public void HideDeletePanel()
        {
            deletePanel.SetActive(false);
            // AnimationManager.Instance.TogglePointerDeleteUI(); //Closes Delete UI
        }

        /// <summary>
        /// If user selects the confirm button, proceed with pointer deletion
        /// and refresh the pointers on the map UI if successful.
        /// </summary>
        public void ConfirmDelete() // 
        {
            Debug.Log($"[PointerView] ConfirmDelete() Triggered, on pointer ID: {clickedPointerId}");
            bool deleted = pointerController.DeletePointer(clickedPointerId);
            if (deleted)
            {
                pointerRenderer.RefreshPointers(); // Update map markers
            }
        }

        /// <summary>
        /// Reloads the list of all pointer entries onto the corresponding UI window.
        /// </summary>
        public void RefreshPointerList()
        {
            if (pointerEntries == null)
                return;

            // Clear old entries
            foreach (var obj in pointerEntries)
                Destroy(obj);
            pointerEntries.Clear();

            if (pointerController == null)
            {
                Debug.LogWarning("[PointerView] RefreshPointerList() - PointerController not assigned!");
                return;
            }

            List<MapPointer> pointers = pointerController.ListPointers();

            // Handle empty state
            if (pointers == null || pointers.Count == 0)
            {
                if (emptyStateText != null)
                {
                    emptyStateText.SetActive(true);
                }
                Debug.Log("[PointerView] RefreshPointerList() - No Pointers to display!");
                return;
            }

            if (emptyStateText != null)
            {
                emptyStateText.SetActive(false);
            }

            // Populate list with new entries
            foreach (MapPointer pointer in pointers)
            {
                GameObject entry = Instantiate(pointerEntryPrefab, pointerEntryContainer);

                // Set the icon
                Image[] pointerImages = entry.gameObject.GetComponentsInChildren<Image>();
                if (pointer.HomeTag)
                {
                    pointerImages[1].sprite = pointerHomeSprite;
                    Debug.Log($"Home Pointer Icon loaded");
                }
                else
                {
                    pointerImages[1].sprite = pointerSprite;
                    Debug.Log($"Standard Pointer Icon loaded");
                }
                pointerImages[1].SetNativeSize();

                // Populate text fields
                TMP_Text[] texts = entry.GetComponentsInChildren<TMP_Text>();
                foreach (TMP_Text txt in texts)
                {
                    string lower = txt.name.ToLower();
                    if (lower.Contains("name"))
                    {
                        txt.text = $"<b>{pointer.Name}</b>\n";
                    }
                    if (lower.Contains("details"))
                    {
                        txt.text =
                            $"Area Name: {pointer.AreaName}\n" +
                            $"Note: {pointer.Note}\n";
                    }
                }
                pointerEntries.Add(entry);
            }
            Debug.Log($"[PointerView] Displayed {pointers.Count} pointers.");
        }

        /// <summary>
        /// Calls to reload and show the list of all pointer entries onto the corresponding UI window.
        /// </summary>
        public void ShowPointerList()
        {
            RefreshPointerList();
        }

        /// <summary>
        /// Closes the UI window where the list of all pointer entries are on 
        /// </summary>
        public void HidePointerList()
        {
            AnimationManager.Instance.TogglePointerList();
        }
    }
}