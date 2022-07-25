// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.Extensions.Caching.Distributed;

namespace Net.DistributedFileStoreCache.SupportCode;

public class CacheEntryOptions
{
    public CacheEntryOptions(){}

    public CacheEntryOptions(DistributedCacheEntryOptions entryOptions)
    {
        if (entryOptions == null) throw new ArgumentNullException(nameof(entryOptions));

        if (entryOptions.AbsoluteExpiration != null)
        {
            //see https://stackoverflow.com/a/1688799/1434764 answer that says it uses utc
            TimeOutTimeUtc = entryOptions.AbsoluteExpiration.Value.ToUniversalTime().Ticks;
        }
        else if (entryOptions.AbsoluteExpirationRelativeToNow != null)
        {
            TimeOutTimeUtc = DateTime.UtcNow.Add(
                (TimeSpan)entryOptions.AbsoluteExpirationRelativeToNow!).Ticks;
        }
        else if (entryOptions.SlidingExpiration != null)
        {
            TimeOutTimeUtc = DateTime.UtcNow.Add((TimeSpan)entryOptions.SlidingExpiration!).Ticks;
        }

        SlidingExpiration = entryOptions.SlidingExpiration;
    }

    /// <summary>
    /// This contains the time when the named value is invalid
    /// </summary>
    public long TimeOutTimeUtc { get; set; }

    /// <summary>
    /// If not null, then it contains the SlidingExpiration
    /// </summary>
    public TimeSpan? SlidingExpiration { get; set; }

    /// <summary>
    /// This returns true if the key/value has expired.
    /// </summary>
    /// <returns></returns>
    public bool HasExpired()
    {
        if (TimeOutTimeUtc < DateTime.UtcNow.Ticks)
            return true;

        RefreshSlidingExpirationIfThere();

        return false;
    }

    /// <summary>
    /// If the entryOptions has a non-null <see cref="SlidingExpiration"/> it will update the time when
    /// the key/value expires and then returns true to say that the <see cref="SlidingExpiration"/> was updated
    /// </summary>
    /// <returns></returns>
    public bool RefreshSlidingExpirationIfThere()
    {
        if (SlidingExpiration == null) return false;

        TimeOutTimeUtc = DateTime.UtcNow.Add((TimeSpan)SlidingExpiration!).Ticks;
        return true;
    }


}