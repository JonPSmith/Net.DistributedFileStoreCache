// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Net.DistributedFileStoreCache.SupportCode;

namespace Net.DistributedFileStoreCache;

public static class RegisterDistributedFileStoreCache
{
    public static IServiceCollection AddDistributedFileStoreCache(this IServiceCollection services,
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
        services.AddSingleton(options);
        //This registers the base DistributedFileStoreCacheStringWithExtras service
        services.AddTransient<IDistributedFileStoreCacheStringWithExtras, DistributedFileStoreCacheStringWithExtras>();
        //Selects which interface to register the service to
        if (options.WhichVersion == FileStoreCacheVersions.DistributedCache)
            services.AddSingleton<IDistributedCache, DistributedFileStoreCache>();
        else if (options.WhichVersion == FileStoreCacheVersions.FileStoreCacheByteWithExtras)
            services.AddSingleton<IDistributedFileStoreCacheWithExtras, DistributedFileStoreCache>();

        return services;
    }
}