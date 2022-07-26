// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Text;
using Net.DistributedFileStoreCache;
using Net.DistributedFileStoreCache.SupportCode;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests;

// see https://stackoverflow.com/questions/1408175/execute-unit-tests-serially-rather-than-in-parallel
[Collection("Sequential")]
public class TestFileLock
{
    private readonly string _jsonFilePath;
    private readonly ITestOutputHelper _output;

    /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
    public TestFileLock(ITestOutputHelper output)
    {
        _output = output;
        _jsonFilePath = Path.Combine(TestData.GetTestDataDir(), "testlock.json");
        if (!Directory.Exists(_jsonFilePath))
            File.WriteAllText(_jsonFilePath, "{\r\n  \"Cache\": {}\r\n}");

    }

    private static (byte[] bytes, int numBytes) ReadFileWithShareNone(string filePath, Action? doInLock = null)
    {
        byte[] buffer = new byte[64000];
        int numBytesRead;
        using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, bufferSize: 1, true);
        {
            numBytesRead = fs.Read(buffer);
            doInLock?.Invoke();
        }
        return (buffer, numBytesRead);
    }

    private static void CreateNewCacheFile(string filePath)
    {
        byte[] buffer = Encoding.UTF8.GetBytes("{\r\n  \"Cache\": {}\r\n}");
        using FileStream fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize: 1, true);
        {
            fs.Write(buffer);
        }
    }


    [Fact]
    public void TestReadFileWithShareNone()
    {
        //SETUP

        //ATTEMPT
        var data = ReadFileWithShareNone(_jsonFilePath);

        //VERIFY
        var json = Encoding.UTF8.GetString(data.bytes, 0, data.numBytes);
        _output.WriteLine(json);
    }

    [Fact]
    public void TestTestReadFileWithShareNone_TryAgainOnUnauthorizedAccess()
    {
        //SETUP
        var options = new DistributedFileStoreCacheOptions
        {
            NumTriesOnUnauthorizedAccess = 10,
            DelayMillisecondsOnUnauthorizedAccess = 100
        };

        double totalMilliseconds = 0;

        //ATTEMPT
        using (new TimeThings(x => totalMilliseconds = x.TotalTimeMilliseconds))
        {
            var ex = Assert.Throws<DistributedFileStoreCacheException>(() => options.TryAgainOnUnauthorizedAccess( () =>
                ReadFileWithShareNone(_jsonFilePath, 
                    () => { ReadFileWithShareNone(_jsonFilePath, null); })));
        }

        //VERIFY
        totalMilliseconds.ShouldBeInRange(10*100, 10 * 100 + 2000);
    }

    [Fact]
    public void TestTestReadFileWithShareNone_AccessWithinLock()
    {
        //SETUP

        //ATTEMPT
        var ex = Assert.Throws<IOException>(() => 
            ReadFileWithShareNone(_jsonFilePath,
            () => { ReadFileWithShareNone(_jsonFilePath, null); }));

        //VERIFY
        ex.Message.ShouldStartWith("The process cannot access the file");
        ex.Message.ShouldEndWith("because it is being used by another process.");
    }

    [Fact]
    public void TestCreateANewFileWhenFileAlreadyExists()
    {
        //SETUP

        //ATTEMPT
        var ex = Assert.Throws<IOException>(() => CreateNewCacheFile(_jsonFilePath));

        //VERIFY
        ex.Message.ShouldEndWith("already exists.");
    }

    [Fact]
    public void TestTestReadFileWithShareNone_FileSystemWatcher_Changed()
    {
        //SETUP
        File.WriteAllText(_jsonFilePath, "{\r\n  \"Cache\": {}\r\n}");
        var watcher = new FileSystemWatcher(TestData.GetTestDataDir());
        watcher.EnableRaisingEvents = true;
        watcher.NotifyFilter = NotifyFilters.LastWrite;

        bool hasChanged = false;
        watcher.Changed += (sender, args) =>
            hasChanged = true;

        //ATTEMPT
        hasChanged.ShouldBeFalse();
        using (new TimeThings(_output))
        {
            File.WriteAllText(_jsonFilePath, "{\r\n  \"Cache\": {\"Still there\": \"keep this\"}\r\n}");
        }
        var time2 = DateTime.Now;

        //VERIFY
        var fileTime = File.GetLastWriteTime(_jsonFilePath);
        _output.WriteLine(fileTime.ToString("O"));
        hasChanged.ShouldEqual(true);
    }

    [Fact]
    public void TestTestReadFileWithShareNone_FileSystemWatcher_Renamed()
    {
        //SETUP
        var filePath = Path.Combine(TestData.GetTestDataDir(), "testlock.1.json");
        File.WriteAllText(filePath, "{\r\n  \"Cache\": {}\r\n}");
        var watcher = new FileSystemWatcher(TestData.GetTestDataDir());
        watcher.EnableRaisingEvents = true;
        watcher.NotifyFilter = NotifyFilters.FileName;

        bool hasChanged = false;
        var newName = Path.GetFileName(filePath);
        watcher.Renamed += (sender, args) =>
        {
            hasChanged = true;
            newName = args.Name;
        };

        //ATTEMPT
        hasChanged.ShouldBeFalse();
        using (new TimeThings(_output))
        {
            File.Move(filePath, Path.Combine(TestData.GetTestDataDir(), "testlock.2.json"),true);
        }
        using (new TimeThings(_output))
        {
            File.Delete(filePath);
        }
        var time2 = DateTime.Now;

        //VERIFY
        hasChanged.ShouldEqual(true);
        newName.ShouldEqual("testlock.2.json");
    }
}