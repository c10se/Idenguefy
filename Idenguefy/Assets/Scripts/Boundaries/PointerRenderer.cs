using Idenguefy.Controllers;
using Idenguefy.Entities;
using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

namespace Idenguefy.Boundaries
{
    /// <summary>
    /// Responsible for rendering the users saved map pointers onto the UI map.
    /// 
    /// Responsibilities:
    /// <list type="number">
    ///     <item><description> Convert pointer coordinates (lon/lat) to pixel positions through MapUtils Class </description></item>
    ///     <item><description> Assigns the correct Pointer Sprite to the pointer based on its home tag</description></item>
    ///     <item><description> Showcases information that the user has written about the pointer in a dedicated UI popup window </description></item>
    ///     <item><description> Includes buttons as means to access additional functionalities like editing/deleting the specified pointer</description></item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Author: Cheuk Hong
    /// Version: 1.0
    /// </remarks>
    public class PointerRenderer : MonoBehaviour
    {
        [Header("Map References")]
        [Tooltip("The main map panel RectTransform where pointers will be parented.")]
        public RectTransform mapPanelRect;    //assign in Inspector
        [Tooltip("The default sprite for a map pointer.")]
        public Sprite pointerSprite;         //assign in Inspector
        [Tooltip("The sprite for a map pointer in dark mode.")]
        public Sprite pointerDarkSprite;       //assign in Inspector
        [Tooltip("The sprite for a 'Home' tagged pointer.")]
        public Sprite pointerHomeSprite;
        [Tooltip("The sprite for a 'Home' tagged pointer in dark mode.")]
        public Sprite pointerDarkHomeSprite;

        [Header("UI references")]
        [Tooltip("The info panel that appears when a pointer is clicked.")]
        public GameObject pointerInfoPanel;  //assign in Inspector
        [Tooltip("The text field inside the info panel to display pointer details.")]
        public TMP_Text pointerInfoText;     //assign in Inspector (inside info panel)
        [Tooltip("The 'Edit' button within the info panel.")]
        public Button editButton;            //assign in Inspector 
        [Tooltip("The 'Delete' button within the info panel.")]
        public Button deleteButton;          //assign in Inspector
        [Tooltip("The 'Close' button within the info panel.")]
        public Button closeInfoButton;       //optional close button?

        /// <summary>
        /// Tracks the currently selected pointer GameObject to manage highlighting.
        /// </summary>
        private GameObject currentSelectedPointer;   //Track the currently selected pointer
        /// <summary>
        /// A list of all instantiated pointer GameObjects currently rendered on the map.
        /// </summary>
        //private List<GameObject> pointersObjList = new List<GameObject>(); // Tracks all instances of Pointers rendered
        private Dictionary<GameObject, MapPointer> pointerObjDict = new(); //True if tagged home, else false.

        /// <summary>Internal reference to the minimum X tile, used for redrawing pointers dynamically.</summary>
        private int refXMin;
        /// <summary>Internal reference to the minimum Y tile, used for redrawing pointers dynamically.</summary>
        private int refYMin;
        /// <summary>Internal reference to the tile size, used for redrawing pointers dynamically.</summary>
        private float refTileSize;

        [Header("Controller Reference")]
        [Tooltip("Reference to the main MapController.")]
        public MapController mapController;
        [Tooltip("Reference to the PointerController for data operations.")]
        public PointerController pointerController;



        [Header("View Reference")]
        [Tooltip("Reference to the PointerView for handling UI panel logic.")]
        public PointerView pointerView;

