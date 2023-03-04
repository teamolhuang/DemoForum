using DemoForum.Models.Entities;

namespace DemoForum.Repositories;

public interface IUserRepository : ICrudRepository<User, int>
{
    Task<(bool, User?)> CheckLoginValid(User user);

    Task<User?> ReadByUsername(string username);
}