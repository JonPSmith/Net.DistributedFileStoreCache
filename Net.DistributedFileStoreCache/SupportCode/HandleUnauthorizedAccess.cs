// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

namespace Net.DistributedFileStoreCache.SupportCode;

/// <summary>
/// This contains extension methods to retry on certain exceptions
/// </summary>
public static class HandleUnauthorizedAccess
{
    /// <summary>
    /// This will run provided action that might cause a <see cref="UnauthorizedAccessException"/>.
    /// If that exception happens, then it will retry the action after a delay
    /// </summary>
    /// <param name="fileStoreCacheOptions">This contains parameters that set the delay and the number of times</param>
    /// <param name="applyAction"></param>
    /// <exception cref="DistributedFileStoreCacheException"></exception>
    public static void TryAgainOnUnauthorizedAccess(this DistributedFileStoreCacheOptions fileStoreCacheOptions, Action applyAction)
    {
        var numTries = 0;
        var success = false;
        while (!success)
        {
            try
            {
                applyAction();
                success = true;
            }
            catch (UnauthorizedAccessException e)
            {
                if (numTries++ > fileStoreCacheOptions.NumTriesOnUnauthorizedAccess)
                    throw new DistributedFileStoreCacheException(
                        "A file lock stopped this action for " +
                        $"{fileStoreCacheOptions.DelayMillisecondsOnUnauthorizedAccess * fileStoreCacheOptions.NumTriesOnUnauthorizedAccess:N0} milliseconds," +
                        " which is longer that the settings allow.",
                        e);
                Thread.Sleep(fileStoreCacheOptions.DelayMillisecondsOnUnauthorizedAccess);
            }
            catch (AggregateException e)
            {
                if (numTries++ > fileStoreCacheOptions.NumTriesOnUnauthorizedAccess)
                    throw new DistributedFileStoreCacheException(
                        "Another process stopped access for " +
                        $"{fileStoreCacheOptions.DelayMillisecondsOnUnauthorizedAccess * fileStoreCacheOptions.NumTriesOnUnauthorizedAccess:N0} milliseconds," +
                        " which is longer that the settings allow.",
                        e);
                Thread.Sleep(fileStoreCacheOptions.DelayMillisecondsOnUnauthorizedAccess);
            }
            catch (IOException e)
            {
                if (numTries++ > fileStoreCacheOptions.NumTriesOnUnauthorizedAccess)
                    throw new DistributedFileStoreCacheException(
                        "There was a problem on accessing the cache file for " +
                        $"{fileStoreCacheOptions.DelayMillisecondsOnUnauthorizedAccess * fileStoreCacheOptions.NumTriesOnUnauthorizedAccess:N0} milliseconds," +
                        " which is longer that the settings allow.",
                        e);
                Thread.Sleep(fileStoreCacheOptions.DelayMillisecondsOnUnauthorizedAccess);
            }
        }
    }

    /// <summary>
    /// This will run provided async action that might cause a <see cref="UnauthorizedAccessException"/>.
    /// If that exception happens, then it will retry the action after a delay
    /// </summary>
    /// <param name="fileStoreCacheOptions">This contains parameters that set the delay and the number of times</param>
    /// <param name="applyActionAsync"></param>
    /// <exception cref="DistributedFileStoreCacheException"></exception>
    public static async Task TryAgainOnUnauthorizedAccessAsync(this DistributedFileStoreCacheOptions fileStoreCacheOptions, 
        Func<ValueTask> applyActionAsync)
    {
        var numTries = 0;
        var success = false;
        while (!success)
        {
            try
            {
                await applyActionAsync();
                success = true;
            }
            catch (UnauthorizedAccessException e)
            {
                if (numTries++ > fileStoreCacheOptions.NumTriesOnUnauthorizedAccess)
                    throw new DistributedFileStoreCacheException(
                        "A file lock stopped this action for " +
                        $"{fileStoreCacheOptions.DelayMillisecondsOnUnauthorizedAccess * fileStoreCacheOptions.NumTriesOnUnauthorizedAccess:N0} milliseconds," +
                        "which is longer that the settings allow.",
                        e);
                await Task.Delay( fileStoreCacheOptions.DelayMillisecondsOnUnauthorizedAccess);
            }
            catch (AggregateException e)
            {
                if (numTries++ > fileStoreCacheOptions.NumTriesOnUnauthorizedAccess)
                    throw new DistributedFileStoreCacheException(
                        "Another process stopped access for " +
                        $"{fileStoreCacheOptions.DelayMillisecondsOnUnauthorizedAccess * fileStoreCacheOptions.NumTriesOnUnauthorizedAccess:N0} milliseconds," +
                        "which is longer that the settings allow.",
                        e);
                await Task.Delay(fileStoreCacheOptions.DelayMillisecondsOnUnauthorizedAccess);
            }
            catch (IOException e)
            {
                if (numTries++ > fileStoreCacheOptions.NumTriesOnUnauthorizedAccess)
                    throw new DistributedFileStoreCacheException(
                        "There was a problem on accessing the cache file for " +
                        $"{fileStoreCacheOptions.DelayMillisecondsOnUnauthorizedAccess * fileStoreCacheOptions.NumTriesOnUnauthorizedAccess:N0} milliseconds," +
                        "which is longer that the settings allow.",
                        e);
                await Task.Delay(fileStoreCacheOptions.DelayMillisecondsOnUnauthorizedAccess);
            }
        }
    }
}