using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;
using Net.DistributedFileStoreCache;
using TestSupport.Helpers;

namespace ConsoleBenchmark;

public class ConsoleBenchmark
{
    private readonly IDistributedFileStoreCacheString _distributedCache;

    public ConsoleBenchmark()
    {
        var services = new ServiceCollection();
        services.AddDistributedFileStoreCache(options =>
        {
            options.WhichVersion = FileStoreCacheVersions.String;
            options.PathToCacheFileDirectory = TestData.GetCallingAssemblyTopLevelDir();
            options.SecondPartOfCacheFileName = GetType().Name;
            options.MaxBytesInJsonCacheFile = 50 * 10000;
        });
        var serviceProvider = services.BuildServiceProvider();

        _distributedCache = serviceProvider.GetRequiredService<IDistributedFileStoreCacheString>();
    }

    //[Params(10_000)]
    [Params(100, 1000, 10_000)]
    public int NumKeysAtStart { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        var allKeyValues = new List<KeyValuePair<string,string>>();
        for (int i = 0; i < NumKeysAtStart; i++)
        {
            allKeyValues.Add(new KeyValuePair<string, string>($"Key{i:D4}", DateTime.UtcNow.ToString("O")));
        }
        _distributedCache.ClearAll(allKeyValues);
    }

    [Benchmark]
    public void AddKey()
    {
        _distributedCache.Set("NewKey", DateTime.UtcNow.ToString("O"), null);
        _distributedCache.Get("NewKey"); //This forces an read
    }

    [Benchmark]
    public void AddManyKey100()
    {
        var allKeyValues = new List<KeyValuePair<string, string>>();
        for (int i = 0; i < 100; i++)
        {
            allKeyValues.Add(new KeyValuePair<string, string>($"NewKey{i:D4}", DateTime.UtcNow.ToString("O")));
        }

        _distributedCache.SetMany(allKeyValues);
        _distributedCache.Get("NewKey"); //This forces an read
    }

    [Benchmark]
    public async Task AddKeyAsync()
    {
        await _distributedCache.SetAsync("NewKey", DateTime.UtcNow.ToString("O"), null);
        await _distributedCache.GetAsync("NewKey"); //This forces an read
    }

    [Benchmark]
    public void GetKey()
    {
        _distributedCache.Get("Key0000");
    }

    [Benchmark]
    public void GetAllKeyValues()
    {
        var all = _distributedCache.GetAllKeyValues();
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
    }
}