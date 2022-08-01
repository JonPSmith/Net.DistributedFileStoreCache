// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Net.DistributedFileStoreCache;
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

    private IDistributedFileStoreCacheString SetupCache(int maxBytes)
    {
        var services = new ServiceCollection();
        services.AddDistributedFileStoreCache(options =>
        {
            options.WhichVersion = FileStoreCacheVersions.String;
            options.PathToCacheFileDirectory = TestData.GetTestDataDir();
            options.SecondPartOfCacheFileName = GetType().Name;
            options.TurnOffStaticFilePathCheck = true;

            options.MaxBytesInJsonCacheFile = maxBytes;
        });
        var serviceProvider = services.BuildServiceProvider();

        return serviceProvider.GetRequiredService<IDistributedFileStoreCacheString>();
    }

    [Theory]
    [InlineData(100, 1)]
    [InlineData(200, 2)]
    public void TestFailsOnMaxBytes(int maxBytes, int numValues)
    {
        //SETUP
        var cache = SetupCache(maxBytes);
        cache.ClearAll();

        //ATTEMPT
        cache.Set("Test1", "123456789012345678901234567890", null);
        cache.Set("Test2", "123456789012345678901234567890", null);

        //VERIFY
        cache.GetAllKeyValues().Count.ShouldEqual(numValues);
    }
}