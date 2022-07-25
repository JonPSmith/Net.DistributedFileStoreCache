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
        IHostEnvironment environment,
        Action<DistributedFileStoreCacheOptions>? optionsAction = null)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        var options = new DistributedFileStoreCacheOptions();
        optionsAction?.Invoke(options);
        options.SecondPartOfCacheFileName ??= environment.EnvironmentName;
        options.PathToCacheFileDirectory ??= environment.ContentRootPath;

        //Set up the static file watcher
        StaticCachePart.SetupStaticCache(options);

        // Add services to the container.
        services.AddSingleton(options);
        //This registers the base DistributedFileStoreCacheStringWithExtras service
        services.AddTransient<IDistributedFileStoreCacheStringWithExtras, DistributedFileStoreCacheStringWithExtras>();
        //Selects which interface to register the service to
        if (options.WhichInterface == DistributedFileStoreCacheInterfaces.DistributedCache)
            services.AddSingleton<IDistributedCache, DistributedFileStoreCache>();
        else if (options.WhichInterface == DistributedFileStoreCacheInterfaces.DistributedFileStoreWithExtras)
            services.AddSingleton<IDistributedFileStoreCacheWithExtras, DistributedFileStoreCache>();

        return services;
    }
}