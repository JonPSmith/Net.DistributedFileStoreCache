// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

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
public class TestDistributedFileStoreCacheStringWithExtras_Async 
{
    private readonly IDistributedFileStoreCacheStringWithExtras _distributedCache;
    private readonly DistributedFileStoreCacheOptions _options;
    private readonly ITestOutputHelper _output;

    public TestDistributedFileStoreCacheStringWithExtras_Async(ITestOutputHelper output)
    {
        _output = output;

        var services = new ServiceCollection();
        services.AddDistributedFileStoreCache(options =>
        {
            options.WhichInterface = DistributedFileStoreCacheInterfaces.DistributedFileStoreStringWithExtras;
            options.PathToCacheFileDirectory = TestData.GetTestDataDir();
            options.SecondPartOfCacheFileName = GetType().Name;
            options.TurnOffStaticFilePathCheck = true;
        });
        var serviceProvider = services.BuildServiceProvider();

        _options = serviceProvider.GetRequiredService<DistributedFileStoreCacheOptions>();
        _distributedCache = serviceProvider.GetRequiredService<IDistributedFileStoreCacheStringWithExtras>();
    }

    [Fact]
    public async Task DistributedFileStoreCacheSetAsync()
    {
        //SETUP
        _distributedCache.ClearAll();

        //ATTEMPT
        await _distributedCache.SetAsync("test", "hello async", null);

        //VERIFY
        var value = await _distributedCache.GetAsync("test");
        value.ShouldEqual("hello async");

        var allValues = await _distributedCache.GetAllKeyValuesAsync();
        allValues.Count.ShouldEqual(1);
        allValues["test"].ShouldEqual("hello async");

        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public async Task DistributedFileStoreCacheSetNullBad()
    {
        //SETUP
        _distributedCache.ClearAll();

        //ATTEMPT
        try
        {
            await _distributedCache.SetAsync("test", null, null);
        }
        catch (NullReferenceException)
        {
            return;
        }

        //VERIFY
        Assert.True(false, "should have throw exception");
    }

    [Fact]
    public async Task DistributedFileStoreCacheWithSetChangeAsync()
    {
        //SETUP
        _distributedCache.ClearAll();

        //ATTEMPT
        await _distributedCache.SetAsync("test", "first", null);
        _options.DisplayCacheFile(_output);
        await _distributedCache.SetAsync("test", "second", null);
        _options.DisplayCacheFile(_output);

        //VERIFY
        var value = await _distributedCache.GetAsync("test");
        value.ShouldEqual("second");
        var allValues = await _distributedCache.GetAllKeyValuesAsync();
        allValues.Count.ShouldEqual(1);
    }

    [Fact]
    public async Task DistributedFileStoreCacheRemoveAsync()
    {
        //SETUP
        _distributedCache.ClearAll();
        await _distributedCache.SetAsync("YYY", "another to go", null);
        await _distributedCache.SetAsync("Still there", "keep this", null);

        //ATTEMPT
        await _distributedCache.RemoveAsync("YYY");

        //VERIFY
        (await _distributedCache.GetAsync("YYY")).ShouldBeNull();
        (await _distributedCache.GetAllKeyValuesAsync()).Count.ShouldEqual(1);

        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public async Task DistributedFileStoreCacheSetTwice()
    {
        //SETUP
        _distributedCache.ClearAll();

        //ATTEMPT
        await _distributedCache.SetAsync("test1", "first", null);
        await _distributedCache.SetAsync("test2", "second", null);

        //VERIFY
        var allValues = await _distributedCache.GetAllKeyValuesAsync();
        allValues.Count.ShouldEqual(2);
        allValues["test1"].ShouldEqual("first");
        allValues["test2"].ShouldEqual("second");

        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public async Task DistributedFileStoreCacheHeavyUsage()
    {
        //SETUP
        

        //ATTEMPT
        for (int i = 0; i < 10; i++)
        {
            _distributedCache.ClearAll();
            await _distributedCache.SetAsync($"test{i}", i.ToString(), null);
            _distributedCache.Get($"test{i}").ShouldEqual(i.ToString());
        }


        //VERIFY
        var allValues = await _distributedCache.GetAllKeyValuesAsync();
        allValues.Count.ShouldEqual(1);

        _options.DisplayCacheFile(_output);
    }
}