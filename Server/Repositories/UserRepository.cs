using DemoForum.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DemoForum.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ForumContext _forumContext;

    public UserRepository(ForumContext forumContext)
    {
        _forumContext = forumContext;
    }

    /// <summary>
    /// Registers a new User. <br/>
    /// Checks if the username already exists, throws ArguementException when true.<br/>
    /// Creates the data row with Password encrypted.<br/>
    /// Note that the Username db column is case-insensitive.
    /// </summary>
    /// <param name="model">User</param>
    /// <returns>Created entity</returns>
    /// <exception cref="ArgumentException">If username already exists in db.</exception>
    public async Task<User> Create(User model)
    {
        // User.username is case-insensitive in DB, hence no need for cultureInfo
        bool isExist = await _forumContext.Users.AnyAsync(u => u.Username == model.Username);

        if (isExist)
            throw new ArgumentException("Username already registered");

        model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);

        await _forumContext.AddAsync(model);
        await _forumContext.SaveChangesAsync();
        return model;
    }

    /// <summary>
    /// Gets an User from db by Id as key.<br/>
    /// </summary>
    /// <param name="key">Id</param>
    /// <returns>Entity; null if not found.</returns>
    public async Task<User?> Read(int key)
    {
        return await _forumContext.Users.FirstOrDefaultAsync(u => u.Id == key);
    }
    
    /// <summary>
    /// Gets an User from db by Username as key.<br/>
    /// Note that the Username db column is case-insensitive. 
    /// </summary>
    /// <param name="username">Username</param>
    /// <returns>Entity; null if not found.</returns>
    public async Task<User?> ReadByUsername(string username)
    {
        return await _forumContext.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public Task<User> Update(int key, User obj)
    {
        throw new NotImplementedException();
    }

    public Task<User> Delete(int key)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Checks if a login credential is valid. Don't encrypt input password beforehand.
    /// </summary>
    /// <param name="model">User</param>
    /// <returns>true: password is correct</returns>
    public async Task<(bool, User?)> CheckLoginValid(User model)
    {
        User? entity = await ReadByUsername(model.Username);
        bool isValid = entity != null && BCrypt.Net.BCrypt.Verify(model.Password, entity.Password);
        return (isValid, entity);
    }
}