Net.DistributedFileStoreCache


## Performance figures

I used 

Each key/value takes 37 charcters and the size of the cache file are:

| NumKeysValues | Cache file size|
|-------------- |------------:|
|           100 |      4.6 kb |
|          1000 |     40.1 kb |
|         10000 |    400.0 kb |

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1766 (21H1/May2021Update)
Intel Core i9-9940X CPU 3.30GHz, 1 CPU, 28 logical and 14 physical cores
.NET SDK=6.0.203
  [Host]     : .NET 6.0.7 (6.0.722.32202), X64 RyuJIT
  DefaultJob : .NET 6.0.7 (6.0.722.32202), X64 RyuJIT



### Read times

Summary of the read side is:

- Reads a single cache value took ~25 ns at all levels of of cache side tested.
- Getting a Dictionary of ALL the cache key/values took ~80 ns at all levels of of cache side tested.

|          Method | NumKeysAtStart |        Mean |       Error |    StdDev |
|---------------- |--------------- |------------:|------------:|----------:|
|          GetKey |            100 |    22.69 ns |    0.367 ns |  0.343 ns |
| GetAllKeyValues |            100 |    84.12 ns |    1.251 ns |  1.170 ns |
|          GetKey |           1000 |    21.24 ns |    0.322 ns |  0.301 ns |
| GetAllKeyValues |           1000 |    81.42 ns |    1.104 ns |  1.033 ns |
|          GetKey |          10000 |    24.28 ns |    0.314 ns |  0.278 ns |
| GetAllKeyValues |          10000 |    81.36 ns |    0.996 ns |  0.932 ns |

### Write times

Summary of the write side is:

- Adding a cache value to cache goes up as the size of the cache is. This makes sense as unlike a database you 
are reading and then writing ALL the cache values into a file.

|          Method | NumKeysAtStart |        Mean |     Error |    StdDev |
|---------------- |--------------- |------------:|----------:|----------:|
|          AddKey |            100 | 1,302.69 us |   9.85 us |   9.21 us |
|     AddKeyAsync |            100 | 1,664.47 us |  32.51 us |  34.79 us |
|          AddKey |           1000 | 1,673.28 us |  25.60 us |  23.95 us |
|     AddKeyAsync |           1000 | 2,267.81 us |  45.10 us |  42.18 us |
|          AddKey |          10000 | 7,898.67 us | 172.10 us | 507.46 us |
|     AddKeyAsync |          10000 | 8,922.15 us | 178.30 us | 307.57 us |

