// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.IO;
using Net.DistributedFileStoreCache;
using Net.DistributedFileStoreCache.SupportCode;
using Xunit.Abstractions;

namespace Test.TestHelpers;

public static class DisplayExtensions
{
    public static void DisplayCacheFile(this DistributedFileStoreCacheOptions options, ITestOutputHelper output)
    {
        var cacheFilePath = options.FormCacheFilePath();
        options.TryAgainOnUnauthorizedAccess(() =>
        {
            if (!File.Exists(cacheFilePath))
            {
                output.WriteLine($"No cache file called {Path.GetFileName(cacheFilePath)} was found.");
                return;
            }

            var lines = File.ReadAllLines(cacheFilePath);
            foreach (var line in lines)
            {
                output.WriteLine(line);
            }
        });
    }

}