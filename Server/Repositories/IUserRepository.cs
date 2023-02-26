using DemoForum.Models.Entities;

namespace DemoForum.Repositories;

public interface IUserRepository : ICrudRepository<User, string>
{
    Task<bool> CheckLoginValid(User user);
}