// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TestSupport.Attributes;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests;


public class TestSqlServerTiming
{
    private readonly ITestOutputHelper _output;

    public TestSqlServerTiming(ITestOutputHelper output)
    {
        _output = output;
    }

    public class MyCache
    {
        [Key]
        public string Key { get; set; }
        public string Value { get; set; }
    }



    public class TestDbContext : DbContext
    {
        public DbSet<MyCache> Cache { get; set; }

        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options) { }
    }


    //I build this to see how quick 
    [RunnableInDebugOnly]
    public void TestReadFileWithShareNone()
    {
        //SETUP
        var options = this.CreateUniqueClassOptions<TestDbContext>();
        var context = new TestDbContext(options);
        
        context.Database.EnsureClean();

        const int NumTest = 100;
        //warmup
        for (int i = 0; i < 10; i++)
        {
            context.Add(new MyCache { Key = $"Key1{i:D4}", Value = DateTime.Now.Ticks.ToString() });
            context.SaveChanges();
        }

        //ATTEMPT
        using (new TimeThings(_output, "sql", NumTest))
        {
            for (int i = 0; i < NumTest; i++)
            {
                context.Add(new MyCache { Key = $"Key2{i:D4}", Value = DateTime.Now.Ticks.ToString() });
                context.SaveChanges();
                context.ChangeTracker.Clear();
            }
        }


        //VERIFY
    }


}