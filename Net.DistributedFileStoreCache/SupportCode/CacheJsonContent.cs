// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

namespace Net.DistributedFileStoreCache.SupportCode;

public class CacheJsonContent
{
    public const string CacheSection = nameof(Cache);

    public Dictionary<string,string> Cache { get; set; } = new Dictionary<string, string>();

    public Dictionary<string, long> TimeOuts { get; set; } = new Dictionary<string, long>();

}