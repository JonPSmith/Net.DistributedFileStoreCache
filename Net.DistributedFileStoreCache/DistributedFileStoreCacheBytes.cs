// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Text;
using Microsoft.Extensions.Caching.Distributed;

namespace Net.DistributedFileStoreCache;

public class DistributedFileStoreCacheBytes : IDistributedFileStoreCacheBytes
{
    private readonly IDistributedFileStoreCacheString _stringCache;

    public DistributedFileStoreCacheBytes(IDistributedFileStoreCacheString stringCache)
    {
        _stringCache = stringCache;
    }

    /// <summary>Gets a value with the given key.</summary>
    /// <param name="key">A string identifying the requested value.</param>
    /// <returns>The located value or null.</returns>
    public byte[]? Get(string key)
    {
        var stringValue = _stringCache.Get(key);
        if (stringValue == null)
            return null;
        return Encoding.UTF8.GetBytes(stringValue);
    }

    /// <summary>Gets a value with the given key.</summary>
    /// <param name="key">A string identifying the requested value.</param>
    /// <param name="token">Optional. The <see cref="T:System.Threading.CancellationToken" /> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation, containing the located value or null.</returns>
    public async Task<byte[]?> GetAsync(string key, CancellationToken token = new CancellationToken())
    {
        var stringValue = await _stringCache.GetAsync(key, token);
        if (stringValue == null)
            return null;
        return Encoding.UTF8.GetBytes(stringValue);
    }

    /// <summary>Sets a value with the given key.</summary>
    /// <param name="key">A string identifying the requested value.</param>
    /// <param name="value">The value to set in the cache.</param>
    /// <param name="options">The cache options for the value.</param>
    public void Set(string key, byte[] value, DistributedCacheEntryOptions? options)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        _stringCache.Set(key, Encoding.UTF8.GetString(value), options);
    }

    /// <summary>Sets the value with the given key.</summary>
    /// <param name="key">A string identifying the requested value.</param>
    /// <param name="value">The value to set in the cache.</param>
    /// <param name="options">The cache options for the value.</param>
    /// <param name="token">Optional. The <see cref="T:System.Threading.CancellationToken" /> used to propagate notifications that the operation should be canceled.</param>
    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions? options,
        CancellationToken token = new CancellationToken())
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        return _stringCache.SetAsync(key, Encoding.UTF8.GetString(value), options, token);
    }

    /// <summary>
    /// Refreshes a value in the cache based on its key, resetting its sliding expiration timeout (if any).
    /// </summary>
    /// <param name="key">A string identifying the requested value.</param>
    public void Refresh(string key)
    {
        _stringCache.Refresh(key);
    }

    /// <summary>
    /// Refreshes a value in the cache based on its key, resetting its sliding expiration timeout (if any).
    /// </summary>
    /// <param name="key">A string identifying the requested value.</param>
    /// <param name="token">Optional. The <see cref="T:System.Threading.CancellationToken" /> used to propagate notifications that the operation should be canceled.</param>
    public Task RefreshAsync(string key, CancellationToken token = new CancellationToken())
    {
        return _stringCache.RefreshAsync(key);
    }

    /// <summary>Removes the value with the given key.</summary>
    /// <param name="key">A string identifying the requested value.</param>
    public void Remove(string key)
    {
        _stringCache.Remove(key);
    }

    /// <summary>Removes the value with the given key.</summary>
    /// <param name="key">A string identifying the requested value.</param>
    /// <param name="token">Optional. The <see cref="T:System.Threading.CancellationToken" /> used to propagate notifications that the operation should be canceled.</param>
    public Task RemoveAsync(string key, CancellationToken token = new CancellationToken())
    {
        return _stringCache.RemoveAsync(key, token);
    }

    /// <summary>
    /// This clears all the key/value pairs from the json cache file
    /// </summary>
    public void ClearAll()
    {
        _stringCache.ClearAll();
    }

    /// <summary>
    /// This return all the cached values as a dictionary
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, byte[]> GetAllKeyValues()
    {
        var stringValues = _stringCache.GetAllKeyValues();

        var stringByteDictionary = new Dictionary<string, byte[]>();
        foreach (var key in stringValues.Keys)
        {
            stringByteDictionary.Add(key, Encoding.UTF8.GetBytes(stringValues[key]));
        }

        return stringByteDictionary;
    }

    public async Task<IReadOnlyDictionary<string, byte[]>> GetAllKeyValuesAsync()
    {
        var stringValues = await _stringCache.GetAllKeyValuesAsync();

        var stringByteDictionary = new Dictionary<string, byte[]>();
        foreach (var key in stringValues.Keys)
        {
            stringByteDictionary.Add(key, Encoding.UTF8.GetBytes(stringValues[key]));
        }

        return stringByteDictionary;
    }
}