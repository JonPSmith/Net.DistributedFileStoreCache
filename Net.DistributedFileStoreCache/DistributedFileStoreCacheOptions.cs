// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;

namespace Net.DistributedFileStoreCache;

/// <summary>
/// This provides the members to select which version of the FileStore cache class / interface you want registers as a service
/// </summary>
public enum FileStoreCacheVersions
{
    /// <summary>
    /// Use this to register the <see cref="DistributedFileStoreCacheString"/> against the <see cref="IDistributedFileStoreCacheString"/> interface
    /// </summary>
    String,
    /// <summary>
    /// Use this to register the <see cref="DistributedFileStoreCacheClass"/> against the <see cref="IDistributedFileStoreCacheClass"/> interface
    /// </summary>
    Class,
    /// <summary>
    /// Use this to register the <see cref="DistributedFileStoreCacheBytes"/> against the <see cref="IDistributedFileStoreCacheBytes"/> interface
    /// </summary>
    Bytes,
    /// <summary>
    /// Use this to register the <see cref="DistributedFileStoreCacheBytes"/> against the <see cref="Microsoft.Extensions.Caching.Distributed.IDistributedCache"/> interface
    /// </summary>
    // ReSharper disable once InconsistentNaming
    IDistributedCache
}

/// <summary>
/// This contains all the options used to register / setup the FileStore cache
/// </summary>
public class DistributedFileStoreCacheOptions
{
    /// <summary>
    /// This defines which version of the <see cref="DistributedFileStoreCacheBytes"/> services are registered
    /// 1. Default is <see cref="IDistributedFileStoreCacheString"/>, where the value is of type string, plus two extra features
    /// 2. If set to <see cref="IDistributedFileStoreCacheBytes"/>, where the value is of type byte[], plus two extra features
    /// 3. If set to <see cref="FileStoreCacheVersions.IDistributedCache"/>, which implements the <see cref="IDistributedCache"/> interface
    /// </summary>
    public FileStoreCacheVersions WhichVersion { get; set; }

    /// <summary>
    /// This defines the maximum bytes that can be in the cache json file.
    /// If you exceed this, then you will have an exception, so I recommend you think .
    /// </summary>
    public int MaxBytesInJsonCacheFile { get; set; } = 10_000;

    /// <summary>
    /// If you want to set the maximum bytes that the cache can hold then you can use this calculation
    /// </summary>
    /// <param name="maxEntries">The maximum number of cache entries you want to add to the cache</param>
    /// <param name="maxKeyLength">The maximum size of the key string</param>
    /// <param name="maxValueLength">The maximum size of the value string</param>
    /// <param name="charSize">Optional:
    /// If ascii = 1, if byte or unicode and not using UnsafeRelaxedJsonEscaping then 6, if unicode with UnsafeRelaxedJsonEscaping then 2 </param>
    /// <param name="percentWithTimeout">Optional: the percent (between 0 and 100) of the entries will have a timeout</param>
    public void SetMaxBytesByCalculation(int maxEntries, int maxKeyLength, int maxValueLength, int charSize = 1, double percentWithTimeout = 0)
    {
        maxValueLength *= charSize ;
        MaxBytesInJsonCacheFile = maxEntries * (maxKeyLength + maxValueLength + 6) + 15 + 14;
        if (percentWithTimeout > 0)
            MaxBytesInJsonCacheFile += (int)(maxEntries * percentWithTimeout / 100.0 *
                                             (maxKeyLength + DateTime.MaxValue.Ticks.ToString().Length + 6));
    }

    /// <summary>
    /// This allows you to replace the System.Text.Json default serialization options. Here are some reasons you might want to so this: 
    /// 1. If you are using Unicode characters in the <see cref="FileStoreCacheVersions.String"/> version or the
    /// <see cref="FileStoreCacheVersions.Class"/> then setting this parameter to a
    /// <see cref="JsonSerializerOptions"/> containing { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping },
    /// then the json smaller (and easier to read).
    /// 2. The default serialization creates one long line, which is efficient on space but hard to read.
    /// If you set this parameter to a <see cref="JsonSerializerOptions"/> containing { WriteIndented = true },
    /// then it takes up LOT more space but its easier to read.
    /// </summary>
    public JsonSerializerOptions JsonSerializerForCacheFile { get; set; } = new JsonSerializerOptions();

    /// <summary>
    /// This provides the path to the directory containing the cache file name
    /// If null, this will be set to the <see cref="IHostEnvironment"/>.<see cref="IHostEnvironment.ContentRootPath"/>.
    /// But you can set your own filepath by setting this parameter
    /// </summary>
    public string? PathToCacheFileDirectory { get; set; }

    /// <summary>
    /// This provides a suffix to the cache file name <see cref="FirstPartOfCacheFileName"/>
    /// - useful to stop development file effect the production
    /// If null, this will be set to the <see cref="IHostEnvironment"/>.<see cref="IHostEnvironment.EnvironmentName"/>.
    /// But you can replace the name from the environment settings
    /// </summary>
    public string? SecondPartOfCacheFileName { get; set; }

    /// <summary>
    /// This holds the first part of the distributed cache file used by the <see cref="DistributedFileStoreCacheBytes"/>.
    /// Note that it shouldn't have the file type (e.g. ".json") on the name
    /// </summary>
    public string FirstPartOfCacheFileName { get; set; } = "FileStoreCacheFile";

    /// <summary>
    /// This sets the delay between a retry after a <see cref="UnauthorizedAccessException"/> is throw
    /// NOTE: Keep it small 
    /// </summary>
    public int DelayMillisecondsOnUnauthorizedAccess { get; set; } = 5;

    /// <summary>
    /// This sets the number of retries after a <see cref="UnauthorizedAccessException"/> is throw
    /// </summary>
    public int NumTriesOnUnauthorizedAccess { get; set; } = 20;

    /// <summary>
    /// By default this will check that you are trying to register more then one <see cref="DistributedFileStoreCacheBytes"/>.
    /// You need to set this to true if you are running unit tests with different cache file names (and run tests serially)
    /// </summary>
    public bool TurnOffStaticFilePathCheck { get; set; } = false;
}