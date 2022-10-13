// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

namespace Net.DistributedFileStoreCache.SupportCode;

internal class CacheFileRemove
{
    private readonly string _key;

    public CacheFileRemove(string key)
    {
        _key = key ?? throw new ArgumentNullException(nameof(key), "The key cannot be null");
    }

    public void RemoveKeyValueHandler(ref CacheJsonContent currentJson)
    {
        currentJson.Cache.Remove(_key);
        currentJson.TimeOuts.Remove(_key);
    }
}