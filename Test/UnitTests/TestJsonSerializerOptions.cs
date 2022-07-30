// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Text.Encodings.Web;
using System.Text.Json;
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
public class TestJsonSerializerOptions
{
    private readonly ITestOutputHelper _output;
    private DistributedFileStoreCacheOptions _options;

    public TestJsonSerializerOptions(ITestOutputHelper output)
    {
        _output = output;
    }

    private IDistributedFileStoreCacheStringWithExtras SetupCache(JsonSerializerOptions jsonOptions)
    {
        var services = new ServiceCollection();
        services.AddDistributedFileStoreCache(options =>
        {
            options.WhichVersion = FileStoreCacheVersions.FileStoreCacheStrings;
            options.PathToCacheFileDirectory = TestData.GetTestDataDir();
            options.SecondPartOfCacheFileName = GetType().Name;
            options.TurnOffStaticFilePathCheck = true;

            options.JsonSerializerForCacheFile = jsonOptions;
        });
        var serviceProvider = services.BuildServiceProvider();

        _options = serviceProvider.GetService<DistributedFileStoreCacheOptions>()!;
        return serviceProvider.GetRequiredService<IDistributedFileStoreCacheStringWithExtras>();
    }

    [Fact]
    public void TestDefaultJsonSerializer()
    {
        //SETUP
        var cache = SetupCache(new JsonSerializerOptions());
        cache.ClearAll();

        //ATTEMPT
        cache.Set("Test", "Hello today!", null);

        //VERIFY
        var fileContent = File.ReadAllText(_options.FormCacheFilePath());
        fileContent.ShouldEqual(@"{""Cache"":{""Test"":""Hello today!""},""TimeOuts"":{}}");
    }

    [Fact]
    public void TestJsonSerializerWriteIndented()
    {
        //SETUP
        var cache = SetupCache(new JsonSerializerOptions { WriteIndented = true});
        cache.ClearAll();

        //ATTEMPT
        cache.Set("Test", "Hello today!", null);

        //VERIFY
        var fileContent = File.ReadAllText(_options.FormCacheFilePath());
        fileContent.ShouldEqual(@"{
  ""Cache"": {
    ""Test"": ""Hello today!""
  },
  ""TimeOuts"": {}
}");
    }

    [Fact]
    public void TestDefaultJsonSerializer_JsonInJson()
    {
        //SETUP
        var cache = SetupCache(new JsonSerializerOptions());
        cache.ClearAll();

        var value = JsonSerializer.Serialize(new Dictionary<int, string>
        {
            {1, "One"}, {2,"Two"}
        });

        //ATTEMPT
        cache.Set("Json", value, null);

        //VERIFY
        var fileContent = File.ReadAllText(_options.FormCacheFilePath());
        fileContent.ShouldEqual(@"{""Cache"":{""Json"":""{\u00221\u0022:\u0022One\u0022,\u00222\u0022:\u0022Two\u0022}""},""TimeOuts"":{}}");
    }

    [Fact]
    public void TestJsonSerializerUnsafeRelaxedJsonEscaping_JsonInJson()
    {
        //SETUP
        var cache = SetupCache(new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        cache.ClearAll();

        var value = JsonSerializer.Serialize(new Dictionary<int, string>
        {
            {1, "One"}, {2,"Two"}
        });

        //ATTEMPT
        cache.Set("Json", value, null);

        //VERIFY
        var fileContent = File.ReadAllText(_options.FormCacheFilePath());
        fileContent.ShouldEqual(@"{""Cache"":{""Json"":""{\""1\"":\""One\"",\""2\"":\""Two\""}""},""TimeOuts"":{}}");
    }

    //This test shows that UnsafeRelaxedJsonEscaping doesn't do anything different to normal
    [Fact]
    public void TestJsonSerializerUnsafeRelaxedJsonEscaping_ASCII()
    {
        //SETUP
        var cache = SetupCache(new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        cache.ClearAll();

        //ATTEMPT
        cache.Set("Test", "Hello today!", null);

        //VERIFY
        var fileContent = File.ReadAllText(_options.FormCacheFilePath());
        fileContent.ShouldEqual(@"{""Cache"":{""Test"":""Hello today!""},""TimeOuts"":{}}");
    }

    //This test shows that UnsafeRelaxedJsonEscaping doesn't do anything different to normal
    [Fact]
    public void TestJsonSerializerUnsafeRelaxedJsonEscaping_Bytes()
    {
        //SETUP
        var cache = SetupCache(new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        cache.ClearAll();

        var value = new string(new[] { (char)1, (char)2, (char)3 });

        //ATTEMPT
        cache.Set("Test", value, null);

        //VERIFY
        var fileContent = File.ReadAllText(_options.FormCacheFilePath());
        fileContent.ShouldEqual(@"{""Cache"":{""Test"":""\u0001\u0002\u0003""},""TimeOuts"":{}}");
    }

    //This test shows that UnsafeRelaxedJsonEscaping doesn't do anything different to normal
    [Fact]
    public void TestJsonSerializerUnsafeRelaxedJsonEscaping_Unicode()
    {
        //SETUP
        var cache = SetupCache(new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        cache.ClearAll();

        var value = "בָּרוּךְ אַתָּה ה' אֱ-לֹהֵינוּ, מֶלֶך הָעוֹלָם";

        //ATTEMPT
        cache.Set("Test", value, null);

        //VERIFY
        var fileContent = File.ReadAllText(_options.FormCacheFilePath());
        fileContent.ShouldEqual(@"{""Cache"":{""Test"":""בָּרוּךְ אַתָּה ה' אֱ-לֹהֵינוּ, מֶלֶך הָעוֹלָם""},""TimeOuts"":{}}");
    }
}