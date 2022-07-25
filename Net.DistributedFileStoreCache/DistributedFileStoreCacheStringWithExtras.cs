// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.Extensions.Caching.Distributed;
using Net.DistributedFileStoreCache.SupportCode;

namespace Net.DistributedFileStoreCache;

public class DistributedFileStoreCacheStringWithExtras : IDistributedFileStoreCacheStringWithExtras
{
    private readonly CacheFileHandler _cacheFileHandler;


    public DistributedFileStoreCacheStringWithExtras(DistributedFileStoreCacheOptions fileStoreCacheOptions)
    {
        _cacheFileHandler = new CacheFileHandler(fileStoreCacheOptions);
    }

    /// <summary>Gets a value with the given key.</summary>
    /// <param name="key">A string identifying the requested value.</param>
    /// <returns>The located value or null.</returns>
    public string? Get(string key)
    {
        return _cacheFileHandler.GetValue(key);
    }

    /// <summary>Gets a value with the given key.</summary>
    /// <param name="key">A string identifying the requested value.</param>
    /// <param name="token">Optional. The <see cref="T:System.Threading.CancellationToken" /> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation, containing the located value or null.</returns>
    public Task<string?> GetAsync(string key, CancellationToken token = new CancellationToken())
    {
        return _cacheFileHandler.GetValueAsync(key);
    }

    /// <summary>Sets a value with the given key.</summary>
    /// <param name="key">A string identifying the requested value.</param>
    /// <param name="value">The value to set in the cache.</param>
    /// <param name="options">The cache options for the value.</param>
    public void Set(string key, string value, DistributedCacheEntryOptions? options)
    {
        _cacheFileHandler.AddKeyValueToCacheFile(key, value);
    }

    /// <summary>Sets the value with the given key.</summary>
    /// <param name="key">A string identifying the requested value.</param>
    /// <param name="value">The value to set in the cache.</param>
    /// <param name="options">The cache options for the value.</param>
    /// <param name="token">Optional. The <see cref="T:System.Threading.CancellationToken" /> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
    public Task SetAsync(string key, string value, DistributedCacheEntryOptions? options,
        CancellationToken token = new CancellationToken())
    {
        return _cacheFileHandler.AddKeyValueToCacheFileAsync(key, value);
    }

    /// <summary>
    /// Refreshes a value in the cache based on its key, resetting its sliding expiration timeout (if any).
    /// </summary>
    /// <param name="key">A string identifying the requested value.</param>
    public void Refresh(string key)
    {
        _cacheFileHandler.RefreshCacheFile();
    }

    /// <summary>
    /// Refreshes a value in the cache based on its key, resetting its sliding expiration timeout (if any).
    /// </summary>
    /// <param name="key">A string identifying the requested value.</param>
    /// <param name="token">Optional. The <see cref="T:System.Threading.CancellationToken" /> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
    public Task RefreshAsync(string key, CancellationToken token = new CancellationToken())
    {
        return _cacheFileHandler.RefreshCacheFileAsync();
    }

    /// <summary>Removes the value with the given key.</summary>
    /// <param name="key">A string identifying the requested value.</param>
    public void Remove(string key)
    {
        _cacheFileHandler.RemoveKeyValueToCacheFile(key);
    }

    /// <summary>Removes the value with the given key.</summary>
    /// <param name="key">A string identifying the requested value.</param>
    /// <param name="token">Optional. The <see cref="T:System.Threading.CancellationToken" /> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
    public Task RemoveAsync(string key, CancellationToken token = new CancellationToken())
    {
        return _cacheFileHandler.RemoveKeyValueToCacheFileAsync(key);
    }

    /// <summary>
    /// This clears all the key/value pairs from the json cache file
    /// </summary>
    public void ClearAll()
    {
        _cacheFileHandler.ResetCacheFile();
    }

    /// <summary>
    /// This return all the cached values as a dictionary
    /// </summary>
    /// <returns></returns>
    public IReadOnlyDictionary<string, string> GetAllKeyValues()
    {
        return _cacheFileHandler.GetAllValues();
    }

    /// <summary>
    /// This return all the cached values as a dictionary
    /// </summary>
    /// <returns></returns>
    public Task<IReadOnlyDictionary<string, string>> GetAllKeyValuesAsync()
    {
        return _cacheFileHandler.GetAllValuesAsync();
    }
}