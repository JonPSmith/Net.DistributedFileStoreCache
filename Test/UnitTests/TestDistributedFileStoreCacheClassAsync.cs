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
public class TestDistributedFileStoreCacheClassAsync
{
    private readonly IDistributedFileStoreCacheClass _fsCache;
    private readonly DistributedFileStoreCacheOptions _options;
    private readonly ITestOutputHelper _output;

    public TestDistributedFileStoreCacheClassAsync(ITestOutputHelper output)
    {
        _output = output;

        var services = new ServiceCollection();
        _options = services.AddDistributedFileStoreCache(options =>
        {
            options.WhichVersion = FileStoreCacheVersions.Class;
            options.PathToCacheFileDirectory = TestData.GetTestDataDir();
            options.SecondPartOfCacheFileName = GetType().Name;
            options.TurnOffStaticFilePathCheck = true;
        });
        var serviceProvider = services.BuildServiceProvider();

        _fsCache = serviceProvider.GetRequiredService<IDistributedFileStoreCacheClass>();
    }

    private class JsonClass1
    {
        public int MyInt { get; set; }
        public string MyString { get; set; }
    }


    [Fact]
    public async Task DistributedFileStoreCacheSetClass_SetJsonClass1()
    {
        //SETUP
        _fsCache.ClearAll();

        //ATTEMPT
        await _fsCache.SetClassAsync("test", new JsonClass1{MyInt = 1, MyString = "Hello"});

        //VERIFY
        var allValues = _fsCache.GetAllKeyValues();
        allValues.Count.ShouldEqual(1);
        allValues["test"].ShouldEqual("{\"MyInt\":1,\"MyString\":\"Hello\"}");

        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public async Task DistributedFileStoreCacheSetClass_GetClassFromStringJsonClass1()
    {
        //SETUP
        _fsCache.ClearAll();
        await _fsCache.SetClassAsync("test", new JsonClass1 { MyInt = 1, MyString = "Hello" });

        //ATTEMPT
        var allValuesDict = _fsCache.GetAllKeyValues();
        var jsonClass1 = _fsCache.GetClassFromString<JsonClass1>(allValuesDict ["test"]);

        //VERIFY
        jsonClass1.ShouldBeType<JsonClass1>();
        jsonClass1.ShouldNotBeNull();
        jsonClass1.MyInt.ShouldEqual(1);
        jsonClass1.MyString.ShouldEqual("Hello");
        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public async Task DistributedFileStoreCacheSetClass_GetJsonClass1()
    {
        //SETUP
        _fsCache.ClearAll();
        await _fsCache.SetClassAsync("test", new JsonClass1 { MyInt = 1, MyString = "Hello" });

        //ATTEMPT
        var jsonClass1 = _fsCache.GetClass<JsonClass1>("test");

        //VERIFY
        jsonClass1.ShouldBeType<JsonClass1>();
        jsonClass1.ShouldNotBeNull();
        jsonClass1.MyInt.ShouldEqual(1);
        jsonClass1.MyString.ShouldEqual("Hello");
        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public async Task DistributedFileStoreCacheSetManyClass_JsonClass2()
    {
        //SETUP
        _fsCache.ClearAll();

        //ATTEMPT
        await _fsCache.SetManyClassAsync(new List<KeyValuePair<string, JsonClass1>>
        {
            new("test1", new JsonClass1 { MyInt = 1, MyString = "Hello" }),
            new("test2", new JsonClass1 { MyInt = 2, MyString = "Goodbye" })
        });

        //VERIFY
        var allValues = _fsCache.GetAllKeyValues();
        allValues.Count.ShouldEqual(2);
        allValues["test1"].ShouldEqual("{\"MyInt\":1,\"MyString\":\"Hello\"}");
        allValues["test2"].ShouldEqual("{\"MyInt\":2,\"MyString\":\"Goodbye\"}");

        _options.DisplayCacheFile(_output);
    }
}