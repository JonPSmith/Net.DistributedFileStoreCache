// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Net.DistributedFileStoreCache;
using Net.DistributedFileStoreCache.SupportCode;
using Test.TestHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests;

// see https://stackoverflow.com/questions/1408175/execute-unit-tests-serially-rather-than-in-parallel
[Collection("Sequential")]
public class TestDistributedFileStoreCacheString
{
    private readonly IDistributedFileStoreCacheString _distributedCache;
    private readonly DistributedFileStoreCacheOptions _options;
    private readonly ITestOutputHelper _output;

    public TestDistributedFileStoreCacheString(ITestOutputHelper output)
    {
        _output = output;

        var services = new ServiceCollection();
        _options = services.AddDistributedFileStoreCache(options =>
        {
            options.WhichVersion = FileStoreCacheVersions.String;
            options.PathToCacheFileDirectory = TestData.GetTestDataDir();
            options.SecondPartOfCacheFileName = GetType().Name;
            options.TurnOffStaticFilePathCheck = true;
        });
        var serviceProvider = services.BuildServiceProvider();

        _distributedCache = serviceProvider.GetRequiredService<IDistributedFileStoreCacheString>();
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
        _distributedCache.Set("test", "goodbye", null);

        //VERIFY
        var allValues = _distributedCache.GetAllKeyValues();
        allValues.Count.ShouldEqual(1);
        allValues["test"].ShouldEqual("goodbye");

        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public void DistributedFileStoreCacheSet_Unicode()
    {
        //SETUP
        _distributedCache.ClearAll();

        //ATTEMPT
        var unicode = "בָּרוּךְ אַתָּה ה' אֱ-לֹהֵינוּ, מֶלֶך הָעוֹלָם";
        _distributedCache.Set("Unicode", unicode, null);
        _distributedCache.Set("ascii", "my ascii", null);

        //VERIFY
        var allValues = _distributedCache.GetAllKeyValues();
        allValues.Count.ShouldEqual(2);
        allValues["Unicode"].ShouldEqual(unicode);
        allValues["ascii"].ShouldEqual("my ascii");

        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public void DistributedFileStoreCacheSet_AbsoluteExpirationStillValid()
    {
        //SETUP
        _distributedCache.ClearAll();

        //ATTEMPT
        _distributedCache.Set("test-timeout1Sec", "time1", new DistributedCacheEntryOptions{ AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1)});

        //VERIFY
        _distributedCache.Get("test-timeout1Sec").ShouldEqual("time1");
        StaticCachePart.CacheContent.TimeOuts["test-timeout1Sec"].ShouldNotBeNull();

        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public void DistributedFileStoreCacheSet_AbsoluteExpirationExpired()
    {
        //SETUP
        _distributedCache.ClearAll();

        //ATTEMPT
        _distributedCache.Set("test-timeoutExpired", "time1", new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromTicks(1) });

        //VERIFY
        _distributedCache.Get("test-timeoutExpired").ShouldBeNull();
        StaticCachePart.CacheContent.TimeOuts.ContainsKey("test-timeout1Sec").ShouldBeFalse();

        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public void DistributedFileStoreCacheSet_SlidingExpiration()
    {
        //SETUP

        //ATTEMPT
        var ex = Assert.Throws<NotImplementedException>(() => _distributedCache.Set("test-bad", "time1", 
            new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromTicks(1) }));

        //VERIFY
        ex.Message.ShouldEqual("This library doesn't support sliding expirations for performance reasons.");
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
        catch (NullReferenceException)
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
        _distributedCache.Set("test", "first", null);
        _options.DisplayCacheFile(_output);
        _distributedCache.Set("test", "second", null);
        _options.DisplayCacheFile(_output);

        //VERIFY
        _output.WriteLine("------------------------------");
        _output.WriteLine(string.Join(", ", _distributedCache.Get("test").Select(x => (int)x)));
        var value = _distributedCache.Get("test");
        value.ShouldEqual("second");
        var allValues = _distributedCache.GetAllKeyValues();
        allValues.Count.ShouldEqual(1);
    }

    [Fact]
    public void DistributedFileStoreCacheRemove()
    {
        //SETUP
        _distributedCache.ClearAll();
        _distributedCache.Set("XXX", "gone in a minute", null);
        _distributedCache.Set("Still there", "keep this", null);

        //ATTEMPT
        _distributedCache.Remove("XXX");

        //VERIFY
        var allValues = _distributedCache.GetAllKeyValues();
        _distributedCache.Get("XXX").ShouldBeNull();
        _distributedCache.Get("Still there").ShouldEqual("keep this");
        _distributedCache.GetAllKeyValues().Count.ShouldEqual(1);

        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public void DistributedFileStoreCacheSetTwice()
    {
        //SETUP
        _distributedCache.ClearAll();

        //ATTEMPT
        _distributedCache.Set("test1", "first", null);
        _distributedCache.Set("test2", "second", null);

        //VERIFY
        var allValues = _distributedCache.GetAllKeyValues();
        allValues.Count.ShouldEqual(2);
        allValues["test1"].ShouldEqual("first");
        allValues["test2"].ShouldEqual("second");

        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public void DistributedFileStoreCacheHeavyUsage()
    {
        //SETUP
        

        //ATTEMPT
        for (int i = 0; i < 10; i++)
        {
            _distributedCache.ClearAll();
            _distributedCache.Set($"test{i}", i.ToString(), null);
            _distributedCache.Get($"test{i}").ShouldEqual(i.ToString());
        }

        //VERIFY
        var allValues = _distributedCache.GetAllKeyValues();
        allValues.Count.ShouldEqual(1);

        _options.DisplayCacheFile(_output);
    }
}