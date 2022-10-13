// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.Extensions.Caching.Distributed;

namespace Net.DistributedFileStoreCache.SupportCode;

internal class CacheFileSetMany
{
    private readonly List<KeyValuePair<string, string>>? _manyEntries;
    private readonly DistributedCacheEntryOptions? _timeoutOptions;

    public CacheFileSetMany(List<KeyValuePair<string, string>>? manyEntries, DistributedCacheEntryOptions? timeoutOptions)
    {
        _manyEntries = manyEntries;
        _timeoutOptions = timeoutOptions;
    }

    public void SetManyKeyValueHandler(ref CacheJsonContent currentJson)
    {
        if (_manyEntries == null || !_manyEntries.Any())
            return;

        foreach (var keyValue in _manyEntries)
        {
            if (keyValue.Key == null) throw new NullReferenceException("The key of a KeyPair cannot be null");
            if (keyValue.Value == null) throw new NullReferenceException("The value of a KeyPair cannot be null");
            currentJson.Cache[keyValue.Key] = keyValue.Value;
            ExpirationExtensions.SetupTimeoutIfOptions(ref currentJson, keyValue.Key, _timeoutOptions);
        }
    }
}