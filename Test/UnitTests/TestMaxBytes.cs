// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Net.DistributedFileStoreCache;
using Net.DistributedFileStoreCache.SupportCode;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests;

// see https://stackoverflow.com/questions/1408175/execute-unit-tests-serially-rather-than-in-parallel
[Collection("Sequential")]
public class TestMaxBytes
{
    private readonly ITestOutputHelper _output;

    public TestMaxBytes(ITestOutputHelper output)
    {
        _output = output;
    }

    private IDistributedFileStoreCacheStringWithExtras SetupCache(int maxBytes)
    {
        var services = new ServiceCollection();
        services.AddDistributedFileStoreCache(options =>
        {
            options.WhichInterface = DistributedFileStoreCacheInterfaces.DistributedFileStoreStringWithExtras;
            options.PathToCacheFileDirectory = TestData.GetTestDataDir();
            options.SecondPartOfCacheFileName = GetType().Name;
            options.TurnOffStaticFilePathCheck = true;

            options.MaxBytesInJsonCacheFile = maxBytes;
        });
        var serviceProvider = services.BuildServiceProvider();

        return serviceProvider.GetRequiredService<IDistributedFileStoreCacheStringWithExtras>();
    }

    [Fact]
    public void TestFailsOnMaxBytes()
    {
        //SETUP
        var cache = SetupCache(20);
        cache.ClearAll();

        //ATTEMPT
        cache.Set("Test", "123456789012345678901234567890", null);
        var ex = Assert.Throws<DistributedFileStoreCacheException>(
            () => cache.Set("Test", "1111111111111111111111111111111", null));

        //VERIFY
        ex.Message.ShouldStartWith("Your cache json file has more that 20 bytes");
    }
}