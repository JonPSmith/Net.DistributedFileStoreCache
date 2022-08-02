// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

namespace Net.DistributedFileStoreCache.SupportCode;

/// <summary>
/// This class defines the content of the json cache file
/// </summary>
public class CacheJsonContent
{
    /// <summary>
    /// This holds all the cache entries
    /// </summary>
    public Dictionary<string,string> Cache { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// This contains all the absolute timeout applied to an cache entry. The cache entry key is used.
    /// </summary>
    public Dictionary<string, long> TimeOuts { get; set; } = new Dictionary<string, long>();
}