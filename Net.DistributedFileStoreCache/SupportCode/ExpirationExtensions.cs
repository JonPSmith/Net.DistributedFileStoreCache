// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using Microsoft.Extensions.Caching.Distributed;

namespace Net.DistributedFileStoreCache.SupportCode;

internal static class ExpirationExtensions
{
    /// <summary>
    /// This sets up the timeout if a <see cref="DistributedCacheEntryOptions"/> was provided
    /// </summary>
    /// <param name="cache"></param>
    /// <param name="key"></param>
    /// <param name="entryOptions"></param>
    /// <exception cref="NotImplementedException"></exception>
    public static void SetupTimeoutIfOptions(this CacheJsonContent cache, string key, DistributedCacheEntryOptions? entryOptions)
    {
        if (entryOptions == null)
            return;

        if (entryOptions.SlidingExpiration != null)
            throw new NotImplementedException("This library doesn't support sliding expirations for performance reasons.");

        if (entryOptions.AbsoluteExpiration != null)
        {
            //see https://stackoverflow.com/a/1688799/1434764 answer that says it uses utc
            cache.TimeOuts[key] = entryOptions.AbsoluteExpiration.Value.ToUniversalTime().Ticks;
        }
        else if (entryOptions.AbsoluteExpirationRelativeToNow != null)
        {
            cache.TimeOuts[key]  = DateTime.UtcNow.Add(
                (TimeSpan)entryOptions.AbsoluteExpirationRelativeToNow!).Ticks;
        }
    }

    public static bool HasExpired(this long timeoutTicks)
    {
        return timeoutTicks < DateTime.UtcNow.Ticks;
    }

    /// <summary>
    /// This returns null if there no set value, or if it is expired.
    /// Otherwise it returns the value.
    /// </summary>
    /// <param name="cache"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string? ReturnNullIfExpires(this CacheJsonContent cache, string key)
    {
        if (!StaticCachePart.CacheContent.Cache.TryGetValue(key, out string? value))
            return null;

        if (cache.TimeOuts.TryGetValue(key, out long timeoutTicks))
        {
            if (timeoutTicks.HasExpired())
                //it is timed out
                return null;
        }

        return value;
    }

    public static IReadOnlyDictionary<string, string> ReturnNonExpiredCacheValues(this CacheJsonContent cacheContent)
    {
        foreach (var key in cacheContent.TimeOuts.Keys.Where(key => cacheContent.TimeOuts[key].HasExpired()))
        {
            cacheContent.Cache.Remove(key);
        }
        return new ReadOnlyDictionary<string, string>(cacheContent.Cache);
    }

    public static void RemoveExpiredCacheValues(this CacheJsonContent cacheContent)
    {
        foreach (var key in cacheContent.TimeOuts.Keys.Where(key => cacheContent.TimeOuts[key].HasExpired()))
        {
            cacheContent.Cache.Remove(key);
            cacheContent.TimeOuts.Remove(key);
        }
    }
}