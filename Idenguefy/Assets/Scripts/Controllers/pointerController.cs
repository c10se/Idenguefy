using Idenguefy.Boundaries;
using Idenguefy.Entities;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
//using UnityEngine.UIElements;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace Idenguefy.Controllers
{
    /// <summary>
    /// Manages user-created map pointers (e.g., saved or tagged locations).
    /// 
    /// Provides full CRUD functionality:
    /// <list type="number">
    ///     <item><description> CreatePointer: Add new pointer </description></item>
    ///     <item><description> ReadPointer: Retrieve pointer by ID </description></item>
    ///     <item><description> EditPointer: Modify details of an existing pointer </description></item>
    ///     <item><description> DeletePointer: Remove pointer from the list </description></item>
    /// </summary>
    /// <remarks>
    /// Author: Cheuk Hong, Sharina, Napatr, Xavier
    /// Version: 1.0
    /// Notes: N/A
    /// </remarks>
    public class PointerController : MonoBehaviour
    {

        //[Header("Map References")]
        //public RectTransform mapPanelRect;   //assign in Inspector
        //public Sprite pointerSprite;          //assign in Inspector (pointer.png)

        //[Header("UI references")]
        //public GameObject pointerInfoPanel;  //assign in Inspector
        //public TMP_Text pointerInfoText;     //assign in Inspector (inside info panel)
        //public Button closePointerInfoButton;      //close button

        [Header("Controller References")]
        public MapController mapController;

        // Store all pointers in memory
        private List<MapPointer> pointers;
        private DataManager<MapPointer> dataManager;

        private void Awake()
        {
            dataManager = new DataManager<MapPointer>("Idenguefy_Pointers");
            pointers = dataManager.LoadData();
        }


        // ------------------------
        // CRUD Methods
        // ------------------------

        /// <summary>
        /// List all map pointers.
        /// </summary>
        public List<MapPointer> ListPointers()
        {
            pointers = dataManager.LoadData(); // attempting to resolve inconsistency when deleting pointers
            return new List<MapPointer>(pointers); // return a copy
        }
        
        /// <summary>
        /// Create a new map pointer and add it to the list.
        /// </summary>
        public void CreatePointer(string mapID, string name, bool homeTag, string areaName, string note, Vector2 touchcoordinates)
        {
            pointers = dataManager.LoadData();
            mapID = pointers.Count.ToString();
            (float, float) coordinates = mapController.ScreenToGeo(touchcoordinates);
            MapPointer newPointer = new MapPointer(mapID, name, homeTag, areaName, note, coordinates);
            pointers.Add(newPointer);
            Debug.Log($"Pointer ID: {newPointer.MapID}");
            Debug.Log($"[PointerController] Created pointer {name} at lon={coordinates.Item1}, lat={coordinates.Item2}");
            Debug.Log($"Pointers: {pointers}");

            dataManager.SaveData(pointers);
        }

        /// <summary>
        /// Read (find) a pointer by its ID.
        /// </summary>
        public MapPointer ReadPointer(string mapID)
        {
            MapPointer pointer = pointers.FirstOrDefault(p => p.MapID == mapID);
            Debug.LogWarning($"[MapPointerController] ReadPointer() - Pointer with ID {pointer.MapID} found.");
            return pointer;
        }

        /// <summary>
        /// Edit a pointer’s details by ID.
        /// </summary>
        public bool EditPointer(string mapID, string newName = null, bool? newHomeTag = null, string newAreaName = null, string newNote = null, (float lon, float lat)? newCoordinates = null)
        {
            pointers = dataManager.LoadData();
            MapPointer pointer = ReadPointer(mapID);
            if (pointer == null)
            {
                Debug.LogWarning($"[MapPointerController] Pointer with ID {mapID} not found.");
                return false;
            }

            if (newName != null) pointer.Name = newName;
            if (newHomeTag.HasValue) pointer.HomeTag = newHomeTag.Value;
            if (newAreaName != null) pointer.AreaName = newAreaName;
            if (newNote != null) pointer.Note = newNote;
            if (newCoordinates.HasValue) pointer.Coordinates = newCoordinates.Value;

            Debug.Log($"[MapPointerController] Edited pointer {mapID}.");

            dataManager.SaveData(pointers);
            return true;
        }

        /// <summary>
        /// Delete a pointer by its ID.
        /// </summary>
        public bool DeletePointer(string mapID)
        {
            Debug.LogWarning($"[MapPointerController] DeletePointer() - Deleting Pointer with ID {mapID}.");
            pointers = dataManager.LoadData();
            MapPointer pointer = ReadPointer(mapID);
            if (pointer == null)
            {
                Debug.LogWarning($"[MapPointerController] Pointer with ID {mapID} not found.");
                return false;
            }

            pointers.Remove(pointer);
            for (int i=0; i< pointers.Count; i++)
            {
                pointers[i].MapID = i.ToString();
            }

            Debug.Log($"[MapPointerController] DeletePointer() - Deleted pointer {pointer.Name}, ID: {pointer.MapID}.");
            dataManager.SaveData(pointers);
            return true;
        }
    }

}
