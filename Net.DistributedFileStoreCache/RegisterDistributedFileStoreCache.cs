// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Net.DistributedFileStoreCache.SupportCode;

namespace Net.DistributedFileStoreCache;

/// <summary>
/// This class contains the register / setup of the distributed FileStore cache
/// </summary>
public static class RegisterDistributedFileStoreCache
{
    /// <summary>
    /// Use this to register the version of the distributed FileStore cache service you want.
    /// It also ensures the cache json file is setup
    /// </summary>
    /// <param name="services">Needs the <see cref="IServiceCollection"/> to register the selected distributed FileStore cache version</param>
    /// <param name="optionsAction">This allows to set up any of the properties in the <see cref="DistributedFileStoreCacheOptions"/> class</param>
    /// <param name="environment">Optional: If provided it sets the <see cref="DistributedFileStoreCacheOptions.PathToCacheFileDirectory"/> property
    /// from the <see cref="IHostEnvironment.ContentRootPath"/> and the <see cref="DistributedFileStoreCacheOptions.SecondPartOfCacheFileName"/> property
    /// from the <see cref="IHostEnvironment.EnvironmentName"/> property.
    /// </param>
    /// <returns><see cref="DistributedFileStoreCacheOptions"/>, which is useful in unit testing.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="DistributedFileStoreCacheException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static DistributedFileStoreCacheOptions AddDistributedFileStoreCache(this IServiceCollection services,
        Action<DistributedFileStoreCacheOptions>? optionsAction = null,
        IHostEnvironment? environment = null)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        var options = new DistributedFileStoreCacheOptions();
        optionsAction?.Invoke(options);
        options.PathToCacheFileDirectory ??= environment?.ContentRootPath;
        options.SecondPartOfCacheFileName ??= environment?.EnvironmentName;

        options.JsonSerializerForCacheFile ??= options.WhichVersion == FileStoreCacheVersions.Class
            // if the JsonSerializerForCacheFile isn't already set up and the version is Class, then add UnsafeRelaxedJsonEscaping
            ? new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }
            : new JsonSerializerOptions();

        if (options.PathToCacheFileDirectory == null)
            throw new DistributedFileStoreCacheException(
                $"You either need to provide an value for the {nameof(environment)} parameter, " +
                $"or set the options' {nameof(DistributedFileStoreCacheOptions.PathToCacheFileDirectory)} property.");

        if (options.SecondPartOfCacheFileName == null)
            throw new DistributedFileStoreCacheException(
                $"You either need to provide an value for the {nameof(environment)} parameter, " +
                $"or set the options' {nameof(DistributedFileStoreCacheOptions.SecondPartOfCacheFileName)} property.");

        //Set up the static file watcher
        StaticCachePart.SetupStaticCache(options);

        // Add services to the container.
        switch (options.WhichVersion)
        {
            case FileStoreCacheVersions.String:
                services.AddSingleton<IDistributedFileStoreCacheString>(new DistributedFileStoreCacheString(options));
                break;
            case FileStoreCacheVersions.Class:
                services.AddSingleton<IDistributedFileStoreCacheClass>(new DistributedFileStoreCacheClass(options));
                break;
            case FileStoreCacheVersions.Bytes:
                services.AddSingleton<IDistributedFileStoreCacheBytes>(new DistributedFileStoreCacheBytes(new DistributedFileStoreCacheString(options)));
                break;
            case FileStoreCacheVersions.IDistributedCache:
                services.AddSingleton(new DistributedFileStoreCacheBytes(new DistributedFileStoreCacheString(options)) as IDistributedCache);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return options;
    }
}