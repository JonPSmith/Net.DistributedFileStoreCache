// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

namespace Net.DistributedFileStoreCache.SupportCode;


/// <summary>
/// This class contains extension methods using the <see cref="DistributedFileStoreCacheOptions"/>
/// </summary>
public static class CacheFileExtensions
{
    /// <summary>
    /// This returns the cache json filename, including its type
    /// </summary>
    /// <param name="fileStoreCacheOptions"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static string FormCacheFileName(this DistributedFileStoreCacheOptions fileStoreCacheOptions)
    {
        if (fileStoreCacheOptions == null) throw new ArgumentNullException(nameof(fileStoreCacheOptions));
        return $"{fileStoreCacheOptions.FirstPartOfCacheFileName}.{fileStoreCacheOptions.SecondPartOfCacheFileName}.json";
    }

    /// <summary>
    /// This returns the FilePath to the cache json file
    /// </summary>
    /// <param name="fileStoreCacheOptions"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static string FormCacheFilePath(this DistributedFileStoreCacheOptions fileStoreCacheOptions)
    {
        if (fileStoreCacheOptions == null) throw new ArgumentNullException(nameof(fileStoreCacheOptions));
        if (fileStoreCacheOptions.PathToCacheFileDirectory == null)
            throw new ArgumentNullException(nameof(fileStoreCacheOptions.PathToCacheFileDirectory));
        return Path.Combine(fileStoreCacheOptions.PathToCacheFileDirectory, fileStoreCacheOptions.FormCacheFileName());
    }
}