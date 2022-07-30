// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.Extensions.Caching.Distributed;

namespace Net.DistributedFileStoreCache;

public interface IDistributedFileStoreCacheString
{
    /// <summary>Gets a value with the given key.</summary>
    /// <param name="key">A string identifying the requested value.</param>
    /// <returns>The located value or null.</returns>
    string? Get(string key);

    /// <summary>Gets a value with the given key.</summary>
    /// <param name="key">A string identifying the requested value.</param>
    /// <param name="token">Optional. The <see cref="T:System.Threading.CancellationToken" /> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation, containing the located value or null.</returns>
    Task<string?> GetAsync(string key, CancellationToken token = new CancellationToken());

    /// <summary>Sets a value with the given key.</summary>
    /// <param name="key">A string identifying the requested value.</param>
    /// <param name="value">The value to set in the cache.</param>
    /// <param name="options">The cache options for the value.</param>
    void Set(string key, string value, DistributedCacheEntryOptions? options);

    /// <summary>Sets the value with the given key.</summary>
    /// <param name="key">A string identifying the requested value.</param>
    /// <param name="value">The value to set in the cache.</param>
    /// <param name="options">The cache options for the value.</param>
    /// <param name="token">Optional. The <see cref="T:System.Threading.CancellationToken" /> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
    Task SetAsync(string key, string value, DistributedCacheEntryOptions? options,
        CancellationToken token = new CancellationToken());

    /// <summary>Removes the value with the given key.</summary>
    /// <param name="key">A string identifying the requested value.</param>
    void Remove(string key);

    /// <summary>Removes the value with the given key.</summary>
    /// <param name="key">A string identifying the requested value.</param>
    /// <param name="token">Optional. The <see cref="T:System.Threading.CancellationToken" /> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
    Task RemoveAsync(string key, CancellationToken token = new CancellationToken());

    /// <summary>
    /// This clears all the key/value pairs from the json cache file
    /// </summary>
    void ClearAll();

    /// <summary>
    /// This return all the cached values as a dictionary
    /// </summary>
    /// <returns></returns>
    IReadOnlyDictionary<string, string> GetAllKeyValues();

    /// <summary>
    /// This return all the cached values as a dictionary
    /// </summary>
    /// <returns></returns>
    Task<IReadOnlyDictionary<string, string>> GetAllKeyValuesAsync(CancellationToken token = new CancellationToken());
}