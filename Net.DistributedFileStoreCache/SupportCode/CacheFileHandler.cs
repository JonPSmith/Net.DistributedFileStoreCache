// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Net.DistributedFileStoreCache.SupportCode;

/// <summary>
/// This class contains all the code that accesses the local static cache and the json cache file.
/// This class should be internal, but you can't use protected in a public class.
/// </summary>
public class CacheFileHandler
{
    private readonly DistributedFileStoreCacheOptions _options;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="options"></param>
    public CacheFileHandler (DistributedFileStoreCacheOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// This handles the Get
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string? GetValue(string key)
    {
        if (StaticCachePart.LocalCacheIsOutOfDate)
            _options.TryAgainOnUnauthorizedAccess(UpdateLocalCacheFromCacheFile);

        return StaticCachePart.CacheContent.ReturnNullIfExpires(key);
    }

    /// <summary>
    /// This handles the GetAsync
    /// </summary>
    /// <param name="key"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<string?> GetValueAsync(string key, CancellationToken token)
    {
        if (StaticCachePart.LocalCacheIsOutOfDate)
            await _options.TryAgainOnUnauthorizedAccessAsync(() => UpdateLocalCacheFromCacheFileAsync(token));

        return StaticCachePart.CacheContent.ReturnNullIfExpires(key);
    }

    /// <summary>
    /// This handles the GetAllValues 
    /// </summary>
    /// <returns></returns>
    public IReadOnlyDictionary<string, string> GetAllValues()
    {
        if (StaticCachePart.LocalCacheIsOutOfDate)
            _options.TryAgainOnUnauthorizedAccess(UpdateLocalCacheFromCacheFile);

        return StaticCachePart.CacheContent.ReturnNonExpiredCacheValues();
    }

    /// <summary>
    /// This handles the GetAllValuesAsync 
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<IReadOnlyDictionary<string, string>> GetAllValuesAsync(CancellationToken token)
    {
        if (StaticCachePart.LocalCacheIsOutOfDate)
            await _options.TryAgainOnUnauthorizedAccessAsync(() => UpdateLocalCacheFromCacheFileAsync(token));

        return StaticCachePart.CacheContent.ReturnNonExpiredCacheValues();
    }

    /// <summary>
    /// This handles the Set
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="entryOptions"></param>
    public void SetKeyValue(string key, string value, DistributedCacheEntryOptions? entryOptions)
    {
        var setter = new CacheFileSetOne(key, value, entryOptions);
        _options.TryAgainOnUnauthorizedAccess(() =>
            ReadAndChangeCacheJsonFile(setter.SetKeyValueHandler, false)
                .CheckSyncValueTaskWorked());
    }

    /// <summary>
    /// This handles the SetAsync
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="entryOptions"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task SetKeyValueAsync(string key, string value, DistributedCacheEntryOptions? entryOptions,
        CancellationToken token)
    {
        var setter = new CacheFileSetOne(key, value, entryOptions);
        await _options.TryAgainOnUnauthorizedAccessAsync(async () =>
            await ReadAndChangeCacheJsonFile(setter.SetKeyValueHandler, true, token: token));
    }

    /// <summary>
    /// This handles the SetMany
    /// </summary>
    /// <param name="manyEntries"></param>
    /// <param name="entryOptions"></param>
    public void SetKeyValueMany(List<KeyValuePair<string, string>> manyEntries, DistributedCacheEntryOptions? entryOptions)
    {
        var setMany = new CacheFileSetMany(manyEntries, entryOptions);
        _options.TryAgainOnUnauthorizedAccess(() =>
            ReadAndChangeCacheJsonFile(setMany.SetManyKeyValueHandler, false)
                .CheckSyncValueTaskWorked());
    }

    /// <summary>
    /// This handles the SetManyAsync
    /// </summary>
    /// <param name="manyEntries"></param>
    /// <param name="entryOptions"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task SetKeyValueManyAsync(List<KeyValuePair<string, string>> manyEntries, DistributedCacheEntryOptions? entryOptions,
        CancellationToken token)
    {
        var setMany = new CacheFileSetMany(manyEntries, entryOptions);
        await _options.TryAgainOnUnauthorizedAccessAsync(async () =>
            await ReadAndChangeCacheJsonFile(setMany.SetManyKeyValueHandler, true, token: token));
    }

    /// <summary>
    /// This handles the Remove
    /// </summary>
    /// <param name="key"></param>
    public void RemoveKeyValue(string key)
    {
        var remover = new CacheFileRemove(key);
        _options.TryAgainOnUnauthorizedAccess(() =>
            ReadAndChangeCacheJsonFile(remover.RemoveKeyValueHandler, false)
                .CheckSyncValueTaskWorked());
    }


    /// <summary>
    /// This handles the RemoveAsync
    /// </summary>
    /// <param name="key"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task RemoveKeyValueAsync(string key, CancellationToken token)
    {
        var remover = new CacheFileRemove(key);
        await _options.TryAgainOnUnauthorizedAccessAsync(async () =>
            await ReadAndChangeCacheJsonFile(remover.RemoveKeyValueHandler, false, token: token));
    }

    /// <summary>
    /// This handles the ClearAll
    /// </summary>
    /// <param name="manyEntries">if not null, then after of the clearing the cache these KeyValues will written into the cache</param>
    /// <param name="entryOptions">Optional: If there are entries to add to the cache, this will set the timeout time.</param>
    public void ResetCacheFile(List<KeyValuePair<string, string>>? manyEntries, DistributedCacheEntryOptions? entryOptions)
    {
        var setMany = new CacheFileSetMany(manyEntries, entryOptions);
        _options.TryAgainOnUnauthorizedAccess(() =>
            ReadAndChangeCacheJsonFile(setMany.SetManyKeyValueHandler , false, true).CheckSyncValueTaskWorked());
    }


