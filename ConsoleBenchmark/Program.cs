
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;
using Net.DistributedFileStoreCache;
using TestSupport.Helpers;

public class ConsoleBenchmark
{
    private readonly IDistributedFileStoreCacheStringWithExtras _distributedCache;

    public ConsoleBenchmark()
    {
        var services = new ServiceCollection();
        services.AddDistributedFileStoreCache(options =>
        {
            options.WhichInterface = DistributedFileStoreCacheInterfaces.DistributedFileStoreStringWithExtras;
            options.PathToCacheFileDirectory = TestData.GetCallingAssemblyTopLevelDir();
            options.SecondPartOfCacheFileName = GetType().Name;
            options.MaxBytesInJsonCacheFile = 50 * 1000;
        });
        var serviceProvider = services.BuildServiceProvider();

        _distributedCache = serviceProvider.GetRequiredService<IDistributedFileStoreCacheStringWithExtras>();
    }

    //[Params(1000)]
    [Params(10, 100, 1000)]
    public int NumKeysAtStart { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _distributedCache.ClearAll();
        for (int i = 0; i < NumKeysAtStart; i++)
        {
            _distributedCache.Set($"Key{i:D3}", DateTime.UtcNow.ToString("O"), null);
        }
    }

    [Benchmark]
    public void AddKey()
    {
        _distributedCache.Set("NewKey", DateTime.UtcNow.ToString("O"), null);
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
        _distributedCache.Get("Key000");
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
    }
}
