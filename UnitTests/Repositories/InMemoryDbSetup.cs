using System;
using DemoForum.Models.Entities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace DemoForumTests.Repositories;

public abstract class InMemoryDbSetup
{
    protected ForumContext ForumContext = null!;
    
    [SetUp]
    public void SetUp()
    {
        DbContextOptions<ForumContext> inMemoryOptions = new DbContextOptionsBuilder<ForumContext>()
            .UseInMemoryDatabase(DateTime.Now.Ticks.ToString())
            .EnableSensitiveDataLogging()
            .Options;

        ForumContext = new ForumContext(inMemoryOptions);
    }

    [TearDown]
    public void TearDown()
    {
        ForumContext.Dispose(); 
    }
}