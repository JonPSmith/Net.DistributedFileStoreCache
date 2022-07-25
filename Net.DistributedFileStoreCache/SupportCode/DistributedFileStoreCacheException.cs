// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

namespace Net.DistributedFileStoreCache.SupportCode;

public class DistributedFileStoreCacheException : Exception
{
    public DistributedFileStoreCacheException(string message) : base(message){}
    public DistributedFileStoreCacheException(string message, Exception innerException) : base(message, innerException){}
}