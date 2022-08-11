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
public class TestDistributedFileStoreCacheClass
{
    private readonly IDistributedFileStoreCacheClass _fsCache;
    private readonly DistributedFileStoreCacheOptions _options;
    private readonly ITestOutputHelper _output;

    public TestDistributedFileStoreCacheClass(ITestOutputHelper output)
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

    private class JsonClass2
    {
        public JsonClass1 MyClass1 { get; set; }
        public int MyInt2 { get; set; }
    }

    [Fact]
    public void DistributedFileStoreCacheSetClass_SetJsonClass1()
    {
        //SETUP
        _fsCache.ClearAll();

        //ATTEMPT
        _fsCache.SetClass("test", new JsonClass1{MyInt = 1, MyString = "Hello"});

        //VERIFY
        var allValues = _fsCache.GetAllKeyValues();
        allValues.Count.ShouldEqual(1);
        allValues["test"].ShouldEqual("{\"MyInt\":1,\"MyString\":\"Hello\"}");

        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public void DistributedFileStoreCacheSetClass_GetClassFromStringJsonClass1()
    {
        //SETUP
        _fsCache.ClearAll();
        _fsCache.SetClass("test", new JsonClass1 { MyInt = 1, MyString = "Hello" });

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
    public void DistributedFileStoreCacheSetClass_GetJsonClass1()
    {
        //SETUP
        _fsCache.ClearAll();
        _fsCache.SetClass("test", new JsonClass1 { MyInt = 1, MyString = "Hello" });

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
    public void DistributedFileStoreCacheSetClass_Bad()
    {
        //SETUP
        _fsCache.ClearAll();
        _fsCache.SetClass("test", new JsonClass1 { MyInt = 1, MyString = "Hello" });

        //ATTEMPT
        var jsonClass2 = _fsCache.GetClass<JsonClass2>("test");

        //VERIFY
        jsonClass2.ShouldBeType<JsonClass2>();
        jsonClass2.ShouldNotBeNull();
        jsonClass2.MyInt2.ShouldEqual(default);
        jsonClass2.MyClass1.ShouldEqual(null);
    }

    [Fact]
    public void DistributedFileStoreCacheSetClass_JsonClass2()
    {
        //SETUP
        _fsCache.ClearAll();

        //ATTEMPT
        _fsCache.SetClass("test", new JsonClass2 { MyInt2 = 3, MyClass1 = new JsonClass1{ MyInt = 1, MyString = "Hello" } });

        //VERIFY
        var allValues = _fsCache.GetAllKeyValues();
        allValues.Count.ShouldEqual(1);
        allValues["test"].ShouldEqual("{\"MyClass1\":{\"MyInt\":1,\"MyString\":\"Hello\"},\"MyInt2\":3}");


        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public void DistributedFileStoreCacheSetClass_Unicode()
    {
        //SETUP
        _fsCache.ClearAll();
        var unicode = "בָּרוּךְ אַתָּה ה' אֱ-לֹהֵינוּ, מֶלֶך הָעוֹלָם";

        //ATTEMPT
        _fsCache.SetClass("test", new JsonClass1 { MyInt = 1, MyString = unicode });

        //VERIFY
        var allValues = _fsCache.GetAllKeyValues();
        allValues.Count.ShouldEqual(1);
        allValues["test"].ShouldEqual("{\"MyInt\":1,\"MyString\":\"" + unicode + "\"}");


        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public void DistributedFileStoreCacheSetClass_JsonClass_Example()
    {
        //SETUP
        _fsCache.ClearAll();

        //ATTEMPT
        _fsCache.SetClass("test", new JsonClass2 { MyInt2 = 3, 
            MyClass1 = new JsonClass1 { MyInt = 1, MyString = "Hello" } });

        //VERIFY
        _fsCache.Get("test").ShouldEqual(
            "{\"MyClass1\":{\"MyInt\":1,\"MyString\":\"Hello\"},\"MyInt2\":3}");
        var jsonClass2 = _fsCache.GetClass<JsonClass2>("test");
        jsonClass2.ShouldBeType<JsonClass2>();
        jsonClass2.ShouldNotBeNull();
        jsonClass2.MyInt2.ShouldEqual(3);
        jsonClass2.MyClass1.ShouldNotBeNull();
        jsonClass2.MyClass1.MyInt.ShouldEqual(1);
        jsonClass2.MyClass1.MyString.ShouldEqual("Hello");

        _options.DisplayCacheFile(_output);
        //Example if no UnsafeRelaxedJsonEscaping
        //"{\u0022MyClass1\u0022:{\u0022MyInt\u0022:1,\u0022MyString\u0022:\u0022Hello\u0022},\u0022MyInt\u0022:3}"}
    }
}