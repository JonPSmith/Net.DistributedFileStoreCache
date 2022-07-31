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

public class TestDistributedFileStoreCacheClass
{
    private readonly IDistributedFileStoreCacheClass _distributedCache;
    private readonly DistributedFileStoreCacheOptions _options;
    private readonly ITestOutputHelper _output;

    public TestDistributedFileStoreCacheClass(ITestOutputHelper output)
    {
        _output = output;

        var services = new ServiceCollection();
        _options = services.AddDistributedFileStoreCache(options =>
        {
            options.WhichVersion = FileStoreCacheVersions.FileStoreCacheClasses;
            options.PathToCacheFileDirectory = TestData.GetTestDataDir();
            options.SecondPartOfCacheFileName = GetType().Name;
            options.TurnOffStaticFilePathCheck = true;
            options.JsonSerializerForCacheFile = new JsonSerializerOptions
                { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        });
        var serviceProvider = services.BuildServiceProvider();

        _distributedCache = serviceProvider.GetRequiredService<IDistributedFileStoreCacheClass>();
    }

    private class JsonClass1
    {
        public int MyInt { get; set; }
        public string MyString { get; set; }
    }

    private class JsonClass2
    {
        public JsonClass1 MyClass1 { get; set; }
        public int MyInt { get; set; }
    }

    [Fact]
    public void DistributedFileStoreCacheSetClass_JsonClass1()
    {
        //SETUP
        _distributedCache.ClearAll();

        //ATTEMPT
        _distributedCache.SetClass("test", new JsonClass1{MyInt = 1, MyString = "Hello"}, null);

        //VERIFY
        var allValues = _distributedCache.GetAllKeyValues();
        allValues.Count.ShouldEqual(1);
        allValues["test"].ShouldEqual("{\"MyInt\":1,\"MyString\":\"Hello\"}");

        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public void DistributedFileStoreCacheSetClass_JsonClass2()
    {
        //SETUP
        _distributedCache.ClearAll();

        //ATTEMPT
        _distributedCache.SetClass("test", new JsonClass2 { MyInt = 3, MyClass1 = new JsonClass1{ MyInt = 1, MyString = "Hello" } }, null);

        //VERIFY
        var allValues = _distributedCache.GetAllKeyValues();
        allValues.Count.ShouldEqual(1);
        allValues["test"].ShouldEqual("{\"MyClass1\":{\"MyInt\":1,\"MyString\":\"Hello\"},\"MyInt\":3}");


        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public void DistributedFileStoreCacheSetClass_JsonClass_Example()
    {
        //SETUP
        _distributedCache.ClearAll();

        //ATTEMPT
        _distributedCache.SetClass("test", new JsonClass2 { MyInt = 3, 
            MyClass1 = new JsonClass1 { MyInt = 1, MyString = "Hello" } }, null);

        //VERIFY
        _distributedCache.Get("test").ShouldEqual(
            "{\"MyClass1\":{\"MyInt\":1,\"MyString\":\"Hello\"},\"MyInt\":3}");
        var jsonClass2 = _distributedCache.GetClass<JsonClass2>("test");
        jsonClass2.ShouldBeType<JsonClass2>();
        jsonClass2.ShouldNotBeNull();
        jsonClass2.MyInt.ShouldEqual(3);
        jsonClass2.MyClass1.ShouldNotBeNull();
        jsonClass2.MyClass1.MyInt.ShouldEqual(1);
        jsonClass2.MyClass1.MyString.ShouldEqual("Hello");

        _options.DisplayCacheFile(_output);
    }
}