        /// <summary>
        /// Draws all pointers onto the map UI by iterating through the list
        /// and calling CreatePointerMarker for each.
        /// </summary>
        /// <param name="pointers">The list of MapPointer data objects to render.</param>
        /// <param name="zoom">The current map zoom level.</param>
        /// <param name="xMin">The minimum X tile index of the map.</param>
        /// <param name="yMin">The minimum Y tile index of the map.</param>
        /// <param name="tileSize">The size (in pixels) of a single map tile.</param>
        public void DrawAllPointers(List<MapPointer> pointers, int zoom, int xMin, int yMin, float tileSize)
        {
            refXMin = xMin;
            refYMin = yMin;
            refTileSize = tileSize;
            Debug.Log($"[PointerRenderer] DrawAllPointers() - Method triggered");
            foreach (var pointer in pointers)
            {
                CreatePointerMarker(pointer, zoom, xMin, yMin, tileSize);
            }
        }
        /// <summary>
        /// Creates a single pointer marker GameObject on the map.
        /// Calculates its position, assigns the correct sprite (based on HomeTag and theme),
        /// and adds an OnClick listener.
        /// </summary>
        /// <param name="pointer">The MapPointer data object.</param>
        /// <param name="zoom">The current map zoom level.</param>
        /// <param name="xMin">The minimum X tile index of the map.</param>
        /// <param name="yMin">The minimum Y tile index of the map.</param>
        /// <param name="tileSize">The size (in pixels) of a single map tile.</param>
        private void CreatePointerMarker(MapPointer pointer, int zoom, int xMin, int yMin, float tileSize)
        {
            //Find cluster centre
            Vector2 centre = Vector2.zero;

            float lon = pointer.Longitude;
            float lat = pointer.Latitude;

            // Convert geo-coordinates to pixel coordinates
            MapUtils.LonLatToPixelRatio(lon, lat, zoom, xMin, yMin,
                out double xRatio, out double yRatio);

            float xPixel = (float)(xRatio * tileSize);
            float yPixel = (float)(yRatio * tileSize);

            centre = new Vector2(xPixel, yPixel);

            //Adjust for mapPanel pivot (0.5, 0.5)
            Vector2 offset = new Vector2(mapPanelRect.sizeDelta.x / 2f, -mapPanelRect.sizeDelta.y / 2f);
            Vector2 anchoredCentre = centre - offset;

            //Create pointer UI with a button
            GameObject pointerObj = new GameObject($"Pointer_{pointer.MapID}", typeof(Image), typeof(Button));
            pointerObj.transform.SetParent(mapPanelRect, false);

            // Set the correct sprite based on theme and HomeTag
            Image img = pointerObj.GetComponent<Image>();
            bool darkMode = new SettingsController().GetThemePref();
            if (darkMode)
            {
                if (pointer.HomeTag == true) { img.sprite = pointerDarkHomeSprite; }
                else { img.sprite = pointerDarkSprite; }
            }
            else
            {
                if (pointer.HomeTag == true) { img.sprite = pointerHomeSprite; }
                else { img.sprite = pointerSprite; }
            }
            img.color = new Color(img.color.r, img.color.g, img.color.b, 0.8f);

            // Set size and position
            RectTransform rect = pointerObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 150);
            rect.anchoredPosition = anchoredCentre;

            // Configure the button component
            Button button = pointerObj.GetComponent<Button>();
            button.transition = Selectable.Transition.None; // No visual transition
            //Dynamically create a listner, it's the "OnClick" in the inspector, but cannot see since it's dyanmically created
            button.onClick.AddListener(() => OnPointerClicked(pointer, pointerObj));
            //pointersObjList.Add(pointerObj);
            pointerObjDict.Add(pointerObj, pointer); 
            Debug.Log($"[PointerRenderer] CreatePointerMarker() Pointer: {pointer.Name}, placed at {rect.anchoredPosition}, located at:{pointer.Coordinates} , raw {centre}, offset {offset}");  //Debug to check offset
        }
        /// <summary>
        /// Method for settings to call for recolouring all pointers in the current list of instantiated objects
        /// </summary>
        public void RecolorPointers()
        {
            if (pointerObjDict.Count > 0)
            {
                foreach (KeyValuePair<GameObject, MapPointer> kvp in pointerObjDict)
                {
                    bool darkMode = new SettingsController().GetThemePref();
                    if (darkMode)
                    {
                        if (kvp.Value.HomeTag) { kvp.Key.GetComponent<Image>().sprite = pointerDarkHomeSprite; }
                        else { kvp.Key.GetComponent<Image>().sprite = pointerDarkSprite; }
                    }
                    else
                    {
                        if (kvp.Value.HomeTag) { kvp.Key.GetComponent<Image>().sprite = pointerHomeSprite; }
                        else { kvp.Key.GetComponent<Image>().sprite = pointerSprite; }
                    }
                }
            }
        }

        /// <summary>
        /// Focuses on clearing the pointer on map by finding and deleting the instance of it.
        /// Internally, the entry still exists, and would have to be handled separately.
        /// </summary>
        /// <param name="pointerObj">The pointer GameObject to destroy.</param>
        public void ClearPointer(GameObject pointerObj)
        {
            string pointerName = pointerObj.name;
            GameObject objToRemove = GameObject.Find(pointerName);
            if (objToRemove != null)
            {
                Debug.Log($"Pointer Obj: _{objToRemove.name}_ found. Destroying...");
                Destroy(objToRemove);
            }
        }

        /// <summary>
        /// Making use of ClearPointer(), removes all generated pointer objects and redraws them
        /// by fetching the latest list from the PointerController.
        /// </summary>
        public void RefreshPointers()
        {
            //foreach (var obj in pointersObjList)
            foreach (var obj in pointerObjDict.Keys)
            {
                if (obj != null)
                {
                    ClearPointer(obj);
                }
            }
            //after clearing all pointerObjs on map, clear reference data struct and redraw
            //pointersObjList.Clear();
            pointerObjDict.Clear();
            List<MapPointer> pointersList = pointerController.ListPointers();
            Debug.Log($"[PointerRenderer] RefreshPointers() - Pointer List Count = {pointersList.Count}");
            DrawAllPointers(pointersList, mapController.ZOOM, refXMin, refYMin, refTileSize);
        }

        /// <summary>
        /// Brightens the pointer to showcase responsiveness to user interaction (selection).
        /// </summary>
        /// <param name="pointer">The pointer GameObject to highlight.</param>
        private void HighlightPointer(GameObject pointer)
        {
            var img = pointer.GetComponent<Image>();
            if (img != null)
                img.color = new Color(img.color.r, img.color.g, img.color.b, 1f); //brighten to show selected
            Debug.Log($"[PointerRenderer] HiglightPointer() - Func triggered");
        }

