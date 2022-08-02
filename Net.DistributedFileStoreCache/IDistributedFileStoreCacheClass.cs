// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.Extensions.Caching.Distributed;

namespace Net.DistributedFileStoreCache;

/// <summary>
/// Interface for the Class FileStore cache version. Note that it also inherits the <see cref="IDistributedFileStoreCacheString"/> interface.
/// </summary>
public interface IDistributedFileStoreCacheClass : IDistributedFileStoreCacheString
{
    /// <summary>
    /// This method is useful if you want to decode a cache value via the <see cref="DistributedFileStoreCacheString.GetAllKeyValues"/>
    /// or the <see cref="DistributedFileStoreCacheString.GetAllKeyValuesAsync"/> methods
    /// </summary>
    /// <typeparam name="T">A class which can be created</typeparam>
    /// <param name="jsonString"></param>
    /// <returns>The deserialize class or null.</returns>
    T? GetClassFromString<T>(string? jsonString) where T : class, new();

    /// <summary>Gets a class stored as json linked to the given key.</summary>
    /// <param name="key">A string identifying the requested stored class.</param>
    /// <typeparam name="T">A class which can be created</typeparam>
    /// <returns>The deserialize class or null.</returns>
    T? GetClass<T>(string key) where T : class, new();

    /// <summary>Gets a class stored as json linked to the given key.</summary>
    /// <param name="key">A string identifying the requested stored class.</param>
    /// <param name="token">Optional. The <see cref="T:System.Threading.CancellationToken" /> used to propagate notifications that the operation should be canceled.</param>
    /// <typeparam name="T">A class which can be created</typeparam>
    /// <returns>The located class or null withing a Task result.</returns>
    Task<T?> GetClassAsync<T>(string key, CancellationToken token = new CancellationToken()) where T : class, new();

    /// <summary>Serializers the class and stores the json against the given key.</summary>
    /// <param name="key">A string identifying the requested value.</param>
    /// <param name="yourClass">The class that you wanted to be stored in the cache.</param>
    /// <param name="options">The cache options for the value.</param>
    /// <typeparam name="T">A class which can be created</typeparam>
    void SetClass<T>(string key, T yourClass, DistributedCacheEntryOptions? options) where T : class, new();

    /// <summary>Serializers the class and stores the json against the given key.</summary>
    /// <param name="key">A string identifying the requested value.</param>
    /// <param name="yourClass">The class that you wanted to be stored in the cache.</param>
    /// <param name="options">The cache options for the value.</param>
    /// <param name="token">Optional. The <see cref="T:System.Threading.CancellationToken" /> used to propagate notifications that the operation should be canceled.</param>
    /// <typeparam name="T">A class which can be created</typeparam>
    Task SetClassAsync<T>(string key, T yourClass, DistributedCacheEntryOptions? options,
        CancellationToken token = new CancellationToken()) where T : class, new();
}