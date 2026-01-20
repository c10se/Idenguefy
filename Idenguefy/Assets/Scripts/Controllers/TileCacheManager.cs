using Idenguefy.Controllers;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Unity.Multiplayer.Center.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Manages the disk-based caching of map tiles.
/// Tiles are compressed using GZip to save space and are stored in a platform-specific persistent data path.
/// Includes thread-safe mechanisms to prevent reading a tile while it is being written.
/// </summary>
/// <remarks>
/// Author: Eben, Ryan
/// Version: 1.0
/// </remarks>
public class TileCacheManager : MonoBehaviour
{
    /// <summary>
    /// Gets the platform-specific root path for the tile cache.
    /// Uses LocalApplicationData in the Editor and persistentDataPath in builds.
    /// </summary>
    private static string CachePath
    {
        get
        {
            string basePath = null;
#if UNITY_EDITOR
            basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
#else
            basePath = Application.persistentDataPath;
#endif
            // Ensures a clean, platform-agnostic path.
            string appCache = Path.Combine(basePath, "Idenguefy", "Cache");
            return appCache;
        }
    }

    /// <summary>
    /// Lock object to ensure thread-safe access to the pendingCacheLoad HashSet.
    /// </summary>
    private readonly object pendingCacheLock = new object();

    /// <summary>
    /// A HashSet containing the file names of tiles that are currently being written to disk.
    /// This prevents attempts to read incomplete cache files.
    /// </summary>
    private HashSet<string> pendingCacheLoad = new HashSet<string>();

    /// <summary>
    /// Checks if a specific tile is currently being written to the cache.
    /// </summary>
    /// <param name="x">The tile X coordinate.</param>
    /// <param name="y">The tile Y coordinate.</param>
    /// <returns>True if the tile is being written, false otherwise.</returns>
    public bool checkAnyPendingLoad(int x, int y)
    {
        string key = getFileNameByTile(x, y);
        lock (pendingCacheLock)
            return pendingCacheLoad.TryGetValue(key, out _);
    }

    /// <summary>
    /// Caches a tile texture to disk.
    /// The texture is encoded to PNG and then compressed using GZip.
    /// </summary>
    /// <param name="tileTexture">The Texture2D object of the tile.</param>
    /// <param name="x">The tile X coordinate.</param>
    /// <param name="y">The tile Y coordinate.</param>
    public void CacheTileData(Texture2D tileTexture, int x, int y)
    {
        string fileName = getFileNameByTile(x, y);
        //Debug.Log($"[TileCacheManager] Caching tile ({x}, {y}) at {fileName}");
        Compress(tileTexture.EncodeToPNG(), fileName);
    }

    /// <summary>
    /// Asynchronously retrieves a cached tile from the disk.
    /// </summary>
    /// <param name="x">The tile X coordinate.</param>
    /// <param name="y">The tile Y coordinate.</param>
    /// <returns>
    /// A Task that represents the asynchronous operation.
    /// The result is the loaded Texture2D if found, or null if a cache miss occurs.
    /// </returns>
    public async Task<Texture2D> RetrieveCachedTile(int x, int y)
    {
        string fileName = getFileNameByTile(x, y);
        // Run the decompression on a background thread to avoid blocking the main thread.
        byte[] result = await Task.Run(() => Decompress(fileName));
        if (result != null)
        {
            //Debug.Log($"[TileCacheManager] Cache hit for tile ({x}, {y})");
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(result); // LoadImage must be on the main thread (implicitly handled by await)
            Debug.Log($"[TileCacheManager] Retrieved cached tile ({x}, {y}) from cache.");
            return texture;
        }
        else
        {
            //Debug.Log($"[TileCacheManager] Cache miss for tile ({x}, {y})");
            return null;
        }
    }

    /// <summary>
    /// Checks if the cache directory exists, and creates it if it does not.
    /// </summary>
    private void CheckCacheDirectoryExistence()
    {
        if (!Directory.Exists(CachePath))
        {
            Directory.CreateDirectory(CachePath);
            Debug.Log($"[TileCacheManager] Cache directory created at: {CachePath}");
        }
    }

