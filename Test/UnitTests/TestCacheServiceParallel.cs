// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Net.DistributedFileStoreCache;
using Test.TestHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests;

// see https://stackoverflow.com/questions/1408175/execute-unit-tests-serially-rather-than-in-parallel
[Collection("Sequential")]
public class TestCacheServiceParallel
{
    private readonly ITestOutputHelper _output;
    private DistributedFileStoreCacheOptions _options;

    /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
    public TestCacheServiceParallel(ITestOutputHelper output)
    {
        _output = output;
    }

    private IDistributedFileStoreCacheStringWithExtras SetupDistributedFileStoreCache()
    {
        var services = new ServiceCollection();
        var environment = new StubEnvironment(GetType().Name, TestData.GetTestDataDir());
        services.AddDistributedFileStoreCache(environment, options =>
        {
            options.WhichInterface = DistributedFileStoreCacheInterfaces.DistributedFileStoreStringWithExtras;
            options.TurnOffStaticFilePathCheck = true;
        });
        var serviceProvider = services.BuildServiceProvider();
        _options = serviceProvider.GetRequiredService<DistributedFileStoreCacheOptions>();
        return serviceProvider.GetRequiredService<IDistributedFileStoreCacheStringWithExtras>();
    }

    [Fact]
    public void TestRunTwoServices()
    {
        //SETUP
        var cache1 = SetupDistributedFileStoreCache();
        var cache2 = SetupDistributedFileStoreCache();
        cache1.ClearAll();

        //ATTEMPT
        cache1.Set("Cache1", DateTime.UtcNow.ToString("O"), null);
        cache2.Set("Cache2", DateTime.UtcNow.ToString("O"), null);

        //VERIFY
        cache1.GetAllKeyValues().Keys.ShouldEqual(new []{ "Cache1", "Cache2" });
    }

    [Fact]
    public void TestUpdateInParallelWithDelays()
    {
        //SETUP
        SetupDistributedFileStoreCache().ClearAll();
        var startDate = DateTime.Now;

        //ATTEMPT
        Parallel.ForEach(Enumerable.Range(1, 5),
            currentElement =>
            {
                Task.Delay(10 * currentElement);
                var distributedCache = SetupDistributedFileStoreCache();
                distributedCache.Set($"Key{currentElement}",
                    $"Diff = {DateTime.Now.Subtract(startDate).TotalMilliseconds:F3} ms", null);
            });

        //VERIFY
        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public void TestUpdateInParallel()
    {
        //SETUP
        SetupDistributedFileStoreCache().ClearAll();
        var startDate = DateTime.Now;

        //ATTEMPT
        Parallel.ForEach(Enumerable.Range(1, 5),
            currentElement =>
            {
                var distributedCache = SetupDistributedFileStoreCache();
                distributedCache.Set($"Key{currentElement}",
                    $"Diff = {DateTime.Now.Subtract(startDate).TotalMilliseconds:F3} ms", null);
            });


        //VERIFY
        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public async Task TestUpdateInParallelWithDelaysAsync()
    {
        //SETUP
        SetupDistributedFileStoreCache().ClearAll();
        var startDate = DateTime.Now;

        async Task TaskAsync(int num)
        {
            await Task.Delay(10 * num);
            var distributedCache = SetupDistributedFileStoreCache();
            await distributedCache.SetAsync($"Key{num}",
                $"Diff = {DateTime.Now.Subtract(startDate).TotalMilliseconds:F3} ms", null);
        }

        //ATTEMPT
        await 5.NumTimesAsyncEnumerable().AsyncParallelForEach(TaskAsync);

        //VERIFY
        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public async Task TestUpdateInParallelAsync()
    {
        //SETUP
        SetupDistributedFileStoreCache().ClearAll();
        var startDate = DateTime.Now;

        async Task TaskAsync(int num)
        {
            var distributedCache = SetupDistributedFileStoreCache();
            await distributedCache.SetAsync($"Key{num}",
                $"Diff = {DateTime.Now.Subtract(startDate).TotalMilliseconds:F3} ms", null);
        }

        //ATTEMPT
        await 5.NumTimesAsyncEnumerable().AsyncParallelForEach(TaskAsync);

        //VERIFY
        _options.DisplayCacheFile(_output);
    }
}