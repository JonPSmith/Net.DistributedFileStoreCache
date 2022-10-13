// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.Extensions.Caching.Distributed;
using Net.DistributedFileStoreCache.SupportCode;

namespace Net.DistributedFileStoreCache;

/// <summary>
/// This is the Distributed FileStore cache that has a value of type string.
/// This is the primary FileStore cache version that the other versions link to this class
/// </summary>
public class DistributedFileStoreCacheString : IDistributedFileStoreCacheString
{
    /// <summary>
    /// This class directly creates the <see cref="CacheFileHandler"/> which provides read/write access of the cache json file
    /// </summary>
    protected readonly CacheFileHandler CacheFileHandler;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="fileStoreCacheOptions"></param>
    public DistributedFileStoreCacheString(DistributedFileStoreCacheOptions fileStoreCacheOptions)
    {
        CacheFileHandler = new CacheFileHandler(fileStoreCacheOptions);
    }

    /// <summary>Gets a value with the given key.</summary>
    /// <param name="key">A string identifying the requested value.</param>
    /// <returns>The located value or null.</returns>
    public string? Get(string key)
    {
        return CacheFileHandler.GetValue(key);
    }

    /// <summary>Gets a value with the given key.</summary>
    /// <param name="key">A string identifying the requested value.</param>
    /// <param name="token">Optional. The <see cref="T:System.Threading.CancellationToken" /> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation, containing the located value or null.</returns>
    public Task<string?> GetAsync(string key, CancellationToken token = new CancellationToken())
    {
        return CacheFileHandler.GetValueAsync(key, token);
    }

    /// <summary>Sets a value with the given key.</summary>
    /// <param name="key">A string identifying the requested value.</param>
    /// <param name="value">The value to set in the cache.</param>
    /// <param name="options">The cache options for the value.</param>
    public void Set(string key, string value, DistributedCacheEntryOptions? options)
    {
        CacheFileHandler.SetKeyValue(key, value, options);
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
        return CacheFileHandler.SetKeyValueAsync(key, value, options, token);
    }

    /// <summary>Sets many entries via a list of KeyValues</summary>
    /// <param name="manyEntries">List of KeyValuePairs to be added to the cache.</param>
    /// <param name="options">Optional: The cache options for the value.</param>
    public void SetMany(List<KeyValuePair<string, string>> manyEntries, DistributedCacheEntryOptions? options)
    {
        CacheFileHandler.SetKeyValueMany(manyEntries, options);
    }


    /// <summary>Sets many entries via a list of KeyValues</summary>
    /// <param name="manyEntries">List of KeyValuePairs to be added to the cache.</param>
    /// <param name="options">Optional: The cache options for the value.</param>
    /// <param name="token">Optional. The <see cref="T:System.Threading.CancellationToken" /> used to propagate notifications that the operation should be canceled.</param>
    public Task SetManyAsync(List<KeyValuePair<string, string>> manyEntries, DistributedCacheEntryOptions? options,
        CancellationToken token = new ())
    {
        return CacheFileHandler.SetKeyValueManyAsync(manyEntries, options, token);
    }

    /// <summary>Removes the value with the given key.</summary>
    /// <param name="key">A string identifying the requested value.</param>
    public void Remove(string key)
    {
        CacheFileHandler.RemoveKeyValue(key);
    }

    /// <summary>Removes the value with the given key.</summary>
    /// <param name="key">A string identifying the requested value.</param>
    /// <param name="token">Optional. The <see cref="T:System.Threading.CancellationToken" /> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
    public Task RemoveAsync(string key, CancellationToken token = new CancellationToken())
    {
        return CacheFileHandler.RemoveKeyValueAsync(key, token);
    }

    /// <summary>
    /// This clears all the key/value pairs from the json cache file, with option to add entries after the cache is cleared.
    /// </summary>
    /// <param name="manyEntries">Optional: After of the clearing the cache these KeyValues will written into the cache</param>
    /// <param name="entryOptions">Optional: If there are entries to add to the cache, this will set the timeout time.</param>
    public void ClearAll(List<KeyValuePair<string, string>>? manyEntries = null, DistributedCacheEntryOptions? entryOptions = null)
    {
        CacheFileHandler.ResetCacheFile(manyEntries, entryOptions);
    }

    /// <summary>
    /// This return all the cached values as a dictionary
    /// </summary>
    /// <returns></returns>
    public IReadOnlyDictionary<string, string> GetAllKeyValues()
    {
        return CacheFileHandler.GetAllValues();
    }

    /// <summary>
    /// This return all the cached values as a dictionary
    /// </summary>
    /// <returns></returns>
    public Task<IReadOnlyDictionary<string, string>> GetAllKeyValuesAsync(CancellationToken token = new CancellationToken())
    {
        return CacheFileHandler.GetAllValuesAsync(token);
    }
}