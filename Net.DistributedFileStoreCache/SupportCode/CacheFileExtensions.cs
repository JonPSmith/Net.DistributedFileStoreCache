// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

namespace Net.DistributedFileStoreCache.SupportCode;

public static class CacheFileExtensions
{
    public static string FormCacheFileName(this DistributedFileStoreCacheOptions fileStoreCacheOptions)
    {
        if (fileStoreCacheOptions == null) throw new ArgumentNullException(nameof(fileStoreCacheOptions));
        return $"{fileStoreCacheOptions.FirstPartOfCacheFileName}.{fileStoreCacheOptions.SecondPartOfCacheFileName}.json";
    }

    public static string FormCacheFilePath(this DistributedFileStoreCacheOptions fileStoreCacheOptions)
    {
        if (fileStoreCacheOptions == null) throw new ArgumentNullException(nameof(fileStoreCacheOptions));
        if (fileStoreCacheOptions.PathToCacheFileDirectory == null)
            throw new ArgumentNullException(nameof(fileStoreCacheOptions.PathToCacheFileDirectory));
        return Path.Combine(fileStoreCacheOptions.PathToCacheFileDirectory, fileStoreCacheOptions.FormCacheFileName());
    }

}