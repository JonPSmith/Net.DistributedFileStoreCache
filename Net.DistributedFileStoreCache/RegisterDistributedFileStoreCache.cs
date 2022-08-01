// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Net.DistributedFileStoreCache.SupportCode;

namespace Net.DistributedFileStoreCache;

public static class RegisterDistributedFileStoreCache
{
    public static DistributedFileStoreCacheOptions AddDistributedFileStoreCache(this IServiceCollection services,
        Action<DistributedFileStoreCacheOptions>? optionsAction = null,
        IHostEnvironment? environment = null)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        var options = new DistributedFileStoreCacheOptions();
        optionsAction?.Invoke(options);
        options.PathToCacheFileDirectory ??= environment?.ContentRootPath;
        options.SecondPartOfCacheFileName ??= environment?.EnvironmentName;

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