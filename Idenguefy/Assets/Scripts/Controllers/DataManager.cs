using Idenguefy.Entities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Idenguefy.Controllers
{
    /// <summary>
    /// A generic data manager for saving and loading lists of serializable data using PlayerPrefs.
    /// It wraps the list in a container class to allow for JSON serialization.
    /// Works for pointers, settings, or any other [System.Serializable] class.
    /// </summary>
    /// <typeparam name="T">The type of data being managed (e.g., MapPointer, Settings).</typeparam>
    /// <remarks>
    /// Author: Sharina
    /// Version: 1.0
    /// Notes: N/A
    /// </remarks>
    public class DataManager<T>
    {
        /// <summary>
        /// The key used to store the data in PlayerPrefs.
        /// </summary>
        private string storageKey;

        /// <summary>
        /// A private inner wrapper class used to serialize a list.
        /// Unity's JsonUtility cannot serialize a root-level list directly,
        /// so the list must be a field within a serializable class.
        /// </summary>
        /// <typeparam name="U">The type of data in the list.</typeparam>
        [System.Serializable]
        private class DataWrapper<U>
        {
            /// <summary>
            /// The list of data being wrapped for serialization.
            /// </summary>
            public List<U> dataList = new List<U>();
        }

        /// <summary>
        /// Initializes a new instance of the DataManager with a specific storage key.
        /// </summary>
        /// <param name="key">The unique key to use for PlayerPrefs storage.</param>
        public DataManager(string key)
        {
            storageKey = key;
        }

        /// <summary>
        /// Saves the provided list of data to PlayerPrefs by serializing it to JSON.
        /// This will overwrite any existing data at the same storage key.
        /// </summary>
        /// <param name="dataList">The list of data to save.</param>
        public void SaveData(List<T> dataList)
        {
            PlayerPrefs.DeleteKey(storageKey); // Clear old data first
            DataWrapper<T> wrapper = new DataWrapper<T> { dataList = dataList };
            string json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(storageKey, json);
            PlayerPrefs.Save(); // Commit changes to disk
            Debug.Log(PlayerPrefs.GetString(storageKey));
            Debug.Log($"Saving {dataList.Count} {typeof(T).Name}(s) under key '{storageKey}'.");
        }

        /// <summary>
        /// Loads a list of items from PlayerPrefs by deserializing it from JSON.
        /// Returns an empty list if no valid data is found or the key does not exist.
        /// </summary>
        /// <returns>A List{T} containing the loaded data, or an empty list if loading fails.</returns>
        public List<T> LoadData()
        {
            if (PlayerPrefs.HasKey(storageKey))
            {
                string json = PlayerPrefs.GetString(storageKey);
                DataWrapper<T> deWrapper = JsonUtility.FromJson<DataWrapper<T>>(json);

                Debug.Log(json);

                if (deWrapper != null && deWrapper.dataList != null)
                {
                    Debug.Log($"{typeof(T).Name} data has been loaded from {storageKey}");
                    Debug.Log($"Loaded Count: {deWrapper.dataList.Count}");
                    return deWrapper.dataList;
                }
                else
                {
                    Debug.Log($"No valid {typeof(T).Name} data found, created new list");
                    return new List<T>();
                }
            }
            else
            {
                Debug.Log($"No {typeof(T).Name} data found under {storageKey}, created new list");
                return new List<T>();
            }
        }

    }
}