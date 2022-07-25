// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Text;
using System.Text.Json;

namespace Net.DistributedFileStoreCache.SupportCode;

internal class CacheFileHandler
{
    private readonly DistributedFileStoreCacheOptions _fileStoreCacheOptions;

    public CacheFileHandler (DistributedFileStoreCacheOptions fileStoreCacheOptions)
    {
        _fileStoreCacheOptions = fileStoreCacheOptions;
    }

    public string? GetValue(string key)
    {
        if (StaticCachePart.LocalCacheIsOutOfDate)
            _fileStoreCacheOptions.TryAgainOnUnauthorizedAccess(UpdateLocalCacheFromCacheFile);

        return StaticCachePart.ReadCacheKeyValues!.TryGetValue(key, out string? value) ? value : null;
    }

    public async Task<string?> GetValueAsync(string key)
    {
        if (StaticCachePart.LocalCacheIsOutOfDate)
            await _fileStoreCacheOptions.TryAgainOnUnauthorizedAccessAsync(UpdateLocalCacheFromCacheFileAsync);

        return StaticCachePart.ReadCacheKeyValues!.TryGetValue(key, out string? value) ? value : null;
    }

    public IReadOnlyDictionary<string, string> GetAllValues()
    {
        if (StaticCachePart.LocalCacheIsOutOfDate)
            _fileStoreCacheOptions.TryAgainOnUnauthorizedAccess(UpdateLocalCacheFromCacheFile);

        return StaticCachePart.ReadCacheKeyValues!;
    }

    public async Task<IReadOnlyDictionary<string, string>> GetAllValuesAsync()
    {
        if (StaticCachePart.LocalCacheIsOutOfDate)
            await _fileStoreCacheOptions.TryAgainOnUnauthorizedAccessAsync(UpdateLocalCacheFromCacheFileAsync);

        return StaticCachePart.ReadCacheKeyValues!;
    }

    public void AddKeyValueToCacheFile(string key, string value)
    {
        _fileStoreCacheOptions.TryAgainOnUnauthorizedAccess(() =>
            ReadAndChangeCacheJsonFile(CacheChanges.Add, false, key, value)
                .CheckSyncValueTaskWorked());
    }

    public async Task AddKeyValueToCacheFileAsync(string key, string value)
    {
        await _fileStoreCacheOptions.TryAgainOnUnauthorizedAccessAsync(async () =>
            await ReadAndChangeCacheJsonFile(CacheChanges.Add, true, key, value));
    }

    public void RemoveKeyValueToCacheFile(string key)
    {
        _fileStoreCacheOptions.TryAgainOnUnauthorizedAccess(() =>
            ReadAndChangeCacheJsonFile(CacheChanges.Remove, false, key)
                .CheckSyncValueTaskWorked());
    }

    public async Task RemoveKeyValueToCacheFileAsync(string key)
    {
        await _fileStoreCacheOptions.TryAgainOnUnauthorizedAccessAsync( async () => 
            await ReadAndChangeCacheJsonFile(CacheChanges.Remove, false, key));
    }

    public void RefreshCacheFile()
    {
        if (StaticCachePart.LocalCacheIsOutOfDate)
            _fileStoreCacheOptions.TryAgainOnUnauthorizedAccess(UpdateLocalCacheFromCacheFile);
    }

    public async Task RefreshCacheFileAsync()
    {
        if (StaticCachePart.LocalCacheIsOutOfDate)
            await _fileStoreCacheOptions.TryAgainOnUnauthorizedAccessAsync(UpdateLocalCacheFromCacheFileAsync);
    }

    public void ResetCacheFile()
    {
        _fileStoreCacheOptions.TryAgainOnUnauthorizedAccess(() =>
            ReadAndChangeCacheJsonFile(CacheChanges.Reset, false).CheckSyncValueTaskWorked());
    }


    /// <summary>
    /// This should ONLY be used on startup. Its job is to ensure there is a cache file 
    /// </summary>
    public void CreateNewCacheFileIfMissingWithRetry()
    {
        //Create a valid cache file containing no key/values
        var writeBytes = FillByteBufferWithCacheJsonData(new CacheKeyValues());

        //We run this within a retry loop to make sure it succeeds 
        _fileStoreCacheOptions.TryAgainOnUnauthorizedAccess(() =>
        {
            var cacheFilePath = _fileStoreCacheOptions.FormCacheFilePath();
            if (!File.Exists(cacheFilePath))
            {
                //This uses FileMode.CreateNew to ensure only one file is created
                using FileStream writeStream = new FileStream(cacheFilePath, FileMode.CreateNew, FileAccess.Write,
                    FileShare.None, bufferSize: 1, false);
                {
                    writeStream.Write(writeBytes, 0, writeBytes.Length);
                }
            }
        });
    }

    //-----------------------------------------------------------------
    //private methods

