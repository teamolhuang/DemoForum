﻿using System;
using System.Linq;
using System.Threading.Tasks;
using DemoForum.Models.Entities;
using DemoForum.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace DemoForumTests.Repositories;

public class UserRepositoryTests
{
    private ForumContext _forumContext = null!;
    
    [SetUp]
    public void SetUp()
    {
        DbContextOptions<ForumContext> inMemoryOptions = new DbContextOptionsBuilder<ForumContext>()
            .UseInMemoryDatabase(DateTime.Now.Ticks.ToString())
            .Options;

        _forumContext = new ForumContext(inMemoryOptions);
    }

    [TearDown]
    public void TearDown()
    {
        _forumContext.Dispose(); 
    }

    [Test]
    [TestCase("Babby", "12345678")]
    [TestCase("Pikachu", "111000999")]
    public async Task Create_WillCheckNotExistAndHashPassword_AndReturnRegisteredEntity(string username, string password)
    {
        // Arrange
        IUserRepository userRepository = Arrange_Repo(_forumContext);
        
        User input = Arrange_UserObject(username, password);

        Arrange_EnsureEmptyDb(_forumContext);

        // Act
        User actual = await Create_Act(userRepository, input);

        // Assert
        User entity = Create_Assert_EntityOfUsernameExists(_forumContext, input.Username);
        Create_Assert_InputAndResultAreSameObjects(input, entity, actual);
        Create_Assert_InputObjectHasPasswordReplaced(input, actual);
        Create_Assert_PasswordIsEncrypted(password, actual);
    }

    [Test]
    [TestCase("Babby", "12345678", "9434839589")]
    [TestCase("Pikachu", "111000999", "abcdfkoef43242")]
    public void Create_WillCheckExistence_AndThrowException(string username, string password,
        string overridePassword)
    {
        // Arrange
        IUserRepository userRepository = Arrange_Repo(_forumContext);
        User original = Arrange_UserObject(username, password);
        Arrange_EnsureExistingAsOnlyOneInDb(_forumContext, original);
        User input = Arrange_UserObject(username, overridePassword);

        // Act
        Assert.ThrowsAsync<ArgumentException>(() => Create_Act(userRepository, input));

        // Assert
        // Making it async call here is unnecessary, only makes the code complicated, hence the #pragma
        User entity = Create_Assert_EntityOfUsernameExists(_forumContext, original.Username);
        Create_Assert_EntityIsNotOverriden(original, entity, input);
    }

    [Test]
    [TestCase("Babby", "12345678")]
    [TestCase("Pikachu", "111000999")]
    public async Task Read_WillQueryDb_AndReturnFoundEntity(string username, string password)
    {
        // Arrange
        IUserRepository userRepository = Arrange_Repo(_forumContext);
        User input = Arrange_UserObject(username, password);
        Arrange_EnsureExistingAsOnlyOneInDb(_forumContext, input);
        
        // Act
        User? actual = await userRepository.Read(username);

        // Assert
        Assert.IsNotNull(actual);
        Assert.AreEqual(input.Username, actual!.Username);
        Assert.IsTrue(BCrypt.Net.BCrypt.Verify(input.Password, actual.Password));
    }

    [Test]
    [TestCase("Babby", "12345678", "Babby", "9434839589")]
    [TestCase("Pikachu", "111000999", "Pikachu", "abcdfkoef43242")]
    [TestCase("ErenYeager", "114514114514", "ErenYeager", "114514114514")]
    [TestCase("ErenYeager", "114514114514", "ErenYeager", "WrongPassword")]
    [TestCase("ErenYeager", "114514114514", "WrongName", "114514114514")]
    public async Task CheckLoginValid_WillCompareUsernameAndPassword_AndReturnValidOrNot(string username,
        string password, string loginUsername, string loginPassword)
    {
        // Arrange
        IUserRepository userRepository = Arrange_Repo(_forumContext);
        User entity = Arrange_UserObject(username, password);
        Arrange_EnsureExistingAsOnlyOneInDb(_forumContext, entity);
        User input = Arrange_UserObject(loginUsername, loginPassword);
        
        // Act
        bool actual = await userRepository.CheckLoginValid(input);

        // Assert
        Assert.AreEqual(username == loginUsername && password == loginPassword, actual);
    }

    private static void Create_Assert_EntityIsNotOverriden(User original, User entity, User input)
    {
        Assert.IsTrue(BCrypt.Net.BCrypt.Verify(original.Password, entity.Password));
        Assert.IsFalse(BCrypt.Net.BCrypt.Verify(input.Password, entity.Password));
    }

    private static void Create_Assert_InputAndResultAreSameObjects(User input, User entity, User actual)
    {
        Assert.AreEqual(input, entity);
        Assert.AreEqual(entity, actual);
        Assert.AreEqual(input, actual);
    }

    private void Arrange_EnsureExistingAsOnlyOneInDb(ForumContext forumContext, User input)
    {
        forumContext.Users.Add(new User
            { Username = input.Username, Password = BCrypt.Net.BCrypt.HashPassword(input.Password) });
        forumContext.SaveChanges();
        Assert.AreEqual(1, forumContext.Users.Count());
        User entity = forumContext.Users.First();
        Assert.AreEqual(input.Username, entity.Username);
        Assert.IsTrue(BCrypt.Net.BCrypt.Verify(input.Password, entity.Password));
    }

    private static User Arrange_UserObject(string username, string password)
    {
        User input = new()
        {
            Username = username,
            Password = password
        };
        return input;
    }

    private void Create_Assert_InputObjectHasPasswordReplaced(User input, User entity)
    {
        Assert.AreEqual(input.Password, entity.Password);
    }

    private void Create_Assert_PasswordIsEncrypted(string originalPassword, User entity)
    {
        Assert.AreNotEqual(originalPassword, entity.Password);
        Assert.IsTrue(BCrypt.Net.BCrypt.Verify(originalPassword, entity.Password));
    }

    private User Create_Assert_EntityOfUsernameExists(ForumContext forumContext, string username)
    {
        User? queried = forumContext.Users.FirstOrDefault(u => u.Username == username);
        Assert.IsNotNull(queried);
        return queried!;
    }

    private IUserRepository Arrange_Repo(ForumContext forumContext)
    {
        return new UserRepository(forumContext);
    }

    private async Task<User> Create_Act(IUserRepository userRepository, User input)
    {
        return await userRepository.Create(input);
    }

    private void Arrange_EnsureEmptyDb(ForumContext forumContext)
    {
        Assert.IsEmpty(forumContext.Users);
    }
}