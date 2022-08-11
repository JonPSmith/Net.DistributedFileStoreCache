// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Text.Json;
using Net.DistributedFileStoreCache.SupportCode;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Test.UnitTests;

//NOTE: I tried using streaming to improve performance, but it didn't make any change
//I decided to NOT use streaming because you could add more data than the sync version can handle 
public class TestJsonSerializerStream
{
    private readonly ITestOutputHelper _output;
    private readonly string _filePath;

    public TestJsonSerializerStream(ITestOutputHelper output)
    {
        _output = output;
        _filePath = Path.Combine(TestData.GetTestDataDir(), $"{GetType().Name}.json");
    }

    private async Task UpdateFileInLock(string key, string value)
    {
        using FileStream fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None, bufferSize: 1, true);
        {
            var reader =new StreamReader(fileStream).BaseStream;
            var json = JsonSerializer.Deserialize<CacheJsonContent>(reader, new JsonSerializerOptions());
            json.Cache[key] = value;
            fileStream.Seek(0, SeekOrigin.Begin);
            fileStream.SetLength(0);
            var writer = new StreamWriter(fileStream).BaseStream;

            await JsonSerializer.SerializeAsync(writer, json);
        }
    }

    [Fact]
    public async Task TestUpdateJsonFile()
    {
        //SETUP
        File.WriteAllText(_filePath, "{\r\n  \"Cache\": {}\r\n}");

        //ATTEMPT
        await UpdateFileInLock("test", "does it work");

        //VERIFY
        _output.WriteLine(File.ReadAllText(_filePath));
    }

 
}