    /// <summary>
    /// Compresses a byte array using GZip and writes it to a file asynchronously.
    /// Manages the pending cache load state for thread safety.
    /// </summary>
    /// <param name="bytes">The raw byte data to compress (e.g., from Texture2D.EncodeToPNG()).</param>
    /// <param name="filePath">The file name (not the full path) for the cache file.</param>
    private async void Compress(byte[] bytes, string filePath)
    {
        CheckCacheDirectoryExistence();
        lock (pendingCacheLock)
            pendingCacheLoad.Add(filePath); // Mark file as being written

        string fullPath = Path.Combine(CachePath, filePath);
        Debug.Log($"[TileCacheManager] Compressing and caching to {fullPath}");

        try
        {
            using (FileStream fileStream = new FileStream(fullPath, FileMode.Create))
            using (GZipStream gzipStream = new GZipStream(fileStream, CompressionMode.Compress))
            {
                await gzipStream.WriteAsync(bytes, 0, bytes.Length);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[TileCacheManager] Failed to compress cache file {fullPath}: {ex.Message}");
        }
        finally
        {
            lock (pendingCacheLock)
                pendingCacheLoad.Remove(filePath); // Mark file as finished writing
        }
    }

    /// <summary>
    /// Decompresses a GZip file from the cache directory and returns its raw byte array.
    /// </summary>
    /// <param name="fileName">The file name (not the full path) to read from.</param>
    /// <returns>A byte array of the decompressed data, or null if the file doesn't exist or is being written.</returns>
    private byte[] Decompress(string fileName)
    {
        CheckCacheDirectoryExistence();
        string fullPath = Path.Combine(CachePath, fileName);

        // Do not read if file doesn't exist or is currently being written by another thread.
        if (!File.Exists(fullPath) || pendingCacheLoad.Contains(fileName))
        {
            // Debug.Log($"[TileCacheManager] No cache file found at {fullPath} or file is still being written.");
            return null;
        }

        try
        {
            Debug.Log($"[TileCacheManager] Decompressing cache from {fullPath}");
            using (FileStream fileStream = new FileStream(fullPath, FileMode.Open))
            using (GZipStream gzipStream = new GZipStream(fileStream, CompressionMode.Decompress))
            using (MemoryStream memoryStream = new MemoryStream())
            {
                gzipStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[TileCacheManager] Failed to decompress cache file {fullPath}: {ex.Message}. File might be corrupt.");
            return null;
        }
    }

    /// <summary>
    /// Generates a unique cache key string for a given tile coordinate.
    /// </summary>
    /// <param name="x">The tile X coordinate.</param>
    /// <param name="y">The tile Y coordinate.</param>
    /// <returns>A string key (e.g., "123_456").</returns>
    private string getCacheKey(int x, int y)
    {
        return $"{x}_{y}";
    }

    /// <summary>
    /// Gets the file name for a tile based on its coordinates.
    /// </summary>
    /// <param name="x">The tile X coordinate.</param>
    /// <param name="y">The tile Y coordinate.</param>
    /// <returns>The file name (e.g., "123_456").</returns>
    private string getFileNameByTile(int x, int y)
    {
        return getCacheKey(x, y);
    }

    /// <summary>
    /// Clears the custom tile cache directory and Unity's built-in cache.
    /// </summary>
    public void ClearTileCache()
    {
        try
        {
            // Clear custom disk cache
            if (Directory.Exists(CachePath))
            {
                Directory.Delete(CachePath, true);
                Debug.Log($"[TileCacheManager] Cache cleared at: {CachePath}");
            }

            // Clear Unity's internal cache (e.g., for UnityWebRequests)
            if (Caching.ClearCache())
                Debug.Log("[TileCacheManager] Unity cache cleared successfully.");
            else
                Debug.LogWarning("[TileCacheManager] Could not clear Unity cache (may still be in use).");

            CheckCacheDirectoryExistence(); // Recreate folder if needed
        }
        catch (Exception ex)
        {
            Debug.LogError($"[TileCacheManager] Error clearing cache: {ex.Message}");
        }
    }
}