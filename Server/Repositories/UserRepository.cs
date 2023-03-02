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
    /// Gets an User from db by Username as key.<br/>
    /// Note that the Username db column is case-insensitive. 
    /// </summary>
    /// <param name="key">Username</param>
    /// <returns>Entity; null if not found.</returns>
    public async Task<User?> Read(string key)
    {
        return await _forumContext.Users.FirstOrDefaultAsync(u => u.Username == key);
    }

    public Task<User> Update(string key, User obj)
    {
        throw new NotImplementedException();
    }

    public Task<User> Delete(string key)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Checks if a login credential is valid. Don't encrypt input password beforehand.
    /// </summary>
    /// <param name="model">User</param>
    /// <returns>true: password is correct</returns>
    public async Task<bool> CheckLoginValid(User model)
    {
        User? entity = await Read(model.Username);

        return entity != null && BCrypt.Net.BCrypt.Verify(model.Password, entity.Password);
    }
}