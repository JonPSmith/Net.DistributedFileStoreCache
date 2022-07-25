// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Test")]

namespace Net.DistributedFileStoreCache.SupportCode;

/// <summary>
/// This static class contains the
/// </summary>
internal static class StaticCachePart
{
    /// <summary>
    /// If this is true, then the cache file must be read in into the 
    /// <see cref="ReadCacheKeyValues"/> static dictionary
    /// </summary>
    public static bool LocalCacheIsOutOfDate { get; private set; }

    /// <summary>
    /// This contains the keys/values in this distributed cache.
    /// This is read-only and it is updated by reading the cache file 
    /// </summary>
    public static IReadOnlyDictionary<string, string>? ReadCacheKeyValues => _cacheKeyValues;

    //private values
    private static Dictionary<string, string>? _cacheKeyValues = new ();
    private static FileSystemWatcher? _watcher;
    private static string? _cacheFilePathCheck;

    /// <summary>
    /// This should be called on startup after the <see cref="DistributedFileStoreCacheOptions"/> has been set.
    /// Its job is to set up the file watcher.
    /// </summary>
    /// <param name="fileStoreCacheOptions"></param>
    public static void SetupStaticCache(DistributedFileStoreCacheOptions fileStoreCacheOptions)
    {
        if (fileStoreCacheOptions.PathToCacheFileDirectory == null)
            throw new ArgumentNullException(nameof(fileStoreCacheOptions.PathToCacheFileDirectory));

        _cacheFilePathCheck ??= fileStoreCacheOptions.FormCacheFilePath();
        if (_cacheFilePathCheck != fileStoreCacheOptions.FormCacheFilePath() && !fileStoreCacheOptions.TurnOffStaticFilePathCheck)
            //You can only have one static 
            throw new DistributedFileStoreCacheException(
                "You are trying re-registered the static cache part to a different filepath, which is not allowed.");

        //Make sure there is a cache file
        var cacheHandler = new CacheFileHandler(fileStoreCacheOptions);
        cacheHandler.CreateNewCacheFileIfMissingWithRetry();
        LocalCacheIsOutOfDate = true;

        _watcher = new FileSystemWatcher(fileStoreCacheOptions.PathToCacheFileDirectory,
            fileStoreCacheOptions.FormCacheFileName());
        _watcher.EnableRaisingEvents = true;
        _watcher.NotifyFilter = NotifyFilters.LastWrite;

        _watcher.Changed += (sender, args) =>
        {
            //when the cache file is changed, then the local 
            LocalCacheIsOutOfDate = true;
        };
    }

    /// <summary>
    /// This updates the in-memory version of the cache and sets the <see cref="LocalCacheIsOutOfDate"/> to false
    /// </summary>
    /// <param name="updatedCache"></param>
    public static void UpdateInMemoryCache(Dictionary<string, string> updatedCache)
    {
        _cacheKeyValues = updatedCache;
        LocalCacheIsOutOfDate = false;
    }
}