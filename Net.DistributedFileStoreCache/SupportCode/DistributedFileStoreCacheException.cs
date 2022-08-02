// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

namespace Net.DistributedFileStoreCache.SupportCode;

/// <summary>
/// This is a <see cref="Net.DistributedFileStoreCache"/> exception to use if a error occurs
/// </summary>
public class DistributedFileStoreCacheException : Exception
{
    /// <summary>
    /// Basic exception with just a message 
    /// </summary>
    /// <param name="message"></param>
    public DistributedFileStoreCacheException(string message) : base(message){}

    /// <summary>
    /// This allows another exception be held in the innerException
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public DistributedFileStoreCacheException(string message, Exception innerException) : base(message, innerException){}
}