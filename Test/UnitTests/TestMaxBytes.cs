// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
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
    private DistributedFileStoreCacheOptions _options;

    public TestMaxBytes(ITestOutputHelper output)
    {
        _output = output;
    }

    private IDistributedFileStoreCacheString SetupCache(int maxBytes, bool jsonEscape = false)
    {
        var services = new ServiceCollection();
        _options = services.AddDistributedFileStoreCache(options =>
        {
            options.WhichVersion = FileStoreCacheVersions.String;
            options.PathToCacheFileDirectory = TestData.GetTestDataDir();
            options.SecondPartOfCacheFileName = GetType().Name;
            options.TurnOffStaticFilePathCheck = true;

            if (jsonEscape)
                options.JsonSerializerForCacheFile =
                    new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

            options.MaxBytesInJsonCacheFile = maxBytes;
        });
        var serviceProvider = services.BuildServiceProvider();

        return serviceProvider.GetRequiredService<IDistributedFileStoreCacheString>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(10)]
    [InlineData(100)]
    public void TestSetMaxBytesByCalculation(int numValues)
    {
        //SETUP
        var tempOptions = new DistributedFileStoreCacheOptions();
        tempOptions.SetMaxBytesByCalculation(numValues, 7, 30);
        var cache = SetupCache(tempOptions.MaxBytesInJsonCacheFile);
        cache.ClearAll();

        //ATTEMPT
        for (int i = 0; i < 100; i++)
        {
            cache.Set($"Test{i:D3}", "123456789012345678901234567890", null);
        }

        //VERIFY
        _output.WriteLine(
            $"Calculated maxBytes = {_options.MaxBytesInJsonCacheFile}, Actual size = {File.ReadAllText(_options.FormCacheFilePath()).Length}");
        cache.GetAllKeyValues().Count.ShouldEqual(numValues);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(10)]
    [InlineData(100)]
    public void TestSetMaxBytesByCalculation_WithTimeout(int numValues)
    {
        //SETUP
        var tempOptions = new DistributedFileStoreCacheOptions();
        tempOptions.SetMaxBytesByCalculation(numValues, 7, 30, 1, 100);
        var cache = SetupCache(tempOptions.MaxBytesInJsonCacheFile);
        cache.ClearAll();

        //ATTEMPT
        for (int i = 0; i < 100; i++)
        {
            cache.Set($"Test{i:D3}", "123456789012345678901234567890", 
                new DistributedCacheEntryOptions{AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1) });
        }

        //VERIFY
        _output.WriteLine(
            $"Calculated maxBytes = {_options.MaxBytesInJsonCacheFile}, Actual size = {File.ReadAllText(_options.FormCacheFilePath()).Length}");
        cache.GetAllKeyValues().Count.ShouldEqual(numValues);
    }


    [Fact]
    public void TestSetMaxBytesByCalculation_Unicode_NoJsonEscape()
    {
        //SETUP
        int numValues = 5;
        var unicode = "בָּרוּךְ אַתָּה ה' אֱ-לֹהֵינוּ, מֶלֶך הָעוֹלָם";
        var tempOptions = new DistributedFileStoreCacheOptions();
        tempOptions.SetMaxBytesByCalculation(numValues, 7, unicode.Length, 6);
        var cache = SetupCache(tempOptions.MaxBytesInJsonCacheFile, false);
        cache.ClearAll();

        //ATTEMPT
        for (int i = 0; i < 100; i++)
        {
            cache.Set($"Test{i:D3}", unicode, null);
        }

        //VERIFY
        _output.WriteLine(
            $"Calculated maxBytes = {_options.MaxBytesInJsonCacheFile}, Actual size = {File.ReadAllText(_options.FormCacheFilePath()).Length}");
        cache.GetAllKeyValues().Count.ShouldEqual(numValues);
    }

    [Fact]
    public void TestSetMaxBytesByCalculation_Unicode_WithJsonEscape()
    {
        //SETUP
        int numValues = 5;
        var unicode = "בָּרוּךְ אַתָּה ה' אֱ-לֹהֵינוּ, מֶלֶך הָעוֹלָם";
        var tempOptions = new DistributedFileStoreCacheOptions();
        tempOptions.SetMaxBytesByCalculation(numValues, 7, unicode.Length, 2);
        var cache = SetupCache(tempOptions.MaxBytesInJsonCacheFile, true);
        cache.ClearAll();

        //ATTEMPT
        for (int i = 0; i < 100; i++)
        {
            cache.Set($"Test{i:D3}", unicode, null);
        }

        //VERIFY
        _output.WriteLine(
            $"Calculated maxBytes = {_options.MaxBytesInJsonCacheFile}, Actual size = {File.ReadAllText(_options.FormCacheFilePath()).Length}");
        cache.GetAllKeyValues().Count.ShouldEqual(numValues);
    }
}