    /// <summary>
    /// This should ONLY be used on startup. Its job is to ensure there is a cache file 
    /// </summary>
    public void CreateNewCacheFileIfMissingWithRetry()
    {
        //Create a valid cache file containing no key/values
        var writeBytes = FillByteBufferWithCacheJsonData(new CacheJsonContent());

        //We run this within a retry loop to make sure it succeeds 
        _options.TryAgainOnUnauthorizedAccess(() =>
        {
            var cacheFilePath = _options.FormCacheFilePath();
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
        var readBuffer = new byte[_options.MaxBytesInJsonCacheFile];
        var readFilePath = _options.FormCacheFilePath();

        //This uses FileShare.None to ensure multiple instances don't try to update the in-memory cache at the same time
        using FileStream readStream = new FileStream(readFilePath, FileMode.Open, FileAccess.Read, FileShare.None,
            bufferSize: 1, false);
        {
            var numBytesRead = readStream.Read(readBuffer);
            if (numBytesRead >= _options.MaxBytesInJsonCacheFile)
                throw new DistributedFileStoreCacheException(
                    $"Your cache json file has more that {_options.MaxBytesInJsonCacheFile} " +
                    $"bytes, so you MUST set the option's {nameof(DistributedFileStoreCacheOptions.MaxBytesInJsonCacheFile)} to a bigger value.");

            StaticCachePart.UpdateLocalCache(GetJsonFromByteBuffer(numBytesRead, ref readBuffer));
        }
    }

    private async ValueTask UpdateLocalCacheFromCacheFileAsync(CancellationToken token)
    {
        var readBuffer = new byte[_options.MaxBytesInJsonCacheFile];
        var readFilePath = _options.FormCacheFilePath();
        //This uses FileShare.None to ensure multiple instances don't try to update the in-memory cache at the same time
        using FileStream readStream = new FileStream(readFilePath, FileMode.Open, FileAccess.Read, FileShare.None,
            bufferSize: 1, true);
        {
            var numBytesRead = await readStream.ReadAsync(readBuffer, token);
            if (numBytesRead >= _options.MaxBytesInJsonCacheFile)
                throw new DistributedFileStoreCacheException(
                    $"Your cache json file has more that {_options.MaxBytesInJsonCacheFile} " +
                    $"bytes, so you MUST set the option's {nameof(DistributedFileStoreCacheOptions.MaxBytesInJsonCacheFile)} to a bigger value.");

            StaticCachePart.UpdateLocalCache(GetJsonFromByteBuffer(numBytesRead, ref readBuffer));
        }
    }
    public delegate void UpdateJsonDelegate(ref CacheJsonContent updateCurrentJson);

    private async ValueTask ReadAndChangeCacheJsonFile(UpdateJsonDelegate? updateCurrentJson, bool useAsync, 
        bool reset = false, CancellationToken token = new ())
    {
        //thanks to https://stackoverflow.com/questions/15628902/lock-file-exclusively-then-delete-move-it for this approach

        int numBytesRead = 0;
        var readWriteBuffer = new byte[_options.MaxBytesInJsonCacheFile];
        var filePath = _options.FormCacheFilePath();
        using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None, bufferSize: 1, useAsync);
        {
            if(!reset)
            {
                numBytesRead = useAsync 
                    ? await fileStream.ReadAsync(readWriteBuffer, token)
                    : fileStream.Read(readWriteBuffer);
                if (numBytesRead >= _options.MaxBytesInJsonCacheFile)
                    throw new DistributedFileStoreCacheException(
                        $"Your cache json file has more that {_options.MaxBytesInJsonCacheFile} " +
                        $"bytes, so you MUST set the option's {nameof(DistributedFileStoreCacheOptions.MaxBytesInJsonCacheFile)} to a bigger value.");
            }

            var json = reset ? new CacheJsonContent() : GetJsonFromByteBuffer(numBytesRead, ref readWriteBuffer);
            updateCurrentJson?.Invoke(ref json);

            var bytesToWrite = FillByteBufferWithCacheJsonData(json);
            if (bytesToWrite.Length < _options.MaxBytesInJsonCacheFile)
            {
                //If the data has become longer that the set bytes, then we don't update the cache file (which means the change is lost)

                //thanks to https://stackoverflow.com/questions/15628902/lock-file-exclusively-then-delete-move-it
                fileStream.Seek(0, SeekOrigin.Begin);
                fileStream.SetLength(0);
                if (useAsync)
                    await fileStream.WriteAsync(bytesToWrite, token);
                else
                    fileStream.Write(bytesToWrite);

                //This is here to try and negate the first trigger of the file change
                StaticCachePart.UpdateLocalCache(json);
            }
        }
    }

    private CacheJsonContent GetJsonFromByteBuffer(int numBytes, ref byte[] buffer)
    {
        if (numBytes == 0)
            return new CacheJsonContent();
        var jsonString = Encoding.UTF8.GetString(buffer, 0, numBytes);

        var cacheContent = JsonSerializer.Deserialize<CacheJsonContent>(jsonString)!;
        cacheContent.RemoveExpiredCacheValues();
        return cacheContent;
    }

    private byte[] FillByteBufferWithCacheJsonData(CacheJsonContent allCache)
    {
        var jsonString = JsonSerializer.Serialize(allCache, _options.JsonSerializerForCacheFile);

        return Encoding.UTF8.GetBytes(jsonString);
    }


}