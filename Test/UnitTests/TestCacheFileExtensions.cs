// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Net.DistributedFileStoreCache;
using Net.DistributedFileStoreCache.SupportCode;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests;

public class TestCacheFileExtensions
{
    private readonly ITestOutputHelper _output;

    public TestCacheFileExtensions(ITestOutputHelper output)
    {
        _output = output;
    }


    [Fact]
    public void TestFormCacheFileName()
    {
        //SETUP
        var options = new DistributedFileStoreCacheOptions
        {
            FirstPartOfCacheFileName = "Test",
            SecondPartOfCacheFileName = "Type"
        };

        //ATTEMPT
        var fileName = options.FormCacheFileName();

        //VERIFY
        fileName.ShouldEqual("Test.Type.json");
    }

    [Fact]
    public void TestFormCacheFilePath()
    {
        //SETUP
        var options = new DistributedFileStoreCacheOptions
        {
            FirstPartOfCacheFileName = "Test",
            SecondPartOfCacheFileName = "Type",
            PathToCacheFileDirectory = "C:\\directory\\"
        };

        //ATTEMPT
        var fileName = options.FormCacheFilePath();

        //VERIFY
        fileName.ShouldEqual("C:\\directory\\Test.Type.json");
    }
}