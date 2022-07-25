// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Test.TestHelpers;

public class StubEnvironment : IHostEnvironment
{
    public StubEnvironment(string environmentName, string contentRootPath)
    {
        EnvironmentName = environmentName;
        ContentRootPath = contentRootPath;
    }

    public string EnvironmentName { get; set; }
    public string ApplicationName { get; set; }
    public string ContentRootPath { get; set; }
    public IFileProvider ContentRootFileProvider { get; set; }
}