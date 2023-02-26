using DemoForum.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DemoForum.Repositories;

public class UserRepository : IUserRepository
{
    private ForumContext _forumContext;

    public UserRepository(ForumContext forumContext)
    {
        _forumContext = forumContext;
    }

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
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> CheckLoginValid(User model)
    {
        User? entity = await Read(model.Username);

        return entity != null && BCrypt.Net.BCrypt.Verify(model.Password, entity.Password);
    }
}