        /// <summary>
        /// Returns the Pointer to its original state (color/alpha) to indicate deselection.
        /// </summary>
        /// <param name="pointer">The pointer GameObject to reset.</param>
        private void ResetPointerColor(GameObject pointer)
        {
            var img = pointer.GetComponent<Image>();
            if (img != null)
            {
                string id = pointer.name.Replace("Pointer_", "");
                //Reset to original color
                img.color = new Color(img.color.r, img.color.g, img.color.b, 0.8f);
                Debug.Log($"[PointerRenderer] ResetPointerColor() Func triggered");
            }
        }

        /// <summary>
        /// Triggered when a pointer is clicked.
        /// Manages selection highlighting and calls ShowPointerInfo to display details.
        /// </summary>
        /// <param name="pointer">The data object for the clicked pointer.</param>
        /// <param name="pointerObj">The GameObject of the clicked pointer.</param>
        public void OnPointerClicked(MapPointer pointer, GameObject pointerObj)
        {
            Debug.Log($"[PointerRenderer] OnPointerClicked() - Object Name: {pointerObj.name}: Pointer clicked:{pointer.Name} at {pointer.AreaName}");

            // Handle selection state and highlighting
            if (currentSelectedPointer != null && currentSelectedPointer != pointerObj)
            {
                ResetPointerColor(currentSelectedPointer);
            }
            currentSelectedPointer = pointerObj;
            //assign current pointer ID to pointerView as well
            pointerView.clickedPointerId = pointer.MapID;
            Debug.Log($"[PointerView] Assigned clickedPointerID = {pointerView.clickedPointerId}");
            HighlightPointer(pointerObj);

            // Display pointer details
            if (pointerInfoPanel != null && pointerInfoText != null)
            {
                ShowPointerInfo(pointer, pointerObj);
            }
            HighlightPointer(pointerObj);
            // show pointer info via the map
        }

        /// <summary>
        /// Shows the pointer info panel and populates it with the pointer's details.
        /// Also dynamically assigns listeners to the Edit and Delete buttons.
        /// </summary>
        /// <param name="pointer">The data object for the selected pointer.</param>
        /// <param name="pointerObj">The GameObject of the selected pointer (not currently used here).</param>
        public void ShowPointerInfo(MapPointer pointer, GameObject pointerObj)
        {
            Debug.Log($"[PointerRenderer] ShowPointerInfo() - Pointer: {pointer.Name} info showing...");
            AnimationManager.Instance.TogglePointerInfo();

            // Add listeners
            if (editButton != null)
            {
                editButton.onClick.AddListener(() => pointerView.ShowEditPanel(pointer));
                Debug.Log($"[PointerRenderer] ShowPointerInfo() - Event Listener for Edit assigned");
            }
            if (deleteButton != null)
            {
                deleteButton.onClick.AddListener(() => pointerView.ShowDeletePanel());
                Debug.Log($"[PointerRenderer] ShowPointerInfo() - Event Listener for Delete assigned");
            }

            // Populate text
            pointerInfoText.text =
                $"<b>Pointer Name: {pointer.Name}</b>\n\n" +
                $"Area Name: {pointer.AreaName}\n" +
                $"Note: {pointer.Note}\n" +
                $"Pointer ID: {pointer.MapID}";
        }

        /// <summary>
        /// Closes the Pointer Info Panel and removes all dynamically added listeners
        /// from the Edit and Delete buttons to prevent multiple calls.
        /// </summary>
        public void HidePointerInfo()
        {
            if (pointerInfoPanel != null)
            {
                Debug.Log($"[HidePointerInfo()] Method triggered.");
                AnimationManager.Instance.TogglePointerInfo();

                // Clear listeners to prevent duplicates
                if (editButton != null)
                {
                    editButton.onClick.RemoveAllListeners();
                    Debug.Log($"[PointerRenderer] HidePointerInfo() - Event Listener for Edit removed");
                }
                if (deleteButton != null)
                {
                    deleteButton.onClick.RemoveAllListeners();
                    Debug.Log($"[PointerRenderer] HidePointerInfo() - Event Listener for Delete removed");
                }
                ResetPointerColor(currentSelectedPointer);
            }
        }

        /// <summary>
        /// Coroutine to load all pointers from the PointerController and
        /// initiate the rendering process.
        /// </summary>
        /// <param name="xMin">The minimum X tile index of the map.</param>
        /// <param name="yMin">The minimum Y tile index of the map.</param>
        /// <returns>An IEnumerator for the coroutine.</returns>
        public System.Collections.IEnumerator LoadAllPointers(int xMin, int yMin)
        {
            Debug.Log("[PointerRenderer] LoadAllPointers() - Fetching pointers...");
            List<MapPointer> pointerList = pointerController.ListPointers();
            Debug.Log($"Total pointers to load: {pointerList.Count}");


            DrawAllPointers(pointerList, mapController.ZOOM, xMin, yMin, mapController.TILE_SIZE);
            yield return null;
        }
    }
}