    private void UpdateLocalCacheFromCacheFile()
    {
        var readBuffer = new byte[_fileStoreCacheOptions.MaxBytesInJsonCacheFile];
        var readFilePath = _fileStoreCacheOptions.FormCacheFilePath();


        //This uses FileShare.None to ensure multiple instances don't try to update the in-memory cache at the same time
        using FileStream readStream = new FileStream(readFilePath, FileMode.Open, FileAccess.Read, FileShare.None,
            bufferSize: 1, false);
        {
            var numBytesRead = readStream.Read(readBuffer);
            if (numBytesRead >= _fileStoreCacheOptions.MaxBytesInJsonCacheFile)
                throw new DistributedFileStoreCacheException(
                    $"Your cache json file has more that {_fileStoreCacheOptions.MaxBytesInJsonCacheFile} " +
                    $"bytes, so you MUST set the option's {nameof(DistributedFileStoreCacheOptions.MaxBytesInJsonCacheFile)} to a bigger value.");

            StaticCachePart.UpdateInMemoryCache(GetJsonFromByteBuffer(numBytesRead, ref readBuffer).Cache);
        }

    }

    private async ValueTask UpdateLocalCacheFromCacheFileAsync()
    {
        var readBuffer = new byte[_fileStoreCacheOptions.MaxBytesInJsonCacheFile];
        var readFilePath = _fileStoreCacheOptions.FormCacheFilePath();
        //This uses FileShare.None to ensure multiple instances don't try to update the in-memory cache at the same time
        using FileStream readStream = new FileStream(readFilePath, FileMode.Open, FileAccess.Read, FileShare.None,
            bufferSize: 1, true);
        {
            var numBytesRead = await readStream.ReadAsync(readBuffer);
            if (numBytesRead >= _fileStoreCacheOptions.MaxBytesInJsonCacheFile)
                throw new DistributedFileStoreCacheException(
                    $"Your cache json file has more that {_fileStoreCacheOptions.MaxBytesInJsonCacheFile} " +
                    $"bytes, so you MUST set the option's {nameof(DistributedFileStoreCacheOptions.MaxBytesInJsonCacheFile)} to a bigger value.");

            StaticCachePart.UpdateInMemoryCache(GetJsonFromByteBuffer(numBytesRead, ref readBuffer).Cache);
        }
    }

    private enum CacheChanges { Add, Remove, Reset}

    private async ValueTask ReadAndChangeCacheJsonFile(CacheChanges whatToDo, bool useAsync, string? key = null, string? value = null)
    {
        //thanks to https://stackoverflow.com/questions/15628902/lock-file-exclusively-then-delete-move-it for this approach

        int numBytesRead = 0;
        var readWriteBuffer = new byte[_fileStoreCacheOptions.MaxBytesInJsonCacheFile];
        var filePath = _fileStoreCacheOptions.FormCacheFilePath();
        using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None, bufferSize: 1, useAsync);
        {
            if(whatToDo != CacheChanges.Reset)
            {
                numBytesRead = useAsync 
                    ? await fileStream.ReadAsync(readWriteBuffer)
                    : fileStream.Read(readWriteBuffer);
                if (numBytesRead >= _fileStoreCacheOptions.MaxBytesInJsonCacheFile)
                    throw new DistributedFileStoreCacheException(
                        $"Your cache json file has more that {_fileStoreCacheOptions.MaxBytesInJsonCacheFile} " +
                        $"bytes, so you MUST set the option's {nameof(DistributedFileStoreCacheOptions.MaxBytesInJsonCacheFile)} to a bigger value.");
            }

            CacheKeyValues json;
            switch (whatToDo)
            {
                case CacheChanges.Add:
                    if (key == null) throw new NullReferenceException("The key cannot be null");
                    if (value == null) throw new NullReferenceException("The value cannot be null");
                    json = GetJsonFromByteBuffer(numBytesRead, ref readWriteBuffer);
                    json.Cache[key] = value;
                    break;
                case CacheChanges.Remove:
                    if (key == null) throw new NullReferenceException("The key cannot be null");
                    json = GetJsonFromByteBuffer(numBytesRead, ref readWriteBuffer);
                    json.Cache.Remove(key);
                    break;
                case CacheChanges.Reset:
                    json = new CacheKeyValues();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(whatToDo), whatToDo, null);
            }

           
            //thanks to https://stackoverflow.com/questions/15628902/lock-file-exclusively-then-delete-move-it
            fileStream.Seek(0, SeekOrigin.Begin);
            fileStream.SetLength(0);
            if (useAsync)
                await fileStream.WriteAsync(FillByteBufferWithCacheJsonData(json));
            else
                fileStream.Write(FillByteBufferWithCacheJsonData(json));
        }
    }

    private CacheKeyValues GetJsonFromByteBuffer(int numBytes, ref byte[] buffer)
    {
        if (numBytes == 0)
            return new CacheKeyValues();
        var jsonString = Encoding.UTF8.GetString(buffer, 0, numBytes);

        return JsonSerializer.Deserialize<CacheKeyValues>(jsonString)!;
    }

    private byte[] FillByteBufferWithCacheJsonData(CacheKeyValues allCache)
    {
        var jsonString = JsonSerializer.Serialize(allCache,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        return Encoding.UTF8.GetBytes(jsonString);
    }
}