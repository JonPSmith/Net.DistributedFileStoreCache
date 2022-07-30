// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.Extensions.Caching.Distributed;

namespace Net.DistributedFileStoreCache;

/// <summary>
/// This adds a couple of useful features beyond the <see cref="IDistributedCache"/> interface
/// </summary>
public interface IDistributedFileStoreCacheBytes : IDistributedCache
{
    /// <summary>
    /// This clears all the key/value pairs from the json cache file
    /// </summary>
    void ClearAll();

    /// <summary>
    /// This return all the cached values as a dictionary
    /// </summary>
    /// <returns></returns>
    Dictionary<string, byte[]> GetAllKeyValues();

    /// <summary>
    /// This return all the cached values as a dictionary
    /// </summary>
    /// <returns></returns>
    Task<IReadOnlyDictionary<string, byte[]>> GetAllKeyValuesAsync();
}