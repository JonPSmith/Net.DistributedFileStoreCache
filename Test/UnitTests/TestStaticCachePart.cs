// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Text;
using Microsoft.Extensions.Caching.Distributed;
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
public class TestStaticCachePart
{
    private readonly DistributedFileStoreCacheOptions _options;
    private readonly ITestOutputHelper _output;

    public TestStaticCachePart(ITestOutputHelper output)
    {
        _output = output;

        _options = new DistributedFileStoreCacheOptions
        {
            PathToCacheFileDirectory = TestData.GetTestDataDir(),
            SecondPartOfCacheFileName = GetType().Name,
            IndentJsonInCacheFile = true,
            TurnOffStaticFilePathCheck = true
        };
    }

    private static void CreateNewCacheFile(string filePath)
    {
        byte[] buffer = Encoding.UTF8.GetBytes("{\r\n  \"Cache\": {}\r\n}");
        using FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 1, true);
        {
            fs.Write(buffer);
        }
    }

    [Fact]
    public void TestFileSystemWatcherChange_WriteAllFiles()
    {
        //SETUP
        var watcher = new FileSystemWatcher(_options.PathToCacheFileDirectory, _options.FormCacheFileName());
        watcher.EnableRaisingEvents = true;
        watcher.NotifyFilter = NotifyFilters.LastWrite;

        var count = 0;
        watcher.Changed += (sender, args) => count++;

        //ATTEMPT
        File.WriteAllText(_options.FormCacheFilePath(), "{\r\n  \"Cache\": {\r\n    \"test\": \"goodbye\"\r\n  }\r\n}");

        //VERIFY
        count.ShouldEqual(2);
    }

    [Fact]
    public void TestFileSystemWatcherChange_CreateNewCacheFile()
    {
        //SETUP
        var watcher = new FileSystemWatcher(_options.PathToCacheFileDirectory, _options.FormCacheFileName());
        watcher.EnableRaisingEvents = true;
        watcher.NotifyFilter = NotifyFilters.LastWrite;

        var count = 0;
        watcher.Changed += (sender, args) => count++;

        //ATTEMPT
        CreateNewCacheFile(_options.FormCacheFilePath());

        //VERIFY
        count.ShouldEqual(2);
    }

    [Fact]
    public void TestStartupNoCacheFile()
    {
        //SETUP
        if (File.Exists(_options.FormCacheFilePath())) File.Delete(_options.FormCacheFilePath());

        //ATTEMPT
        StaticCachePart.SetupStaticCache(_options);

        //VERIFY
        File.Exists(_options.FormCacheFilePath()).ShouldBeTrue();
        StaticCachePart.CacheContent.Cache.ShouldEqual(new Dictionary<string, string>());
        StaticCachePart.CacheContent.CacheOptions.ShouldEqual(new Dictionary<string, CacheEntryOptions>());
        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public void TestStartupCacheFileExists()
    {
        //SETUP
        File.WriteAllText(_options.FormCacheFilePath(), "{\r\n  \"Cache\": {\r\n    \"test\": \"goodbye\"\r\n  }\r\n}");

        //ATTEMPT
        StaticCachePart.SetupStaticCache(_options);

        //VERIFY
        _options.DisplayCacheFile(_output);
    }

    [Fact]
    public void TestStartupCacheChangeCacheFile()
    {
        //SETUP
        if (File.Exists(_options.FormCacheFilePath())) File.Delete(_options.FormCacheFilePath());
        StaticCachePart.SetupStaticCache(_options);

        //ATTEMPT
        File.WriteAllText(_options.FormCacheFilePath(), "{\r\n  \"Cache\": {\r\n    \"test\": \"goodbye\"\r\n  }\r\n}");

        //VERIFY
        _options.DisplayCacheFile(_output);
        StaticCachePart.LocalCacheIsOutOfDate.ShouldEqual(true);
    }
}