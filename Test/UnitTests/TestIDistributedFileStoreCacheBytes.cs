// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Text.Encodings.Web;
using System.Text.Json;
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
public class TestIDistributedFileStoreCacheBytes
{
    private readonly IDistributedFileStoreCacheBytes _distributedCache;
    private readonly DistributedFileStoreCacheOptions _options;
    private readonly ITestOutputHelper _output;

    public TestIDistributedFileStoreCacheBytes(ITestOutputHelper output)
    {
        _output = output;

        var services = new ServiceCollection();
        _options = services.AddDistributedFileStoreCache(options =>
        {
            options.WhichVersion = FileStoreCacheVersions.Bytes;
            options.PathToCacheFileDirectory = TestData.GetTestDataDir();
            options.SecondPartOfCacheFileName = GetType().Name;
            options.JsonSerializerForCacheFile = new JsonSerializerOptions
                { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            options.TurnOffStaticFilePathCheck = true;
        });
        var serviceProvider = services.BuildServiceProvider();

        _distributedCache = serviceProvider.GetRequiredService<IDistributedFileStoreCacheBytes>();
    }

    [Fact]
    public void DistributedFileStoreCacheEmpty()
    {
        //SETUP
        _distributedCache.ClearAll();

        //ATTEMPT
        var value = _distributedCache.Get("test");

        //VERIFY
        value.ShouldBeNull();
        _distributedCache.GetAllKeyValues().Count.ShouldEqual(0);

        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public void DistributedFileStoreCacheSet()
    {
        //SETUP
        _distributedCache.ClearAll();

        //ATTEMPT
        _distributedCache.Set("test", new byte[] { 1, 2, 3 }, null);

        //VERIFY
        var allValues = _distributedCache.GetAllKeyValues();
        allValues.Count.ShouldEqual(1);
        allValues["test"].ShouldEqual(new byte[] { 1, 2, 3 });

        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public async Task DistributedFileStoreCacheSetAsync()
    {
        //SETUP
        _distributedCache.ClearAll();

        //ATTEMPT
        await _distributedCache.SetAsync("test", new byte[] { 1, 2, 3 }, null);

        //VERIFY
        var allValues = _distributedCache.GetAllKeyValues();
        allValues.Count.ShouldEqual(1);
        allValues["test"].ShouldEqual(new byte[] { 1, 2, 3 });

        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public void DistributedFileStoreCacheSetNullBad()
    {
        //SETUP
        _distributedCache.ClearAll();

        //ATTEMPT
        try
        {
            _distributedCache.Set("test", null, null);
        }
        catch (ArgumentNullException)
        {
            return;
        }

        //VERIFY
        Assert.True(false, "should have throw exception");
    }

    [Fact]
    public void DistributedFileStoreCacheWithSetChange()
    {
        //SETUP
        _distributedCache.ClearAll();

        //ATTEMPT
        _distributedCache.Set("test", new byte[] { 7, 8, 9 }, null);
        _distributedCache.Set("test", new byte[] { 9, 8, 7 }, null);

        //VERIFY
        var allValues = _distributedCache.GetAllKeyValues();
        allValues.Count.ShouldEqual(1);
        allValues["test"].ShouldEqual(new byte[] { 9, 8, 7 });

        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public void DistributedFileStoreCacheRemove()
    {
        //SETUP
        _distributedCache.ClearAll();
        _distributedCache.Set("test", new byte[] { 11, 12, 13 }, null);

        //ATTEMPT
        _distributedCache.Remove("test");

        //VERIFY
        _distributedCache.GetAllKeyValues().Count.ShouldEqual(0);

        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public async Task DistributedFileStoreCacheRemoveAsync()
    {
        //SETUP
        _distributedCache.ClearAll();
        await _distributedCache.SetAsync("test", new byte[] { 11, 12, 13 }, null);

        //ATTEMPT
        await _distributedCache.RemoveAsync("test");

        //VERIFY
        _distributedCache.GetAllKeyValues().Count.ShouldEqual(0);

        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public void DistributedFileStoreCacheSetTwice()
    {
        //SETUP
        _distributedCache.ClearAll();

        //ATTEMPT
        _distributedCache.Set("test1", new byte[] { 1, 2, 3 }, null);
        _distributedCache.Set("test2", new byte[] { 4, 5, 6 }, null);

        //VERIFY
        var allValues = _distributedCache.GetAllKeyValues();
        allValues.Count.ShouldEqual(2);
        allValues["test1"].ShouldEqual(new byte[] { 1, 2, 3 });
        allValues["test2"].ShouldEqual(new byte[] { 4, 5, 6 });

        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public void DistributedFileStoreCacheSet_Refresh()
    {
        //SETUP

        //ATTEMPT
        var ex = Assert.Throws<NotImplementedException>(() => _distributedCache.Refresh("test"));

        //VERIFY
        ex.Message.ShouldEqual("This library doesn't support sliding expirations for performance reasons.");
    }

    [Fact]
    public async Task DistributedFileStoreCacheSet_RefreshAsync()
    {
        //SETUP

        //ATTEMPT
        var ex = await  Assert.ThrowsAsync<NotImplementedException>( async () => await _distributedCache.RefreshAsync("test"));

        //VERIFY
        ex.Message.ShouldEqual("This library doesn't support sliding expirations for performance reasons.");
    }



}